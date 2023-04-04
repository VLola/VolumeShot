﻿namespace VolumeShot.Models
{
    public class Symbol : Changed
    {
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
        private decimal _percentAsk { get; set; } = 0.3m;
        public decimal PercentAsk
        {
            get { return _percentAsk; }
            set
            {
                _percentAsk = value;
                OnPropertyChanged("PercentAsk");
            }
        }
        private decimal _percentBid { get; set; } = 0.3m;
        public decimal PercentBid
        {
            get { return _percentBid; }
            set
            {
                _percentBid = value;
                OnPropertyChanged("PercentBid");
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