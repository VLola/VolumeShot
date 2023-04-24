namespace VolumeShot.Models
{
    public class General : Changed
    {
        private int? _requests { get; set; } = 0;
        public int? Requests
        {
            get { return _requests; }
            set
            {
                _requests = value;
                OnPropertyChanged("Requests");
            }
        }
        private double _orders { get; set; }
        public double Orders
        {
            get { return _orders; }
            set
            {
                _orders = value;
                OnPropertyChanged("Orders");
            }
        }
    }
}
