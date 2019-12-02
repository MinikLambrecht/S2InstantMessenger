using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using S2_IM_Server.Models;
using S2_IM_Server.Services;

namespace S2_IM_Server
{
    public class ChatHub : Hub<IClient>
    {
        private static readonly ConcurrentDictionary<string, User> ChatClients = new ConcurrentDictionary<string, User>();

        public override Task OnDisconnected(bool stopCalled)
        {
            var userName = ChatClients.SingleOrDefault((c) => c.Value.Id == Context.ConnectionId).Key;

            if (userName == null)
            {
                return base.OnDisconnected(stopCalled);
            }

            Clients.Others.ParticipantDisconnection(userName);
            Console.WriteLine($"<> {userName} disconnected");

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var userName = ChatClients.SingleOrDefault((c) => c.Value.Id == Context.ConnectionId).Key;

            if (userName == null)
            {
                return base.OnReconnected();
            }

            Clients.Others.ParticipantReconnection(userName);
            Console.WriteLine($"== {userName} reconnected");

            return base.OnReconnected();
        }

        public List<User> Login(string name, byte[] photo)
        {
            if (ChatClients.ContainsKey(name))
            {
                return null;
            }

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()}][{DateTime.Now.ToShortTimeString()}]: {name} logged in");

            var users = new List<User>(ChatClients.Values);
            var newUser = new User { Name = name, Id = Context.ConnectionId, Photo = photo };
            var added = ChatClients.TryAdd(name, newUser);

            if (!added) return null;
            Clients.CallerState.UserName = name;
            Clients.Others.ParticipantLogin(newUser);

            return users;
        }

        public void Logout()
        {
            var name = Clients.CallerState.UserName;

            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            ChatClients.TryRemove(name, out User _);
            Clients.Others.ParticipantLogout(name);

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()}][{DateTime.Now.ToShortTimeString()}]: {name} logged out");
        }

        public void BroadcastTextMessage(string message)
        {
            var name = Clients.CallerState.UserName;

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(message))
            {
                Clients.Others.BroadcastTextMessage(name, message);
            }
        }

        public void BroadcastImageMessage(byte[] img)
        {
            var name = Clients.CallerState.UserName;

            if (img != null)
            {
                Clients.Others.BroadcastPictureMessage(name, img);
            }
        }

        public void UnicastTextMessage(string recipient, string message)
        {
            var sender = Clients.CallerState.UserName;

            if (string.IsNullOrEmpty(sender) || recipient == sender || string.IsNullOrEmpty(message) || !ChatClients.ContainsKey(recipient ?? throw new ArgumentNullException(nameof(recipient))))
            {
                return;
            }

            ChatClients.TryGetValue(recipient ?? throw new ArgumentNullException(nameof(recipient)), out var client);

            if (client != null)
            {
                Clients.Client(client.Id).UnicastTextMessage(sender, message);
            }
        }

        public void UnicastImageMessage(string recipient, byte[] img)
        {
            var sender = Clients.CallerState.UserName;

            if (string.IsNullOrEmpty(sender) || recipient == sender || img == null || !ChatClients.ContainsKey(recipient ?? throw new ArgumentNullException(nameof(recipient))))
            {
                return;
            }

            ChatClients.TryGetValue(recipient ?? throw new ArgumentNullException(nameof(recipient)), out var client);

            if (client != null)
            {
                Clients.Client(client.Id).UnicastPictureMessage(sender, img);
            }
        }

        public void Typing(string recipient)
        {
            if (string.IsNullOrEmpty(recipient)) return;
            var sender = Clients.CallerState.UserName;

            ChatClients.TryGetValue(recipient, out var client);

            if (client != null)
            {
                Clients.Client(client.Id).ParticipantTyping(sender);
            }
        }
    }
}
