using System.Collections.ObjectModel;

namespace VolumeShot.Models
{
    public class Login : Changed
    {
        public ObservableCollection<User> Users { get; set; } = new();
        private User _selectedUser { get; set; }
        public User SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;
                OnPropertyChanged("SelectedUser");
            }
        }
        private bool _isRegisteration { get; set; }
        public bool IsRegisteration
        {
            get { return _isRegisteration; }
            set
            {
                _isRegisteration = value;
                OnPropertyChanged("IsRegisteration");
            }
        }
        private bool _isLogin { get; set; }
        public bool IsLogin
        {
            get { return _isLogin; }
            set
            {
                _isLogin = value;
                OnPropertyChanged("IsLogin");
            }
        }
        private string _name { get; set; }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        private string _apiKey { get; set; }
        public string ApiKey
        {
            get { return _apiKey; }
            set
            {
                _apiKey = value;
                OnPropertyChanged("ApiKey");
            }
        }
        private string _secretKey { get; set; }
        public string SecretKey
        {
            get { return _secretKey; }
            set
            {
                _secretKey = value;
                OnPropertyChanged("SecretKey");
            }
        }
        private bool _isTestnet { get; set; }
        public bool IsTestnet
        {
            get { return _isTestnet; }
            set
            {
                _isTestnet = value;
                OnPropertyChanged("IsTestnet");
            }
        }
        private bool _isLoading { get; set; }
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }
    }
}
