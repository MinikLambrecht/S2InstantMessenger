using System;

namespace S2_IM_Client.Models
{
    public class ChatMessage
    {
        private string _message;

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        }

        private string _author;

        public string Author
        {
            get
            {
                return _author;
            }
            set
            {
                _author = value;
            }
        }


        private DateTime _time;

        public DateTime Time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
            }
        }


        private string _picture;

        public string Picture
        {
            get
            {
                return _picture;
            }
            set
            {
                _picture = value;
            }
        }


        private bool _isOriginNative;

        public bool IsOriginNative
        {
            get
            {
                return _isOriginNative;
            }
            set
            {
                _isOriginNative = value;
            }
        }
    }
}
