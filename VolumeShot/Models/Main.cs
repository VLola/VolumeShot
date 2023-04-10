using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Documents;

namespace VolumeShot.Models
{
    public class Main : Changed
    {
        public WpfPlot WpfPlot { get; set; }
        public ObservableCollection<Symbol> Symbols { get; set; } = new();
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
            if (e.PropertyName == "BestAskPrice")
            {
                if (sender != null)
                {
                    Symbol symbol = (Symbol)sender;
                    AddPoint(symbol.DateTime.ToOADate(), Decimal.ToDouble(symbol.BestAskPrice), Decimal.ToDouble(symbol.BestBidPrice));
                }
            }
        }
        public void AddPoint(double time, double priceAsk, double priceBid)
        {
            WpfPlot.Dispatcher.Invoke(new Action(() =>
            {
                WpfPlot.Plot.RenderLock();
                WpfPlot.Plot.AddPoint(x: time, y: priceAsk, color: Color.Red, size: 4);
                WpfPlot.Plot.AddPoint(x: time, y: priceBid, color: Color.Green, size: 4);
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
