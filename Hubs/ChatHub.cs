using System;
using System.Linq;
using System.Threading.Tasks;
using ChatSupport.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatSupport.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatDbContext _db;
        public ChatHub(ChatDbContext db) => _db = db;

        public async Task<string> StartChat(string userName)
        {
            var chatId = Guid.NewGuid().ToString("N");
            var session = new ChatSession
            {
                ChatId = chatId,
                UserName = userName,
                ClaimedBySupport = false
            };
            _db.ChatSessions.Add(session);
            await _db.SaveChangesAsync();

            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
            await Clients.All.SendAsync("NewChatCreated");
            return chatId;
        }

        public async Task SendMessage(string chatId, string sender, string content)
        {
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
        }

        public async Task<ChatSession[]> GetOpenChats()
        {
            return await _db.ChatSessions
                            .Where(s => !s.ClaimedBySupport)
                            .ToArrayAsync();
        }

        public async Task<ChatSession[]> GetMyChats()
        {
            return await _db.ChatSessions
                            .Where(s => s.ClaimedBySupport)
                            .ToArrayAsync();
        }

        public async Task<bool> ClaimChat(string chatId)
        {
            var session = await _db.ChatSessions
                                   .FirstOrDefaultAsync(s => s.ChatId == chatId);
            if (session == null || session.ClaimedBySupport) return false;

            session.ClaimedBySupport = true;
            await _db.SaveChangesAsync();

            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
            await Clients.All.SendAsync("NewChatCreated");
            return true;
        }

        public async Task<bool> EndChat(string chatId)
        {
            var session = await _db.ChatSessions
                                   .FirstOrDefaultAsync(s => s.ChatId == chatId);
            if (session == null) return false;

            _db.ChatSessions.Remove(session);
            await _db.SaveChangesAsync();

            
            await Clients.All.SendAsync("ChatEnded");
            return true;
        }

        public async Task<ChatMessage[]> GetChatHistory(string chatId)
        {
            return await _db.ChatMessages
                            .Where(m => m.ChatId == chatId)
                            .OrderBy(m => m.Timestamp)
                            .ToArrayAsync();
        }
    }
}
