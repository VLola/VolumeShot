using System;
using VolumeShot.Command;
using VolumeShot.Views;

namespace VolumeShot.Models
{
    public class Symbol : Changed
    {
        public Exchange Exchange { get; set; }
        public OrderBook OrderBook = new();
        private RelayCommand? _hideCommand;
        public RelayCommand HideCommand
        {
            get
            {
                return _hideCommand ?? (_hideCommand = new RelayCommand(obj => {
                    IsVisible = false;
                }));
            }
        }
        private RelayCommand? _historyCommand;
        public RelayCommand HistoryCommand
        {
            get
            {
                return _historyCommand ?? (_historyCommand = new RelayCommand(obj => {
                    HistoryWindow historyWindow = new HistoryWindow(Exchange.Bets);
                    historyWindow.Show();
                }));
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
        private bool _isRun { get; set; }
        public bool IsRun
        {
            get { return _isRun; }
            set
            {
                if (value)
                {
                    _isRun = value;
                    OnPropertyChanged("IsRun");
                }
                else
                {
                    if (!IsTrading)
                    {
                        _isRun = value;
                        OnPropertyChanged("IsRun");
                    }
                }
            }
        }
        private bool _isTrading { get; set; }
        public bool IsTrading
        {
            get { return _isTrading; }
            set
            {
                if (value)
                {
                    if(IsRun && DistanceLower > 0m && DistanceUpper > 0m && BestAskPrice > 0m && BestBidPrice > 0m)
                    {
                        _isTrading = value;
                        OnPropertyChanged("IsTrading");
                    }
                }
                else
                {
                    _isTrading = value;
                    OnPropertyChanged("IsTrading");
                }
            }
        }
        private bool _isVisible { get; set; }
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (value)
                {
                    _isVisible = value;
                    OnPropertyChanged("IsVisible");
                }
                else
                {
                    if (!IsRun)
                    {
                        _isVisible = value;
                        OnPropertyChanged("IsVisible");
                    }
                }
            }
        }
        private decimal _volume { get; set; }
        public decimal Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                OnPropertyChanged("Volume");
                if (value < 1000000m)
                {
                    string volume = Decimal.ToInt32(value / 1000).ToString() + " k";
                    VolumeString = volume;
                }
                else
                {
                    string volume = (value / 1000000).ToString() + " M";
                    VolumeString = volume;
                }
            }
        }
        private string _volumeString { get; set; }
        public string VolumeString
        {
            get { return _volumeString; }
            set
            {
                _volumeString = value;
                OnPropertyChanged("VolumeString");
            }
        }
        private decimal _price { get; set; }
        public decimal Price
        {
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged("Price");
            }
        }
        private bool _buyerIsMaker { get; set; }
        public bool BuyerIsMaker
        {
            get { return _buyerIsMaker; }
            set
            {
                _buyerIsMaker = value;
                OnPropertyChanged("BuyerIsMaker");
            }
        }
        private DateTime _tradeTime { get; set; }
        public DateTime TradeTime
        {
            get { return _tradeTime; }
            set
            {
                _tradeTime = value;
                OnPropertyChanged("TradeTime");
            }
        }
        private decimal _bestAskPrice { get; set; }
        public decimal BestAskPrice
        {
            get { return _bestAskPrice; }
            set
            {
                _bestAskPrice = value;
                OnPropertyChanged("BestAskPrice");
            }
        }
        private decimal _bestBidPrice { get; set; }
        public decimal BestBidPrice
        {
            get { return _bestBidPrice; }
            set
            {
                _bestBidPrice = value;
                OnPropertyChanged("BestBidPrice");
            }
        }
        private DateTime _dateTime { get; set; }
        public DateTime DateTime
        {
            get { return _dateTime; }
            set
            {
                _dateTime = value;
                OnPropertyChanged("DateTime");
            }
        }
        private decimal _bestAskPriceLast { get; set; }
        public decimal BestAskPriceLast
        {
            get { return _bestAskPriceLast; }
            set
            {
                _bestAskPriceLast = value;
                OnPropertyChanged("BestAskPriceLast");
            }
        }
        private decimal _bestBidPriceLast { get; set; }
        public decimal BestBidPriceLast
        {
            get { return _bestBidPriceLast; }
            set
            {
                _bestBidPriceLast = value;
                OnPropertyChanged("BestBidPriceLast");
            }
        }
        private decimal _distanceUpper { get; set; }
        public decimal DistanceUpper
        {
            get { return _distanceUpper; }
            set
            {
                _distanceUpper = value;
                OnPropertyChanged("DistanceUpper");
            }
        }
        private decimal _distanceLower { get; set; }
        public decimal DistanceLower
        {
            get { return _distanceLower; }
            set
            {
                _distanceLower = value;
                OnPropertyChanged("DistanceLower");
            }
        }
        private decimal _bufferUpper { get; set; }
        public decimal BufferUpper
        {
            get { return _bufferUpper; }
            set
            {
                _bufferUpper = value;
                OnPropertyChanged("BufferUpper");
            }
        }
        private decimal _bufferLower { get; set; }
        public decimal BufferLower
        {
            get { return _bufferLower; }
            set
            {
                _bufferLower = value;
                OnPropertyChanged("BufferLower");
            }
        }
        private decimal _bufferUpperPrice { get; set; }
        public decimal BufferUpperPrice
        {
            get { return _bufferUpperPrice; }
            set
            {
                _bufferUpperPrice = value;
                OnPropertyChanged("BufferUpperPrice");
            }
        }
        private decimal _bufferLowerPrice { get; set; }
        public decimal BufferLowerPrice
        {
            get { return _bufferLowerPrice; }
            set
            {
                _bufferLowerPrice = value;
                OnPropertyChanged("BufferLowerPrice");
            }
        }
        private decimal _openLongOrderPrice { get; set; }
        public decimal OpenLongOrderPrice
        {
            get { return _openLongOrderPrice; }
            set
            {
                _openLongOrderPrice = value;
                OnPropertyChanged("OpenLongOrderPrice");
            }
        }
        
        private decimal _openShortOrderPrice { get; set; }
        public decimal OpenShortOrderPrice
        {
            get { return _openShortOrderPrice; }
            set
            {
                _openShortOrderPrice = value;
                OnPropertyChanged("OpenShortOrderPrice");
            }
        }
        private decimal _stopLoss { get; set; } = 0.3m;
        public decimal StopLoss
        {
            get { return _stopLoss; }
            set
            {
                _stopLoss = value;
                OnPropertyChanged("StopLoss");
            }
        }

        private decimal _takeProfit { get; set; } = 0.15m;
        public decimal TakeProfit
        {
            get { return _takeProfit; }
            set
            {
                _takeProfit = value;
                OnPropertyChanged("TakeProfit");
            }
        }
        private int _shortPlus { get; set; } = 0;
        public int ShortPlus
        {
            get { return _shortPlus; }
            set
            {
                _shortPlus = value;
                OnPropertyChanged("ShortPlus");
            }
        }
        private int _longPlus { get; set; } = 0;
        public int LongPlus
        {
            get { return _longPlus; }
            set
            {
                _longPlus = value;
                OnPropertyChanged("LongPlus");
            }
        }
        private int _shortMinus { get; set; } = 0;
        public int ShortMinus
        {
            get { return _shortMinus; }
            set
            {
                _shortMinus = value;
                OnPropertyChanged("ShortMinus");
            }
        }
        private int _longMinus { get; set; } = 0;
        public int LongMinus
        {
            get { return _longMinus; }
            set
            {
                _longMinus = value;
                OnPropertyChanged("LongMinus");
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
        private bool _isRedistance { get; set; }
        public bool IsRedistance
        {
            get { return _isRedistance; }
            set
            {
                _isRedistance = value;
                OnPropertyChanged("IsRedistance");
            }
        }
    }
}
