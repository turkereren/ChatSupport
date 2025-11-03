using System;
using System.Linq;
using System.Threading.Tasks;
using ChatSupport.Data;
using ChatSupport.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatSupport.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatDbContext _db;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ChatDbContext db, ILogger<ChatHub> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<string> StartChat(string userName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName))
                    throw new ArgumentException("Kullanıcı adı boş olamaz");

                var chatId = Guid.NewGuid().ToString("N");
                var session = new ChatSession
                {
                    ChatId = chatId,
                    UserName = userName,
                    ClaimedBySupport = false,
                    CreatedAt = DateTime.UtcNow
                };
                _db.ChatSessions.Add(session);
                await _db.SaveChangesAsync();

                await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
                await Clients.All.SendAsync("NewChatCreated");
                
                _logger.LogInformation("Yeni sohbet başlatıldı: {ChatId} - Kullanıcı: {UserName}", chatId, userName);
                return chatId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StartChat hatası: {UserName}", userName);
                throw;
            }
        }

        public async Task SendMessage(string chatId, string sender, string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(chatId) || string.IsNullOrWhiteSpace(content))
                    throw new ArgumentException("ChatId veya içerik boş olamaz");

                var msg = new ChatMessage
                {
                    ChatId = chatId,
                    Sender = sender,
                    Content = content,
                    Timestamp = DateTime.UtcNow
                };
                _db.ChatMessages.Add(msg);
                await _db.SaveChangesAsync();

                await Clients.Group(chatId)
                             .SendAsync("ReceiveMessage", chatId, sender, content);
                
                _logger.LogInformation("Mesaj gönderildi - ChatId: {ChatId}, Sender: {Sender}", chatId, sender);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendMessage hatası - ChatId: {ChatId}", chatId);
                throw;
            }
        }

        public async Task<ChatSession[]> GetOpenChats()
        {
            try
            {
                return await _db.ChatSessions
                                .Where(s => !s.ClaimedBySupport)
                                .OrderByDescending(s => s.CreatedAt)
                                .ToArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOpenChats hatası");
                throw;
            }
        }

        public async Task<ChatSession[]> GetMyChats()
        {
            try
            {
                return await _db.ChatSessions
                                .Where(s => s.ClaimedBySupport)
                                .OrderByDescending(s => s.CreatedAt)
                                .ToArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMyChats hatası");
                throw;
            }
        }

        public async Task<bool> ClaimChat(string chatId)
        {
            try
            {
                var session = await _db.ChatSessions
                                       .FirstOrDefaultAsync(s => s.ChatId == chatId);
                if (session == null || session.ClaimedBySupport) return false;

                session.ClaimedBySupport = true;
                await _db.SaveChangesAsync();

                await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
                await Clients.All.SendAsync("NewChatCreated");
                
                _logger.LogInformation("Sohbet talep edildi: {ChatId}", chatId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ClaimChat hatası - ChatId: {ChatId}", chatId);
                throw;
            }
        }

        public async Task<bool> EndChat(string chatId)
        {
            try
            {
                var session = await _db.ChatSessions
                                       .FirstOrDefaultAsync(s => s.ChatId == chatId);
                if (session == null) return false;

                _db.ChatSessions.Remove(session);
                await _db.SaveChangesAsync();

                await Clients.All.SendAsync("ChatEnded");
                
                _logger.LogInformation("Sohbet sonlandırıldı: {ChatId}", chatId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EndChat hatası - ChatId: {ChatId}", chatId);
                throw;
            }
        }

        public async Task<ChatMessage[]> GetChatHistory(string chatId)
        {
            try
            {
                return await _db.ChatMessages
                                .Where(m => m.ChatId == chatId)
                                .OrderBy(m => m.Timestamp)
                                .ToArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetChatHistory hatası - ChatId: {ChatId}", chatId);
                throw;
            }
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Bağlantı kuruldu: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Bağlantı koptu: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        // Admin veya destek ekranı aktif sohbeti görüntülerken aynı anda gruba dahil olmalı.
        // Sayfa yenilense bile canlı mesaj akışı için tekrar katılım gerekir.
        public async Task JoinChat(string chatId)
        {
            if (string.IsNullOrWhiteSpace(chatId)) return;
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
            _logger.LogInformation("Connection {Conn} joined chat {ChatId}", Context.ConnectionId, chatId);
        }

        public async Task LeaveChat(string chatId)
        {
            if (string.IsNullOrWhiteSpace(chatId)) return;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
            _logger.LogInformation("Connection {Conn} left chat {ChatId}", Context.ConnectionId, chatId);
        }
    }
}
