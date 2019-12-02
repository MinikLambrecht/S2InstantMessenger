using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using System.Drawing;
using S2_IM_Client.Services;
using S2_IM_Client.Enums;
using S2_IM_Client.Models;
using S2_IM_Client.Commands;
using System.Windows.Input;
using System.Diagnostics;
using System.Reactive.Linq;
using MySql.Data.MySqlClient;
using Pyrocorelib;
using S2_IM_Client.Views;
using MySqlHelper = Pyrocorelib.MySqlHelper;

namespace S2_IM_Client.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IChatService _chatService;
        private readonly IDialogService _dialogService;
        private readonly TaskFactory _ctxTaskFactory;

        private const int MaxImageWidth = 150;
        private const int MaxImageHeight = 150;

        private byte[] Avatar()
        {
            byte[] pic = null;

            if (!string.IsNullOrEmpty(_profilePic))
                pic = File.ReadAllBytes(_profilePic);

            return pic;
        }

        MySqlConnection _conn;

        #region Commands

        #region Properties
        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        private string _profilePic;

        public string ProfilePic
        {
            get { return _profilePic; }
            set
            {
                _profilePic = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Participant> _participants = new ObservableCollection<Participant>();

        public ObservableCollection<Participant> Participants
        {
            get { return _participants; }
            set
            {
                _participants = value;
                OnPropertyChanged();
            }
        }

        private Participant _selectedParticipant;

        public Participant SelectedParticipant
        {
            get { return _selectedParticipant; }
            set
            {
                _selectedParticipant = value;
                if (SelectedParticipant.HasSentNewMessage)
                    SelectedParticipant.HasSentNewMessage = false;
                OnPropertyChanged();
            }
        }

        private UserModes _userMode;

        public UserModes UserMode
        {
            get { return _userMode; }
            set
            {
                _userMode = value;
                OnPropertyChanged();
            }
        }

        private string _textMessage;

        public string TextMessage
        {
            get { return _textMessage; }
            set
            {
                _textMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _isConnected;

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoggedIn;

        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                _isLoggedIn = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Connect Command
        private ICommand _connectCommand;

        public ICommand ConnectCommand
        {
            get
            {
                return _connectCommand ?? (_connectCommand = new RelayCommandAsync(Connect));
            }
        }

        private async Task<bool> Connect()
        {
            try
            {
                await _chatService.ConnectAsync();
                IsConnected = true;

                return true;
            }
            catch (Exception) { return false; }
        }
        #endregion

        #region Login Command
        private ICommand _loginCommand;

        public ICommand LoginCommand 
        {
            get
            {
                return _loginCommand ?? (_loginCommand = new RelayCommandAsync(Login, (o) => CanLogin()));
            }
        }

        private async Task<bool> Login()
        {
            try
            {
                var users = await _chatService.LoginAsync(_userName, Avatar());

                if (users != null)
                {
                    _conn = new MySqlConnection(Properties.Settings.Default.ConnectionString);

                    using (var conn = MySqlHelper.NewConnection("instantmessenger"))
                    {
                        conn.Open();
                        users.ForEach(u => Participants.Add(new Participant { Name = u.Name, Photo = u.Photo }));

                        foreach (var participant in Participants)
                        {
                            var query = $"INSERT INTO `users` (User_NAME) VALUES ('{participant.Name}')";
                            MySqlHelper.AddData(query, conn);
                        }
                    }

                    UserMode = UserModes.Chat;
                    IsLoggedIn = true;

                    return true;
                }

                _dialogService.ShowNotification("Username is taken");

                return false;
            }
            catch (Exception) { return false; }
        }

        private bool CanLogin()
        {
            return !string.IsNullOrEmpty(UserName) && UserName.Length >= 2 && UserName.Length <= 20 && IsConnected;
        }
        #endregion

        #region Logout Command
        private ICommand _logoutCommand;

        public ICommand LogoutCommand
        {
            get
            {
                return _logoutCommand ?? (_logoutCommand = new RelayCommandAsync(Logout, (o) => CanLogout()));
            }
        }

        private async Task<bool> Logout()
        {
            try
            {
                await _chatService.LogoutAsync();
                UserMode = UserModes.Login;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CanLogout()
        {
            return IsConnected && IsLoggedIn;
        }
        #endregion

        #region Typing Command
        private ICommand _typingCommand;

        public ICommand TypingCommand
        {
            get
            {
                return _typingCommand ?? (_typingCommand = new RelayCommandAsync(Typing, (o) => CanUseTypingCommand()));
            }
        }

        private async Task<bool> Typing()
        {
            try
            {
                await _chatService.TypingAsync(SelectedParticipant.Name);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CanUseTypingCommand()
        {
            return SelectedParticipant != null && SelectedParticipant.IsLoggedIn;
        }
        #endregion

        #region Send Text Message Command
        private ICommand _sendTextMessageCommand;

        public ICommand SendTextMessageCommand
        {
            get
            {
                return _sendTextMessageCommand ?? (_sendTextMessageCommand = new RelayCommandAsync(SendTextMessage, (o) => CanSendTextMessage()));
            }
        }

        private async Task<bool> SendTextMessage()
        {
            try
            {
                var recipient = _selectedParticipant.Name;
                await _chatService.SendUnicastMessageAsync(recipient, _textMessage);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                var msg = new ChatMessage
                          {
                              Author = UserName,
                              Message = _textMessage,
                              Time = DateTime.Now,
                              IsOriginNative = true
                          };

                SelectedParticipant.Chatter.Add(msg);
                TextMessage = string.Empty;
            }
        }

        private bool CanSendTextMessage()
        {
            return !string.IsNullOrWhiteSpace(TextMessage) && IsConnected && _selectedParticipant != null && _selectedParticipant.IsLoggedIn;
        }
        #endregion

        #region Send Picture Message Command
        private ICommand _sendImageMessageCommand;

        public ICommand SendImageMessageCommand
        {
            get
            {
                return _sendImageMessageCommand ?? (_sendImageMessageCommand = new RelayCommandAsync(SendImageMessage, (o) => CanSendImageMessage()));
            }
        }

        private async Task<bool> SendImageMessage()
        {
            var pic = _dialogService.OpenFile("Select image file", "Images (*.jpg;*.png)|*.jpg;*.png");

            if (string.IsNullOrEmpty(pic))
                return false;

            var img = await Task.Run(() => File.ReadAllBytes(pic));

            try
            {
                var recipient = _selectedParticipant.Name;
                await _chatService.SendUnicastMessageAsync(recipient, img);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                var msg = new ChatMessage { Author = UserName, Picture = pic, Time = DateTime.Now, IsOriginNative = true };
                SelectedParticipant.Chatter.Add(msg);
            }
        }

        private bool CanSendImageMessage()
        {
            return IsConnected && _selectedParticipant != null && _selectedParticipant.IsLoggedIn;
        }
        #endregion

        #region Select Profile Picture Command
        private ICommand _selectProfilePicCommand;

        public ICommand SelectProfilePicCommand
        {
            get
            {
                return _selectProfilePicCommand ?? (_selectProfilePicCommand = new RelayCommand((o) => SelectProfilePic()));
            }
        }

        private void SelectProfilePic()
        {
            var pic = _dialogService.OpenFile("Select image file", "Images (*.jpg;*.png)|*.jpg;*.png");

            if (string.IsNullOrEmpty(pic))
            {
                return;
            }

            var img = Image.FromFile(pic);

            if (img.Width > MaxImageWidth || img.Height > MaxImageHeight)
            {
                _dialogService.ShowNotification($"Image too big, max size is {MaxImageWidth}x{MaxImageHeight} or less.");

                return;
            }

            ProfilePic = pic;
        }
        #endregion

        #region Open Image Command
        private ICommand _openImageCommand;

        public ICommand OpenImageCommand
        {
            get
            {
                return _openImageCommand ?? (_openImageCommand = new RelayCommand<ChatMessage>(OpenImage));
            }
        }

        private static void OpenImage(ChatMessage msg)
        {
            var img = msg.Picture;

            if (string.IsNullOrEmpty(img) || !File.Exists(img)) return;

            Process.Start(img);
        }
        #endregion

        #region Event Handlers
        private void NewTextMessage(string name, string msg, MessageType mt)
        {
            if (mt != MessageType.Unicast)
            {
                return;
            }

            var cm = new ChatMessage { Author = name, Message = msg, Time = DateTime.Now };
            var sender = _participants.FirstOrDefault(u => string.Equals(u.Name, name));

            _ctxTaskFactory.StartNew(() => sender?.Chatter.Add(cm)).Wait();

            if (sender != null && !(SelectedParticipant != null && sender.Name.Equals(SelectedParticipant.Name)))
            {
                _ctxTaskFactory.StartNew(() => sender.HasSentNewMessage = true).Wait();
            }
        }

        private void NewImageMessage(string name, byte[] pic, MessageType mt)
        {
            if (mt != MessageType.Unicast)
            {
                return;
            }

            var imgDirectory = Path.Combine(Environment.CurrentDirectory, "Image Messages");

            if (!Directory.Exists(imgDirectory))
                Directory.CreateDirectory(imgDirectory);

            var imgCount = Directory.EnumerateFiles(imgDirectory).Count() + 1;
            var imgPath = Path.Combine(imgDirectory, $"IMG_{imgCount}.jpg");

            var converter = new ImageConverter();

            using (var img = (Image)converter.ConvertFrom(pic))
            {
                img?.Save(imgPath);
            }

            var cm = new ChatMessage { Author = name, Picture = imgPath, Time = DateTime.Now };
            var sender = _participants.FirstOrDefault(u => string.Equals(u.Name, name));

            _ctxTaskFactory.StartNew(() => sender?.Chatter.Add(cm)).Wait();

            if (sender != null && !(SelectedParticipant != null && sender.Name.Equals(SelectedParticipant.Name)))
            {
                _ctxTaskFactory.StartNew(() => sender.HasSentNewMessage = true).Wait();
            }
        }

        private void ParticipantLogin(User u)
        {
            var ptp = Participants.FirstOrDefault(p => string.Equals(p.Name, u.Name));

            if (_isLoggedIn && ptp == null)
            {
                _ctxTaskFactory.StartNew(() => Participants.Add(new Participant
                {
                    Name = u.Name,
                    Photo = u.Photo
                })).Wait();
            }
        }

        private void ParticipantDisconnection(string name)
        {
            var person = Participants.FirstOrDefault(p => string.Equals(p.Name, name));

            if (person != null)
                person.IsLoggedIn = false;
        }

        private void ParticipantReconnection(string name)
        {
            var person = Participants.FirstOrDefault(p => string.Equals(p.Name, name));

            if (person != null)
                person.IsLoggedIn = true;
        }

        private void Reconnecting()
        {
            IsConnected = false;
            IsLoggedIn = false;
        }

        private async void Reconnected()
        {
            var pic = Avatar();

            if (!string.IsNullOrEmpty(_userName))
                await _chatService.LoginAsync(_userName, pic);

            IsConnected = true;
            IsLoggedIn = true;
        }

        private async void Disconnected()
        {
            var connectionTask = _chatService.ConnectAsync();

            await connectionTask.ContinueWith(t => {
                                                  if (t.IsFaulted)
                                                  {
                                                      return;
                                                  }

                                                  IsConnected = true;

                _chatService.LoginAsync(_userName, Avatar()).Wait();

                IsLoggedIn = true;
            });
        }

        private void ParticipantTyping(string name)
        {
            var person = Participants.FirstOrDefault(p => string.Equals(p.Name, name));

            if (person == null || person.IsTyping)
            {
                return;
            }

            person.IsTyping = true;

            Observable.Timer(TimeSpan.FromMilliseconds(1500)).Subscribe(t => person.IsTyping = false);
        }
        #endregion

        #endregion

        public MainWindowViewModel(IChatService chatSvc, IDialogService dialogSvc, MySqlConnection conn)
        {
            _dialogService = dialogSvc;
            _conn = conn;
            _chatService = chatSvc;

            chatSvc.NewTextMessage += NewTextMessage;
            chatSvc.NewImageMessage += NewImageMessage;
            chatSvc.ParticipantLoggedIn += ParticipantLogin;
            chatSvc.ParticipantLoggedOut += ParticipantDisconnection;
            chatSvc.ParticipantDisconnected += ParticipantDisconnection;
            chatSvc.ParticipantReconnected += ParticipantReconnection;
            chatSvc.ParticipantTyping += ParticipantTyping;
            chatSvc.ConnectionReconnecting += Reconnecting;
            chatSvc.ConnectionReconnected += Reconnected;
            chatSvc.ConnectionClosed += Disconnected;

            _ctxTaskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}