using ScottPlot;
using System.Windows;
using VolumeShot.Models;
using System.Drawing;
using System;
using System.Linq;
using System.Collections.Generic;
using ScottPlot.Plottable;
using System.Windows.Input;

namespace VolumeShot.Views
{
    public partial class ChartWindow : Window
    {
        private ScatterPlot MyScatterPlot2;
        private ScatterPlot MyScatterPlot3;
        private MarkerPlot HighlightedPoint2;
        private int LastHighlightedIndex2 = -1;
        private MarkerPlot HighlightedPoint3;
        private int LastHighlightedIndex3 = -1;
        public ChartWindow(Bet bet)
        {
            InitializeComponent();
            Load(bet);
        }


        private void FormsPlot1_AxesChanged(object sender, EventArgs e)
        {
            formsPlot2.Plot.MatchAxis(formsPlot1.Plot, horizontal: true, vertical: false);
            formsPlot2.Plot.MatchLayout(formsPlot1.Plot, horizontal: true, vertical: false);
            formsPlot2.Refresh(); 
            formsPlot3.Plot.MatchAxis(formsPlot1.Plot, horizontal: true, vertical: false);
            formsPlot3.Plot.MatchLayout(formsPlot1.Plot, horizontal: true, vertical: false);
            formsPlot3.Refresh();
        }

