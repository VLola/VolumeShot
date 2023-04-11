using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VolumeShot.Models
{
    public class Symbol : Changed
    {
        public Exchange Exchange { get; set; }
        public List<Order> Orders { get; set; } = new();
        public OrderBook OrderBook = new();
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
                _isRun = value;
                OnPropertyChanged("IsRun");
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
    }
}
