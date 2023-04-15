using ScottPlot;
using System.Windows;
using VolumeShot.Models;
using System.Drawing;
using System;
using System.Linq;
using System.Collections.Generic;
using ScottPlot.Drawing.Colormaps;

namespace VolumeShot.Views
{
    public partial class ChartWindow : Window
    {
        public ChartWindow(Bet bet)
        {
            InitializeComponent();
            Load(bet);
        }
        private void Load(Bet bet)
        {
            try
            {
                Chart();
                if (bet.SymbolPrices != null)
                {
                    double bufferLower = Decimal.ToDouble(bet.PriceBufferLower);
                    double bufferUpper = Decimal.ToDouble(bet.PriceBufferUpper);
                    double distanceLower = Decimal.ToDouble(bet.PriceDistanceLower);
                    double distanceUpper = Decimal.ToDouble(bet.PriceDistanceUpper);
                    double takeProfit = Decimal.ToDouble(bet.PriceTakeProfit);
                    double stopLoss = Decimal.ToDouble(bet.PriceStopLoss);
                    IEnumerable<SymbolPrice> buyers = bet.SymbolPrices.Where(price => price != null).Where(price => price.BuyerIsMaker == false);
                    IEnumerable<SymbolPrice> makers = bet.SymbolPrices.Where(price => price != null).Where(price => price.BuyerIsMaker == true);
                    double[] buyersX = buyers.Select(price => price.DateTime.ToOADate()).ToArray();
                    double[] buyersY = buyers.Select(price => Decimal.ToDouble(price.Price)).ToArray();
                    double[] makersX = makers.Select(price => price.DateTime.ToOADate()).ToArray();
                    double[] makersY = makers.Select(price => Decimal.ToDouble(price.Price)).ToArray();

                    double[] xBuffer = { bet.SymbolPrices.ToList()[0].DateTime.ToOADate(), bet.OpenTime.ToOADate() };
                    double[] xDictance = { bet.OpenTime.ToOADate(), bet.CloseTime.AddSeconds(20).ToOADate() };
                    plt.Dispatcher.Invoke(() =>
                    {
                        if (buyers.Count() > 0) plt.Plot.AddScatter(buyersX, buyersY, color: Color.Green, lineWidth: 0, markerSize: 3);
                        if (makers.Count() > 0) plt.Plot.AddScatter(makersX, makersY, color: Color.Red, lineWidth: 0, markerSize: 3);
                        plt.Plot.AddScatterLines(xDictance, new double[] { distanceUpper, distanceUpper }, Color.Orange, lineStyle: LineStyle.Dash, label: $"D ▲ : {Math.Round(bet.DistanceUpper, 2)}");
                        plt.Plot.AddScatterLines(xBuffer, new double[] { bufferUpper, bufferUpper }, Color.Gray, lineStyle: LineStyle.Dash, label: $"B ▲: {Math.Round(bet.BufferUpper, 2)}");
                        plt.Plot.AddScatterLines(xBuffer, new double[] { bufferLower, bufferLower }, Color.Gray, lineStyle: LineStyle.Dash, label: $"B ▼ : {Math.Round(bet.BufferLower, 2)}");
                        plt.Plot.AddScatterLines(xDictance, new double[] { distanceLower, distanceLower }, Color.Orange, lineStyle: LineStyle.Dash, label: $"D ▼ : {Math.Round(bet.DistanceLower, 2)}");
                        plt.Plot.AddScatterLines(xDictance, new double[] { takeProfit, takeProfit }, Color.Green, lineStyle: LineStyle.Dash, label: $"TP : {Math.Round(bet.TakeProfit, 2)}");
                        plt.Plot.AddScatterLines(xDictance, new double[] { stopLoss, stopLoss }, Color.Red, lineStyle: LineStyle.Dash, label: $"SL : {Math.Round(bet.StopLoss, 2)}");
                        plt.Plot.AddPoint(bet.OpenTime.ToOADate(), Decimal.ToDouble(bet.OpenPrice), color: Color.Orange, size: 8);
                        plt.Plot.AddPoint(bet.CloseTime.ToOADate(), Decimal.ToDouble(bet.ClosePrice), color: Color.DeepSkyBlue, size: 8);
                        plt.Render();
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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
