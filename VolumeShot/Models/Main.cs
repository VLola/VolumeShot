using Binance.Net.Objects.Models.Futures.Socket;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace VolumeShot.Models
{
    public class Main : Changed
    {
        public WpfPlot WpfPlot { get; set; } = new();
        public ObservableCollection<Symbol> Symbols { get; set; } = new();
        public ObservableCollection<Position> Positions { get; set; } = new();
        
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
