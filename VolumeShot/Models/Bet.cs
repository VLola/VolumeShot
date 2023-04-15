using System;
using System.Collections.Generic;
using VolumeShot.Command;
using VolumeShot.Views;

namespace VolumeShot.Models
{
    public class Bet : Changed
    {
        public List<SymbolPrice> SymbolPrices { get; set; } = new();
        public decimal PriceBufferLower { get; set; }
        public decimal PriceBufferUpper { get; set; }
        public decimal PriceDistanceLower { get; set; }
        public decimal PriceDistanceUpper { get; set; }
        public decimal PriceTakeProfit { get; set; }
        public decimal PriceStopLoss { get; set; }
        public decimal BufferLower { get; set; }
        public decimal BufferUpper { get; set; }
        public decimal DistanceLower { get; set; }
        public decimal DistanceUpper { get; set; }
        public decimal TakeProfit { get; set; }
        public decimal StopLoss { get; set; }
        public DateTime OpenTime { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal Volume { get; set; }
        public string Position { get; set; }

        private RelayCommand? _showCommand;
        public RelayCommand ShowCommand
        {
            get
            {
                return _showCommand ?? (_showCommand = new RelayCommand(obj => {
                    ChartWindow chartWindow = new(this);
                    chartWindow.Show();
                }));
            }
        }
        private bool _isPositive { get; set; }
        public bool IsPositive
        {
            get { return _isPositive; }
            set
            {
                _isPositive = value;
                OnPropertyChanged("IsPositive");
            }
        }
        private DateTime _closeTime { get; set; }
        public DateTime CloseTime
        {
            get { return _closeTime; }
            set
            {
                _closeTime = value;
                OnPropertyChanged("CloseTime");
            }
        }
        private decimal _closePrice { get; set; }
        public decimal ClosePrice
        {
            get { return _closePrice; }
            set
            {
                _closePrice = value;
                OnPropertyChanged("ClosePrice");
            }
        }
        private decimal _quantity { get; set; }
        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChanged("Quantity");
            }
        }
        private decimal _usdt { get; set; }
        public decimal Usdt
        {
            get { return _usdt; }
            set
            {
                _usdt = value;
                OnPropertyChanged("Usdt");
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
        private decimal _total { get; set; }
        public decimal Total
        {
            get { return _total; }
            set
            {
                _total = value;
                OnPropertyChanged("Total");
            }
        }
    }
}
