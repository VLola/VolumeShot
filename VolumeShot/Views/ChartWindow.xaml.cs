using ScottPlot.Plottable;
using ScottPlot;
using System.Windows;
using VolumeShot.Models;
using System.Drawing;
using System;
using System.Linq;
using System.Windows.Documents;
using System.Collections.Generic;

namespace VolumeShot.Views
{
    public partial class ChartWindow : Window
    {
        public ChartWindow(Bet bet)
        {
            InitializeComponent();
            double bufferLower = Decimal.ToDouble(bet.PriceBufferLower);
            double bufferUpper = Decimal.ToDouble(bet.PriceBufferUpper);
            double distanceLower = Decimal.ToDouble(bet.PriceDistanceLower);
            double distanceUpper = Decimal.ToDouble(bet.PriceDistanceUpper);
            double takeProfit = Decimal.ToDouble(bet.PriceTakeProfit);
            double stopLoss = Decimal.ToDouble(bet.PriceStopLoss);
            double[] xPrice = bet.Orders.Select(order=>order.DateTime.ToOADate()).ToArray();
            double[] asks = bet.Orders.Select(order=>Decimal.ToDouble(order.BestAskPrice)).ToArray();
            double[] bids = bet.Orders.Select(order=>Decimal.ToDouble(order.BestBidPrice)).ToArray();

            double[] xBuffer = { bet.Orders.ToList()[0].DateTime.ToOADate(), bet.OpenTime.ToOADate() };
            double[] xDictance = { bet.CloseTime.ToOADate(), bet.Orders.ToList()[bet.Orders.Count() - 1].DateTime.ToOADate() };
            plt.Dispatcher.Invoke(() =>
            {
                plt.Plot.AddScatter(xPrice, asks, color: Color.Red, lineWidth: 0, markerSize: 3);
                plt.Plot.AddScatter(xPrice, bids, color: Color.Green, lineWidth: 0, markerSize: 3);
                plt.Plot.AddScatterLines(xBuffer, new double[] { bufferLower, bufferLower }, Color.Gray, lineStyle: LineStyle.Dash);
                plt.Plot.AddScatterLines(xBuffer, new double[] { bufferUpper, bufferUpper }, Color.Gray, lineStyle: LineStyle.Dash);
                plt.Plot.AddScatterLines(xDictance, new double[] { distanceLower, distanceLower }, Color.Orange, lineStyle: LineStyle.Dash);
                plt.Plot.AddScatterLines(xDictance, new double[] { distanceUpper, distanceUpper }, Color.Orange, lineStyle: LineStyle.Dash);
                plt.Plot.AddScatterLines(xDictance, new double[] { takeProfit, takeProfit }, Color.Green, lineStyle: LineStyle.Dash);
                plt.Plot.AddScatterLines(xDictance, new double[] { stopLoss, stopLoss }, Color.Red, lineStyle: LineStyle.Dash);
                plt.Plot.AddPoint(bet.OpenTime.ToOADate(), Decimal.ToDouble(bet.OpenPrice), color: Color.Orange, size: 8);
                plt.Plot.AddPoint(bet.CloseTime.ToOADate(), Decimal.ToDouble(bet.ClosePrice), color: Color.DeepSkyBlue, size: 8);
                plt.Render();
            });
            //ScatterPlot scatterBufferLower = plt.Plot.AddScatterLines(xBuffer, new double[] { bufferLower, bufferLower }, Color.Gray, lineStyle: LineStyle.Dash);
            //scatterBufferLower.YAxisIndex = 1;
            //ScatterPlot scatterBufferUpper = plt.Plot.AddScatterLines(xBuffer, new double[] { bufferUpper, bufferUpper }, Color.Gray, lineStyle: LineStyle.Dash);
            //scatterBufferUpper.YAxisIndex = 1;
            //ScatterPlot scatterDistanceLower = plt.Plot.AddScatterLines(xDictance, new double[] { distanceLower, distanceLower }, Color.Orange, lineStyle: LineStyle.Dash);
            //scatterDistanceLower.YAxisIndex = 1;
            //ScatterPlot scatterDistanceUpper = plt.Plot.AddScatterLines(xDictance, new double[] { distanceUpper, distanceUpper }, Color.Orange, lineStyle: LineStyle.Dash);
            //scatterDistanceUpper.YAxisIndex = 1;
            //ScatterPlot scatterTakeProfit = plt.Plot.AddScatterLines(xDictance, new double[] { takeProfit, takeProfit }, Color.Green, lineStyle: LineStyle.Dash);
            //scatterTakeProfit.YAxisIndex = 1;
            //ScatterPlot scatterStopLoss = plt.Plot.AddScatterLines(xDictance, new double[] { stopLoss, stopLoss }, Color.Red, lineStyle: LineStyle.Dash);
            //scatterStopLoss.YAxisIndex = 1;
            //ScatterPlot scatterAsks = plt.Plot.AddScatter(xPrice, asks, color: Color.Red, lineWidth: 0, markerSize: 3);
            //scatterAsks.YAxisIndex = 1;
            //ScatterPlot scatterBids = plt.Plot.AddScatter(xPrice, bids, color: Color.Green, lineWidth: 0, markerSize: 3);
            //scatterBids.YAxisIndex = 1;
            //ScatterPlot scatter = plt.Plot.AddScatter(new double[] { bet.OpenTime.ToOADate(), bet.CloseTime.ToOADate() }, new double[] { Decimal.ToDouble(bet.OpenPrice), Decimal.ToDouble(bet.ClosePrice) }, color: Color.DeepSkyBlue, lineWidth: 0, markerSize: 8);
            //scatter.YAxisIndex = 1;

            //plt.Refresh();
        }
    }
}