        private void Load(Bet bet)
        {
            try
            {
                Chart(bet.OpenPrice);

                HighlightedPoint2 = formsPlot2.Plot.AddPoint(0, 0);
                HighlightedPoint2.Color = Color.Orange;
                HighlightedPoint2.MarkerSize = 10;
                HighlightedPoint2.MarkerShape = ScottPlot.MarkerShape.openCircle;
                HighlightedPoint2.IsVisible = false;

                HighlightedPoint3 = formsPlot3.Plot.AddPoint(0, 0);
                HighlightedPoint3.Color = Color.Orange;
                HighlightedPoint3.MarkerSize = 10;
                HighlightedPoint3.MarkerShape = ScottPlot.MarkerShape.openCircle;
                HighlightedPoint3.IsVisible = false;

                DateTime startTime = bet.OpenTime.AddSeconds(-20);
                double bufferLower = Decimal.ToDouble(bet.PriceBufferLower);
                double bufferUpper = Decimal.ToDouble(bet.PriceBufferUpper);
                double distanceLower = Decimal.ToDouble(bet.PriceDistanceLower);
                double distanceUpper = Decimal.ToDouble(bet.PriceDistanceUpper);
                double takeProfit = Decimal.ToDouble(bet.PriceTakeProfit);
                double stopLoss = Decimal.ToDouble(bet.PriceStopLoss);
                IEnumerable<SymbolPrice> buyers = bet.SymbolPrices.Where(price => price != null).Where(price => price.BuyerIsMaker == false && price.DateTime >= startTime);
                IEnumerable<SymbolPrice> makers = bet.SymbolPrices.Where(price => price != null).Where(price => price.BuyerIsMaker == true && price.DateTime >= startTime);
                double[] buyersX = buyers.Select(price => price.DateTime.ToOADate()).ToArray();
                double[] buyersY = buyers.Select(price => Decimal.ToDouble(price.Price)).ToArray();
                double[] buyersQuantity = buyers.Select(price => Decimal.ToDouble(price.Quantity)).ToArray();
                double[] makersX = makers.Select(price => price.DateTime.ToOADate()).ToArray();
                double[] makersY = makers.Select(price => Decimal.ToDouble(price.Price)).ToArray();
                double[] makersQuantity = makers.Select(price => Decimal.ToDouble(price.Quantity)).ToArray();

                double[] xBuffer = { startTime.ToOADate(), bet.OpenTime.ToOADate() };
                double[] xDictance = { bet.OpenTime.ToOADate(), bet.CloseTime.AddSeconds(20).ToOADate() };

                if (buyers.Count() > 0) formsPlot1.Plot.AddScatter(buyersX, buyersY, color: Color.Green, lineWidth: 0, markerSize: 3);
                if (makers.Count() > 0) formsPlot1.Plot.AddScatter(makersX, makersY, color: Color.Red, lineWidth: 0, markerSize: 3);
                formsPlot1.Plot.AddScatterLines(xDictance, new double[] { distanceUpper, distanceUpper }, Color.Orange, lineStyle: LineStyle.Dash, label: $"D ▲ : {Math.Round(bet.DistanceUpper, 2)}");
                formsPlot1.Plot.AddScatterLines(xBuffer, new double[] { bufferUpper, bufferUpper }, Color.Gray, lineStyle: LineStyle.Dash, label: $"B ▲: {Math.Round(bet.BufferUpper, 2)}");
                formsPlot1.Plot.AddScatterLines(xBuffer, new double[] { bufferLower, bufferLower }, Color.Gray, lineStyle: LineStyle.Dash, label: $"B ▼ : {Math.Round(bet.BufferLower, 2)}");
                formsPlot1.Plot.AddScatterLines(xDictance, new double[] { distanceLower, distanceLower }, Color.Orange, lineStyle: LineStyle.Dash, label: $"D ▼ : {Math.Round(bet.DistanceLower, 2)}");
                formsPlot1.Plot.AddScatterLines(xDictance, new double[] { takeProfit, takeProfit }, Color.Green, lineStyle: LineStyle.Dash, label: $"TP : {Math.Round(bet.TakeProfit, 2)}");
                formsPlot1.Plot.AddScatterLines(xDictance, new double[] { stopLoss, stopLoss }, Color.Red, lineStyle: LineStyle.Dash, label: $"SL : {Math.Round(bet.StopLoss, 2)}");
                formsPlot1.Plot.AddPoint(bet.OpenTime.ToOADate(), Decimal.ToDouble(bet.OpenPrice), color: Color.Orange, size: 8);
                formsPlot1.Plot.AddPoint(bet.CloseTime.ToOADate(), Decimal.ToDouble(bet.ClosePrice), color: Color.DeepSkyBlue, size: 8);

                double[] buyersGroupingX = buyersX.GroupBy(p => p).Select(p => p.Key).ToArray();
                double[] buyersGroupingY = buyers.GroupBy(b => b.DateTime).Select(p => p.Sum(p => Decimal.ToDouble(p.Quantity))).ToArray();

                double[] makersGroupingX = makersX.GroupBy(p => p).Select(p => p.Key).ToArray();
                double[] makersGroupingY = makers.GroupBy(b => b.DateTime).Select(p => p.Sum(p => Decimal.ToDouble(p.Quantity))).ToArray();

                var barsBuyers = formsPlot2.Plot.AddBar(buyersGroupingY, buyersGroupingX, color: Color.Green);
                barsBuyers.BarWidth = 0.000000001;
                barsBuyers.BorderColor = Color.Green;

                var barsMakers = formsPlot2.Plot.AddBar(makersGroupingY, makersGroupingX, color: Color.Red);
                barsMakers.BarWidth = 0.000000001;
                barsMakers.BorderColor = Color.Red;

                List<double> volumeX = new();
                volumeX.AddRange(buyersGroupingX);
                volumeX.AddRange(makersGroupingX);

                List<double> volumeY = new();
                volumeY.AddRange(buyersGroupingY);
                volumeY.AddRange(makersGroupingY);

                MyScatterPlot2 = formsPlot2.Plot.AddScatterPoints(volumeX.ToArray(), volumeY.ToArray(), markerSize: 1, color: Color.Transparent);


                Color colorRed = Color.FromArgb(150, Color.Red);
                Color colorGreen = Color.FromArgb(150, Color.LightGreen);

                double[] makersGrouping3Y = makers.GroupBy(b => b.DateTime.ToString("yyyy/MM/ddThh:mm:ss.f")).Select(p => p.Sum(p => Decimal.ToDouble(p.Quantity))).ToArray();
                double[] makersGrouping3X = makers.GroupBy(b => b.DateTime.ToString("yyyy/MM/ddThh:mm:ss.f")).Select(d => DateTime.Parse(d.Key + "5").ToOADate()).ToArray();
                var barsMakers3 = formsPlot3.Plot.AddBar(makersGrouping3Y, makersGrouping3X, color: colorRed);
                barsMakers3.BarWidth = 0.000001;
                barsMakers3.BorderColor = colorRed;

                double[] buyersGrouping3Y = buyers.GroupBy(b => b.DateTime.ToString("yyyy/MM/ddThh:mm:ss.f")).Select(p => p.Sum(p => Decimal.ToDouble(p.Quantity))).ToArray();
                double[] buyersGrouping3X = buyers.GroupBy(b => b.DateTime.ToString("yyyy/MM/ddThh:mm:ss.f")).Select(d=>DateTime.Parse(d.Key+"5").ToOADate()).ToArray();
                var barsBuyers3 = formsPlot3.Plot.AddBar(buyersGrouping3Y, buyersGrouping3X, color: colorGreen);
                barsBuyers3.BarWidth = 0.000001;
                barsBuyers3.BorderColor = colorGreen;

                List<double> volume3X = new();
                volume3X.AddRange(makersGrouping3X);
                volume3X.AddRange(buyersGrouping3X);

                List<double> volume3Y = new();
                volume3Y.AddRange(makersGrouping3Y);
                volume3Y.AddRange(buyersGrouping3Y);

                MyScatterPlot3 = formsPlot3.Plot.AddScatterPoints(volume3X.ToArray(), volume3Y.ToArray(), markerSize: 1, color: Color.Transparent);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void Chart(decimal price)
        {
            string format;
            if (price > 10000m) format = "N0";
            else if (price > 100m) format = "N1";
            else if (price > 100m) format = "N2";
            else if (price > 10m) format = "N3";
            else if (price > 1m) format = "N4";
            else if (price > 0.1m) format = "N5";
            else if (price > 0.01m) format = "N6";
            else if (price > 0.001m) format = "N7";
            else format = "N8";

            formsPlot1.AxesChanged += FormsPlot1_AxesChanged;
            formsPlot1.Plot.Style(figureBackground: Color.Black, dataBackground: Color.Black);

            formsPlot1.Plot.XAxis.TickLabelStyle(color: Color.White);
            formsPlot1.Plot.XAxis.TickMarkColor(ColorTranslator.FromHtml("#333333"));
            formsPlot1.Plot.XAxis.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

            formsPlot1.Plot.YAxis.TickLabelStyle(color: Color.White);
            formsPlot1.Plot.YAxis.TickMarkColor(ColorTranslator.FromHtml("#333333"));
            formsPlot1.Plot.YAxis.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

            formsPlot1.Plot.XAxis.TickLabelFormat("ss:f", dateTimeFormat: true);
            formsPlot1.Plot.YAxis.TickLabelFormat(format, dateTimeFormat: false);

            var legend = formsPlot1.Plot.Legend();
            legend.FillColor = Color.Transparent;
            legend.OutlineColor = Color.Transparent;
            legend.Font.Color = Color.White;
            legend.Font.Bold = true;

            formsPlot2.Plot.Style(figureBackground: Color.Black, dataBackground: Color.Black);

            formsPlot2.Plot.XAxis.TickLabelStyle(color: Color.White);
            formsPlot2.Plot.XAxis.TickMarkColor(ColorTranslator.FromHtml("#333333"));
            formsPlot2.Plot.XAxis.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

            formsPlot2.Plot.YAxis.TickLabelStyle(color: Color.White);
            formsPlot2.Plot.YAxis.TickMarkColor(ColorTranslator.FromHtml("#333333"));
            formsPlot2.Plot.YAxis.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

            formsPlot2.Plot.XAxis.TickLabelFormat("ss:f", dateTimeFormat: true);
            formsPlot2.MouseMove += FormsPlot2_MouseMove;

            formsPlot3.Plot.Style(figureBackground: Color.Black, dataBackground: Color.Black);

            formsPlot3.Plot.XAxis.TickLabelStyle(color: Color.White);
            formsPlot3.Plot.XAxis.TickMarkColor(ColorTranslator.FromHtml("#333333"));
            formsPlot3.Plot.XAxis.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

            formsPlot3.Plot.YAxis.TickLabelStyle(color: Color.White);
            formsPlot3.Plot.YAxis.TickMarkColor(ColorTranslator.FromHtml("#333333"));
            formsPlot3.Plot.YAxis.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

            formsPlot3.Plot.XAxis.TickLabelFormat("ss:f", dateTimeFormat: true);

            formsPlot3.MouseMove += FormsPlot3_MouseMove;
        }

        private void FormsPlot2_MouseMove(object sender, MouseEventArgs e)
        {
            // determine point nearest the cursor
            (double mouseCoordX, double mouseCoordY) = formsPlot2.GetMouseCoordinates();
            double xyRatio = formsPlot2.Plot.XAxis.Dims.PxPerUnit / formsPlot2.Plot.YAxis.Dims.PxPerUnit;
            (double pointX, double pointY, int pointIndex) = MyScatterPlot2.GetPointNearest(mouseCoordX, mouseCoordY, xyRatio);

            // place the highlight over the point of interest
            HighlightedPoint2.X = pointX;
            HighlightedPoint2.Y = pointY;
            HighlightedPoint2.IsVisible = true;

            // render if the highlighted point chnaged
            if (LastHighlightedIndex2 != pointIndex)
            {
                LastHighlightedIndex2 = pointIndex;
                formsPlot2.Render();
            }

            // update the GUI to describe the highlighted point
            this.Title = $"Volume: {Math.Round(pointY)}";
        }
        private void FormsPlot3_MouseMove(object sender, MouseEventArgs e)
        {
            // determine point nearest the cursor
            (double mouseCoordX, double mouseCoordY) = formsPlot3.GetMouseCoordinates();
            double xyRatio = formsPlot3.Plot.XAxis.Dims.PxPerUnit / formsPlot3.Plot.YAxis.Dims.PxPerUnit;
            (double pointX, double pointY, int pointIndex) = MyScatterPlot3.GetPointNearest(mouseCoordX, mouseCoordY, xyRatio);

            // place the highlight over the point of interest
            HighlightedPoint3.X = pointX;
            HighlightedPoint3.Y = pointY;
            HighlightedPoint3.IsVisible = true;

            // render if the highlighted point chnaged
            if (LastHighlightedIndex3 != pointIndex)
            {
                LastHighlightedIndex3 = pointIndex;
                formsPlot3.Render();
            }

            // update the GUI to describe the highlighted point
            this.Title = $"Volume: {Math.Round(pointY)}";
        }
    }
}
