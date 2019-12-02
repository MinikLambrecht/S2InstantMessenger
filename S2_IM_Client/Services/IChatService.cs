using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using S2_IM_Client.Models;
using S2_IM_Client.Enums;

namespace S2_IM_Client.Services
{
    public interface IChatService
    {
        event Action<User> ParticipantLoggedIn;

        event Action<string> ParticipantLoggedOut;
        event Action<string> ParticipantDisconnected;
        event Action<string> ParticipantReconnected;

        event Action ConnectionReconnecting;
        event Action ConnectionReconnected;
        event Action ConnectionClosed;

        event Action<string, string, MessageType> NewTextMessage;
        event Action<string, byte[], MessageType> NewImageMessage;

        event Action<string> ParticipantTyping;

        Task ConnectAsync();
        Task<List<User>> LoginAsync(string name, byte[] photo);
        Task LogoutAsync();

        Task SendBroadcastMessageAsync(string msg);
        Task SendBroadcastMessageAsync(byte[] img);
        Task SendUnicastMessageAsync(string recipient, string msg);
        Task SendUnicastMessageAsync(string recipient, byte[] img);
        Task TypingAsync(string recipient);
    }
}