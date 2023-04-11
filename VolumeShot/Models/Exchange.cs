using Binance.Net.Objects.Models.Futures;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace VolumeShot.Models
{
    public class Exchange : Changed
    {
        public Exchange(BinanceFuturesUsdtSymbol binanceFuturesUsdtSymbol)
        {
            Symbol = binanceFuturesUsdtSymbol.Name;
            MinQuantity = binanceFuturesUsdtSymbol.LotSizeFilter.MinQuantity;
            StepSize = binanceFuturesUsdtSymbol.LotSizeFilter.StepSize;
            TickSize = binanceFuturesUsdtSymbol.PriceFilter.TickSize;
        }
        public ObservableCollection<Bet> Bets { get; set; } = new();
        public List<Order> OpenBetOrders { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
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
        private decimal _takeProfitLong { get; set; }
        public decimal TakeProfitLong
        {
            get { return _takeProfitLong; }
            set
            {
                _takeProfitLong = value;
                OnPropertyChanged("TakeProfitLong");
            }
        }
        private decimal _takeProfitShort { get; set; }
        public decimal TakeProfitShort
        {
            get { return _takeProfitShort; }
            set
            {
                _takeProfitShort = value;
                OnPropertyChanged("TakeProfitShort");
            }
        }
        private decimal _stopLossLong { get; set; }
        public decimal StopLossLong
        {
            get { return _stopLossLong; }
            set
            {
                _stopLossLong = value;
                OnPropertyChanged("StopLossLong");
            }
        }
        private decimal _stopLossShort { get; set; }
        public decimal StopLossShort
        {
            get { return _stopLossShort; }
            set
            {
                _stopLossShort = value;
                OnPropertyChanged("StopLossShort");
            }
        }
        private decimal _takeProfitLongPrice { get; set; }
        public decimal TakeProfitLongPrice
        {
            get { return _takeProfitLongPrice; }
            set
            {
                _takeProfitLongPrice = value;
                OnPropertyChanged("TakeProfitLongPrice");
            }
        }
        private decimal _takeProfitShortPrice { get; set; }
        public decimal TakeProfitShortPrice
        {
            get { return _takeProfitShortPrice; }
            set
            {
                _takeProfitShortPrice = value;
                OnPropertyChanged("TakeProfitShortPrice");
            }
        }
        private decimal _stopLossLongPrice { get; set; }
        public decimal StopLossLongPrice
        {
            get { return _stopLossLongPrice; }
            set
            {
                _stopLossLongPrice = value;
                OnPropertyChanged("StopLossLongPrice");
            }
        }
        private decimal _stopLossShortPrice { get; set; }
        public decimal StopLossShortPrice
        {
            get { return _stopLossShortPrice; }
            set
            {
                _stopLossShortPrice = value;
                OnPropertyChanged("StopLossShortPrice");
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
        private decimal _distanceUpperPrice { get; set; }
        public decimal DistanceUpperPrice
        {
            get { return _distanceUpperPrice; }
            set
            {
                _distanceUpperPrice = value;
                OnPropertyChanged("DistanceUpperPrice");
            }
        }
        private decimal _distanceLowerPrice { get; set; }
        public decimal DistanceLowerPrice
        {
            get { return _distanceLowerPrice; }
            set
            {
                _distanceLowerPrice = value;
                OnPropertyChanged("DistanceLowerPrice");
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
        private decimal _denominatorTakeProfit { get; set; } = 3m;
        public decimal DenominatorTakeProfit
        {
            get { return _denominatorTakeProfit; }
            set
            {
                _denominatorTakeProfit = value;
                OnPropertyChanged("DenominatorTakeProfit");
            }
        }
        private decimal _denominatorStopLoss { get; set; } = 2m;
        public decimal DenominatorStopLoss
        {
            get { return _denominatorStopLoss; }
            set
            {
                _denominatorStopLoss = value;
                OnPropertyChanged("DenominatorStopLoss");
            }
        }
        private decimal _fee { get; set; }
        public decimal Fee
        {
            get { return _fee; }
            set
            {
                _fee = value;
                OnPropertyChanged("Fee");
            }
        }
        private decimal _profit { get; set; }
        public decimal Profit
        {
            get { return _profit; }
            set
            {
                _profit = value;
                OnPropertyChanged("Profit");
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
            }
        }
    }
}
