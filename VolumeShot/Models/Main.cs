using ScottPlot;
using System;
using System.Collections.ObjectModel;
using System.Drawing;

namespace VolumeShot.Models
{
    public class Main : Changed
    {
        private string _version { get; set; }
        public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
                OnPropertyChanged("Version");
                Title = $"Version: {_version} - User: {_loginUser}";
            }
        }
        private string _loginUser { get; set; } = "null";
        public string LoginUser
        {
            get { return _loginUser; }
            set
            {
                _loginUser = value;
                OnPropertyChanged("LoginUser");
                Title = $"Version: {_version} - User: {_loginUser}";
            }
        }
        private string _title { get; set; }
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }
        public WpfPlot WpfPlot { get; set; } = new();
        public ObservableCollection<Symbol> FullSymbols { get; set; } = new();
        public ObservableCollection<Symbol> Symbols { get; set; } = new();
        public ObservableCollection<Position> Positions { get; set; } = new();
        public ObservableCollection<Order> Orders { get; set; } = new();
        public ObservableCollection<Bet> Bets { get; set; } = new();
        private Symbol _selectedFullSymbol { get; set; }
        public Symbol SelectedFullSymbol
        {
            get { return _selectedFullSymbol; }
            set
            {
                _selectedFullSymbol = value;
                OnPropertyChanged("SelectedFullSymbol");
            }
        }
        private Symbol _selectedSymbol { get; set; }
        public Symbol SelectedSymbol
        {
            get { return _selectedSymbol; }
            set
            {
                value.PropertyChanged += Value_PropertyChanged;
                if(_selectedSymbol != null) _selectedSymbol.PropertyChanged -= Value_PropertyChanged;
                _selectedSymbol = value;
                OnPropertyChanged("SelectedSymbol");
            }
        }
        private bool _isVisibleChart { get; set; }
        public bool IsVisibleChart
        {
            get { return _isVisibleChart; }
            set
            {
                _isVisibleChart = value;
                OnPropertyChanged("IsVisibleChart");
            }
        }
        private decimal _balance { get; set; }
        public decimal Balance
        {
            get { return _balance; }
            set
            {
                _balance = value;
                OnPropertyChanged("Balance");
            }
        }
        private decimal _totalHistory { get; set; }
        public decimal TotalHistory
        {
            get { return _totalHistory; }
            set
            {
                _totalHistory = value;
                OnPropertyChanged("TotalHistory");
                if (value >= 0m) IsPositiveTotalHistory = true;
                else IsPositiveTotalHistory = false;
            }
        }
        private bool _isPositiveTotalHistory { get; set; } = true;
        public bool IsPositiveTotalHistory
        {
            get { return _isPositiveTotalHistory; }
            set
            {
                _isPositiveTotalHistory = value;
                OnPropertyChanged("IsPositiveTotalHistory");
            }
        }
        private long _ping { get; set; }
        public long Ping
        {
            get { return _ping; }
            set
            {
                _ping = value;
                OnPropertyChanged("Ping");
            }
        }
        private long _pingMax { get; set; } = 1000;
        public long PingMax
        {
            get { return _pingMax; }
            set
            {
                _pingMax = value;
                OnPropertyChanged("PingMax");
            }
        }
        private bool _isStopTrading { get; set; }
        public bool IsStopTrading
        {
            get { return _isStopTrading; }
            set
            {
                _isStopTrading = value;
                OnPropertyChanged("IsStopTrading");
            }
        }
        private void Value_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Price")
            {
                if (sender != null)
                {
                    Symbol symbol = (Symbol)sender;
                    if(symbol.BuyerIsMaker) AddPoint(symbol.TradeTime.ToOADate(), Decimal.ToDouble(symbol.Price), Color.Red);
                    else AddPoint(symbol.TradeTime.ToOADate(), Decimal.ToDouble(symbol.Price), Color.Green);
                }
            }
        }
        public void AddPoint(double time, double price, Color color)
        {
            WpfPlot.Dispatcher.Invoke(new Action(() =>
            {
                WpfPlot.Plot.RenderLock();
                WpfPlot.Plot.AddPoint(x: time, y: price, color: color, size: 4);
                WpfPlot.Plot.RenderUnlock();
            }));
        }

        public double[] x { get; set; } = new double[2];
        public double[] bufferLower { get; set; } = new double[2];
        public double[] bufferUpper { get; set; } = new double[2];
        public double[] distanceLower { get; set; } = new double[2];
        public double[] distanceUpper { get; set; } = new double[2];
        public double[] takeProfit { get; set; } = new double[2];
        public double[] stopLoss { get; set; } = new double[2];
    }
}
