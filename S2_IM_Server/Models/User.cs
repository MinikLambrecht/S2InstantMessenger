namespace S2_IM_Server.Models
{
    public class User
    {
        private string _name;

        public string Name 
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        private string _id;

        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        private byte[] _photo;

        public byte[] Photo 
        {
            get
            {
                return _photo;
            }
            set
            {
                _photo = value;
            }
        }
    }
}
