using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using S2_IM_Client.Enums;
using S2_IM_Client.Models;
using System.Net;
using Microsoft.AspNet.SignalR.Client;

namespace S2_IM_Client.Services
{
    public class ChatService : IChatService
    {
        private const string Url = "http://localhost:8080/signalchat";

        public event Action<string, string, MessageType> NewTextMessage;
        public event Action<string, byte[], MessageType> NewImageMessage;

        public event Action<User> ParticipantLoggedIn;

        public event Action<string> ParticipantDisconnected;
        public event Action<string> ParticipantLoggedOut;
        public event Action<string> ParticipantReconnected;
        public event Action<string> ParticipantTyping;

        public event Action ConnectionReconnecting;
        public event Action ConnectionReconnected;
        public event Action ConnectionClosed;

        private IHubProxy _hubProxy;
        private HubConnection _connection;

        public async Task ConnectAsync()
        {
            _connection = new HubConnection(Url);

            _hubProxy = _connection.CreateHubProxy("ChatHub");

            _hubProxy.On<User>("ParticipantLogin", (u) => ParticipantLoggedIn?.Invoke(u));

            _hubProxy.On<string>("ParticipantDisconnection", (n) => ParticipantDisconnected?.Invoke(n));
            _hubProxy.On<string>("ParticipantLogout", (n) => ParticipantLoggedOut?.Invoke(n));
            _hubProxy.On<string>("ParticipantReconnection", (n) => ParticipantReconnected?.Invoke(n));
            _hubProxy.On<string>("ParticipantTyping", (p) => ParticipantTyping?.Invoke(p));

            _hubProxy.On<string, string>("BroadcastTextMessage", (n, m) => NewTextMessage?.Invoke(n, m, MessageType.Broadcast));
            _hubProxy.On<string, byte[]>("BroadcastPictureMessage", (n, m) => NewImageMessage?.Invoke(n, m, MessageType.Broadcast));
            _hubProxy.On<string, string>("UnicastTextMessage", (n, m) => NewTextMessage?.Invoke(n, m, MessageType.Unicast));
            _hubProxy.On<string, byte[]>("UnicastPictureMessage", (n, m) => NewImageMessage?.Invoke(n, m, MessageType.Unicast));

            _connection.Reconnecting += Reconnecting;
            _connection.Reconnected += Reconnected;
            _connection.Closed += Disconnected;

            ServicePointManager.DefaultConnectionLimit = 30;

            await _connection.Start();
        }

        private void Disconnected()
        {
            ConnectionClosed?.Invoke();
        }

        private void Reconnected()
        {
            ConnectionReconnected?.Invoke();
        }

        private void Reconnecting()
        {
            ConnectionReconnecting?.Invoke();
        }

        public async Task<List<User>> LoginAsync(string name, byte[] photo)
        {
            return await _hubProxy.Invoke<List<User>>("Login", name, photo);
        }

        public async Task LogoutAsync()
        {
            await _hubProxy.Invoke("Logout");
        }

        public async Task SendBroadcastMessageAsync(string msg)
        {
            await _hubProxy.Invoke("BroadcastTextMessage", msg);
        }

        public async Task SendBroadcastMessageAsync(byte[] img)
        {
            await _hubProxy.Invoke("BroadcastImageMessage", img);
        }

        public async Task SendUnicastMessageAsync(string recipient, string msg)
        {
            await _hubProxy.Invoke("UnicastTextMessage", recipient, msg);
        }

        public async Task SendUnicastMessageAsync(string recipient, byte[] img)
        {
            await _hubProxy.Invoke("UnicastImageMessage", recipient, img);
        }

        public async Task TypingAsync(string recipient)
        {
            await _hubProxy.Invoke("Typing", recipient);
        }
    }
}