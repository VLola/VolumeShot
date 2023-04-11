using ScottPlot;
using System.Windows;
using VolumeShot.Models;
using System.Drawing;
using System;
using System.Linq;

namespace VolumeShot.Views
{
    public partial class ChartWindow : Window
    {
        public ChartWindow(Bet bet)
        {
            InitializeComponent();
            Chart();
            if(bet.Orders != null)
            {
                double bufferLower = Decimal.ToDouble(bet.PriceBufferLower);
                double bufferUpper = Decimal.ToDouble(bet.PriceBufferUpper);
                double distanceLower = Decimal.ToDouble(bet.PriceDistanceLower);
                double distanceUpper = Decimal.ToDouble(bet.PriceDistanceUpper);
                double takeProfit = Decimal.ToDouble(bet.PriceTakeProfit);
                double stopLoss = Decimal.ToDouble(bet.PriceStopLoss);
                double[] xPrice = bet.Orders.Where(order=>order != null).Select(order => order.DateTime.ToOADate()).ToArray();
                double[] asks = bet.Orders.Where(order => order != null).Select(order => Decimal.ToDouble(order.BestAskPrice)).ToArray();
                double[] bids = bet.Orders.Where(order => order != null).Select(order => Decimal.ToDouble(order.BestBidPrice)).ToArray();

                double[] xBuffer = { bet.Orders.ToList()[0].DateTime.ToOADate(), bet.OpenTime.ToOADate() };
                double[] xDictance = { bet.OpenTime.ToOADate(), bet.CloseTime.ToOADate() };
                plt.Dispatcher.Invoke(() =>
                {
                    plt.Plot.AddScatter(xPrice, asks, color: Color.Red, lineWidth: 0, markerSize: 3);
                    plt.Plot.AddScatter(xPrice, bids, color: Color.Green, lineWidth: 0, markerSize: 3);
                    plt.Plot.AddScatterLines(xBuffer, new double[] { bufferLower, bufferLower }, Color.Gray, lineStyle: LineStyle.Dash, label: $"BL: {Math.Round(bet.BufferLower, 2)}");
                    plt.Plot.AddScatterLines(xBuffer, new double[] { bufferUpper, bufferUpper }, Color.Gray, lineStyle: LineStyle.Dash, label: $"BU: {Math.Round(bet.BufferUpper, 2)}");
                    plt.Plot.AddScatterLines(xDictance, new double[] { distanceLower, distanceLower }, Color.Orange, lineStyle: LineStyle.Dash, label: $"DL: {Math.Round(bet.DistanceLower, 2)}");
                    plt.Plot.AddScatterLines(xDictance, new double[] { distanceUpper, distanceUpper }, Color.Orange, lineStyle: LineStyle.Dash, label: $"DU: {Math.Round(bet.DistanceUpper, 2)}");
                    plt.Plot.AddScatterLines(xDictance, new double[] { takeProfit, takeProfit }, Color.Green, lineStyle: LineStyle.Dash, label: $"TP: {Math.Round(bet.TakeProfit, 2)}");
                    plt.Plot.AddScatterLines(xDictance, new double[] { stopLoss, stopLoss }, Color.Red, lineStyle: LineStyle.Dash, label: $"SL: {Math.Round(bet.StopLoss, 2)}");
                    plt.Plot.AddPoint(bet.OpenTime.ToOADate(), Decimal.ToDouble(bet.OpenPrice), color: Color.Orange, size: 8);
                    plt.Plot.AddPoint(bet.CloseTime.ToOADate(), Decimal.ToDouble(bet.ClosePrice), color: Color.DeepSkyBlue, size: 8);
                    plt.Render();
                });
            }
            
        }
        private void Chart()
        {
            plt.Plot.Layout(padding: 12);
            plt.Plot.Style(figureBackground: Color.Black, dataBackground: Color.Black);
            plt.Plot.Frameless();
            plt.Plot.XAxis.TickLabelStyle(color: Color.White);
            plt.Plot.XAxis.TickMarkColor(ColorTranslator.FromHtml("#333333"));
            plt.Plot.XAxis.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

            plt.Plot.YAxis.Ticks(false);
            plt.Plot.YAxis.Grid(false);
            plt.Plot.YAxis2.Ticks(true);
            plt.Plot.YAxis2.Grid(true);
            plt.Plot.YAxis2.TickLabelStyle(color: ColorTranslator.FromHtml("#00FF00"));
            plt.Plot.YAxis2.TickMarkColor(ColorTranslator.FromHtml("#333333"));
            plt.Plot.YAxis2.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

            var legend = plt.Plot.Legend();
            legend.FillColor = Color.Transparent;
            legend.OutlineColor = Color.Transparent;
            legend.Font.Color = Color.White;
            legend.Font.Bold = true;
        }
    }
}
