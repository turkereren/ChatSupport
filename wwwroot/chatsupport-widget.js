/**
 * ChatSupport Widget - Web sitelerine entegre edilebilir canlı destek widget'ı
 * Kullanım: <script src="YOUR_API_URL/chatsupport-widget.js" data-api-url="YOUR_API_URL"></script>
 */

(function() {
    'use strict';

    // API URL'yi script tag'inden al
    const scriptTag = document.currentScript;
    const API_URL = scriptTag.getAttribute('data-api-url') || window.location.origin;
    const WIDGET_POSITION = scriptTag.getAttribute('data-position') || 'bottom-right'; // bottom-right, bottom-left
    const PRIMARY_COLOR = scriptTag.getAttribute('data-color') || '#ffc107';

    // Widget HTML'ini oluştur
    const widgetHTML = `
        <style>
            #chat-support-widget * {
                margin: 0;
                padding: 0;
                box-sizing: border-box;
                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            }
            
            #chat-support-button {
                position: fixed;
                ${WIDGET_POSITION.includes('right') ? 'right: 20px;' : 'left: 20px;'}
                bottom: 20px;
                width: 60px;
                height: 60px;
                border-radius: 50%;
                background: ${PRIMARY_COLOR};
                border: none;
                cursor: pointer;
                box-shadow: 0 4px 12px rgba(0,0,0,0.3);
                z-index: 9998;
                display: flex;
                align-items: center;
                justify-content: center;
                transition: transform 0.2s;
            }
            
            #chat-support-button:hover {
                transform: scale(1.1);
            }
            
            #chat-support-button svg {
                width: 30px;
                height: 30px;
                fill: white;
            }
            
            #chat-support-panel {
                position: fixed;
                ${WIDGET_POSITION.includes('right') ? 'right: 20px;' : 'left: 20px;'}
                bottom: 90px;
                width: 360px;
                max-height: 550px;
                background: #fff;
                border: 1px solid #ddd;
                border-radius: 12px;
                display: none;
                flex-direction: column;
                box-shadow: 0 8px 24px rgba(0,0,0,0.3);
                z-index: 9999;
                overflow: hidden;
            }
            
            #chat-support-panel.active {
                display: flex;
            }
            
            #chat-header {
                background: linear-gradient(135deg, ${PRIMARY_COLOR}, ${PRIMARY_COLOR}dd);
                padding: 15px;
                display: flex;
                justify-content: space-between;
                align-items: center;
                color: #333;
                font-weight: 600;
            }
            
            #chat-close {
                background: none;
                border: none;
                font-size: 20px;
                cursor: pointer;
                color: #333;
            }
            
            #chat-messages {
                flex: 1;
                overflow-y: auto;
                padding: 15px;
                background: #f9f9f9;
                min-height: 300px;
                max-height: 400px;
            }
            
            .chat-message {
                margin: 8px 0;
                padding: 10px 12px;
                background: #fff;
                border-radius: 8px;
                box-shadow: 0 1px 2px rgba(0,0,0,0.1);
                word-wrap: break-word;
            }
            
            .chat-message.user {
                background: ${PRIMARY_COLOR}33;
                margin-left: 20px;
            }
            
            .chat-message.support {
                background: #e3f2fd;
                margin-right: 20px;
            }
            
            .message-sender {
                font-weight: 600;
                font-size: 12px;
                margin-bottom: 4px;
                color: #555;
            }
            
            .message-content {
                font-size: 14px;
                color: #333;
            }
            
            #chat-footer {
                padding: 12px;
                border-top: 1px solid #ddd;
                background: #fff;
            }
            
            #chat-username, #chat-input {
                width: 100%;
                padding: 10px;
                border: 1px solid #ddd;
                border-radius: 6px;
                font-size: 14px;
                margin-bottom: 8px;
            }
            
            #chat-username:disabled, #chat-input:disabled {
                background: #f5f5f5;
            }
            
            #chat-send {
                width: 100%;
                padding: 10px;
                background: ${PRIMARY_COLOR};
                border: none;
                border-radius: 6px;
                cursor: pointer;
                font-weight: 600;
                font-size: 14px;
                color: #333;
            }
            
            #chat-send:disabled {
                background: #ccc;
                cursor: not-allowed;
            }
            
            #chat-send:hover:not(:disabled) {
                opacity: 0.9;
            }
            
            @media (max-width: 480px) {
                #chat-support-panel {
                    width: calc(100vw - 40px);
                    max-height: 70vh;
                }
            }
        </style>
        
        <div id="chat-support-widget">
            <button id="chat-support-button" aria-label="Canlı Destek">
                <svg viewBox="0 0 24 24">
                    <path d="M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 14H6l-2 2V4h16v12z"/>
                </svg>
            </button>
            
            <div id="chat-support-panel">
                <div id="chat-header">
                    <span>Canlı Destek</span>
                    <button id="chat-close">✕</button>
                </div>
                
                <div id="chat-messages"></div>
                
                <div id="chat-footer">
                    <input type="text" id="chat-username" placeholder="Adınızı girin..." />
                    <input type="text" id="chat-input" placeholder="Mesajınızı yazın..." disabled />
                    <button id="chat-send" disabled>Gönder</button>
                </div>
            </div>
        </div>
    `;

    // Widget'ı sayfaya ekle
    function initWidget() {
        const container = document.createElement('div');
        container.innerHTML = widgetHTML;
        document.body.appendChild(container);

        const button = document.getElementById('chat-support-button');
        const panel = document.getElementById('chat-support-panel');
        const closeBtn = document.getElementById('chat-close');
        const messagesDiv = document.getElementById('chat-messages');
        const usernameInput = document.getElementById('chat-username');
        const messageInput = document.getElementById('chat-input');
        const sendBtn = document.getElementById('chat-send');

        let connection = null;
        let chatId = null;
        let isStarted = false;

        // Mesaj ekle
        function addMessage(sender, content, type = 'system') {
            const msgDiv = document.createElement('div');
            msgDiv.className = `chat-message ${type}`;
            
            const senderDiv = document.createElement('div');
            senderDiv.className = 'message-sender';
            senderDiv.textContent = sender;
            
            const contentDiv = document.createElement('div');
            contentDiv.className = 'message-content';
            contentDiv.textContent = content;
            
            msgDiv.appendChild(senderDiv);
            msgDiv.appendChild(contentDiv);
            messagesDiv.appendChild(msgDiv);
            messagesDiv.scrollTop = messagesDiv.scrollHeight;
        }

        // SignalR bağlantısı
        async function initConnection() {
            try {
                connection = new signalR.HubConnectionBuilder()
                    .withUrl(`${API_URL}/chatHub`)
                    .withAutomaticReconnect()
                    .build();

                connection.on("ReceiveMessage", (cid, sender, content) => {
                    if (cid === chatId) {
                        const type = sender === 'Destek' ? 'support' : 'user';
                        addMessage(sender, content, type);
                    }
                });

                connection.onreconnecting(() => {
                    addMessage("Sistem", "Bağlantı yeniden kuruluyor...", "system");
                });

                connection.onreconnected(() => {
                    addMessage("Sistem", "Bağlantı yeniden kuruldu.", "system");
                });

                await connection.start();
                addMessage("Sistem", "Merhaba! Size nasıl yardımcı olabiliriz?", "system");
            } catch (err) {
                console.error("ChatSupport bağlantı hatası:", err);
                addMessage("Sistem", "Bağlantı kurulamadı. Lütfen sayfayı yenileyin.", "system");
            }
        }

        // Sohbet başlat
        async function startChat() {
            const username = usernameInput.value.trim();
            if (!username) {
                alert("Lütfen adınızı girin");
                return;
            }

            try {
                chatId = await connection.invoke("StartChat", username);
                isStarted = true;
                usernameInput.disabled = true;
                messageInput.disabled = false;
                sendBtn.disabled = false;
                messageInput.focus();
                addMessage("Sistem", "Sohbet başlatıldı. Bir destek temsilcisi en kısa sürede size yardımcı olacak.", "system");
            } catch (err) {
                console.error("StartChat hatası:", err);
                alert("Sohbet başlatılamadı. Lütfen tekrar deneyin.");
            }
        }

        // Mesaj gönder
        async function sendMessage() {
            const text = messageInput.value.trim();
            if (!text) return;

            try {
                await connection.invoke("SendMessage", chatId, usernameInput.value.trim(), text);
                messageInput.value = "";
            } catch (err) {
                console.error("SendMessage hatası:", err);
                addMessage("Sistem", "Mesaj gönderilemedi. Lütfen tekrar deneyin.", "system");
            }
        }

        // Event listeners
        button.addEventListener('click', () => {
            panel.classList.toggle('active');
            if (panel.classList.contains('active') && !connection) {
                initConnection();
            }
        });

        closeBtn.addEventListener('click', () => {
            panel.classList.remove('active');
        });

        usernameInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && !isStarted) {
                e.preventDefault();
                startChat();
            }
        });

        messageInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                sendMessage();
            }
        });

        sendBtn.addEventListener('click', sendMessage);
    }

    // Sayfa yüklendiğinde widget'ı başlat
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initWidget);
    } else {
        initWidget();
    }
})();

