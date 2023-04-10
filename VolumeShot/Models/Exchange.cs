using Binance.Net.Objects.Models.Futures;

namespace VolumeShot.Models
{
    internal class Exchange : Changed
    {
        public Exchange(BinanceFuturesUsdtSymbol binanceFuturesUsdtSymbol)
        {
            Symbol = binanceFuturesUsdtSymbol.Name;
            MinQuantity = binanceFuturesUsdtSymbol.LotSizeFilter.MinQuantity;
            StepSize = binanceFuturesUsdtSymbol.LotSizeFilter.StepSize;
            TickSize = binanceFuturesUsdtSymbol.PriceFilter.TickSize;
        }
        public string Symbol { get; set; }
        private decimal _minQuantity { get; set; }
        public decimal MinQuantity
        {
            get { return _minQuantity; }
            set
            {
                _minQuantity = value;
                OnPropertyChanged("MinQuantity");
            }
        }
        private decimal _stepSize { get; set; }
        public decimal StepSize
        {
            get { return _stepSize; }
            set
            {
                _stepSize = value;
                OnPropertyChanged("StepSize");
            }
        }
        private decimal _tickSize { get; set; }
        public decimal TickSize
        {
            get { return _tickSize; }
            set
            {
                _tickSize = value;
                OnPropertyChanged("TickSize");
                int index = value.ToString().IndexOf("1");
                if (index == 0) RoundPrice = index;
                else RoundPrice = index - 1;
            }
        }
        private int _roundPrice { get; set; }
        public int RoundPrice
        {
            get { return _roundPrice; }
            set
            {
                _roundPrice = value;
                OnPropertyChanged("RoundPrice");
            }
        }
        private decimal _usdt { get; set; } = 11m;
        public decimal Usdt
        {
            get { return _usdt; }
            set
            {
                _usdt = value;
                OnPropertyChanged("Usdt");
            }
        }
        private bool _isOpenLongOrder { get; set; }
        public bool IsOpenLongOrder
        {
            get { return _isOpenLongOrder; }
            set
            {
                _isOpenLongOrder = value;
                OnPropertyChanged("IsOpenLongOrder");
            }
        }
        private bool _isOpenShortOrder { get; set; }
        public bool IsOpenShortOrder
        {
            get { return _isOpenShortOrder; }
            set
            {
                _isOpenShortOrder = value;
                OnPropertyChanged("IsOpenShortOrder");
            }
        }
    }
}
