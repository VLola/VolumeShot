﻿using ScottPlot;
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
        private readonly ScottPlot.Plottable.ScatterPlot MyScatterPlot;
        private readonly ScottPlot.Plottable.MarkerPlot HighlightedPoint;
        private int LastHighlightedIndex = -1;
        public ChartWindow(Bet bet)
        {
            InitializeComponent();
            //Load(bet);



            // Add a red circle we can move around later as a highlighted point indicator
            HighlightedPoint = formsPlot2.Plot.AddPoint(0, 0);
            HighlightedPoint.Color = Color.Orange;
            HighlightedPoint.MarkerSize = 10;
            HighlightedPoint.MarkerShape = ScottPlot.MarkerShape.openCircle;
            HighlightedPoint.IsVisible = false;

            formsPlot1.AxesChanged += FormsPlot1_AxesChanged;

            formsPlot1.Plot.XAxis.TickLabelFormat("ss:f", dateTimeFormat: true);
            formsPlot2.Plot.XAxis.TickLabelFormat("ss:f", dateTimeFormat: true);

            formsPlot2.MouseMove+= FormsPlot2_MouseMove;

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


            double[] buyersGroupingX = buyersX.GroupBy(p=>p).Select(p=>p.Key).ToArray();
            double[] buyersGroupingY = buyers.GroupBy(b=>b.DateTime).Select(p=>p.Sum(p=>Decimal.ToDouble(p.Quantity))).ToArray();

            double[] makersGroupingX = makersX.GroupBy(p => p).Select(p => p.Key).ToArray();
            double[] makersGroupingY = makers.GroupBy(b => b.DateTime).Select(p => p.Sum(p => Decimal.ToDouble(p.Quantity))).ToArray();

            var barsBuyers = formsPlot2.Plot.AddBar(buyersGroupingY, buyersGroupingX, color: Color.LightGreen);
            barsBuyers.BarWidth = 0.000001;
            barsBuyers.BorderColor = Color.LightGreen;

            var barsMakers = formsPlot2.Plot.AddBar(makersGroupingY, makersGroupingX, color: Color.Pink);
            barsMakers.BarWidth = 0.000001;
            barsMakers.BorderColor = Color.Pink;

            List<double> volumeX = new();
            volumeX.AddRange(buyersGroupingX); 
            volumeX.AddRange(makersGroupingX);

            List<double> volumeY = new();
            volumeY.AddRange(buyersGroupingY);
            volumeY.AddRange(makersGroupingY);

            MyScatterPlot = formsPlot2.Plot.AddScatterPoints(volumeX.ToArray(), volumeY.ToArray(), markerSize: 1, color: Color.Transparent);
        }

        private void FormsPlot2_MouseMove(object sender, MouseEventArgs e)
        {
            // determine point nearest the cursor
            (double mouseCoordX, double mouseCoordY) = formsPlot2.GetMouseCoordinates();
            double xyRatio = formsPlot2.Plot.XAxis.Dims.PxPerUnit / formsPlot2.Plot.YAxis.Dims.PxPerUnit;
            (double pointX, double pointY, int pointIndex) = MyScatterPlot.GetPointNearest(mouseCoordX, mouseCoordY, xyRatio);

            // place the highlight over the point of interest
            HighlightedPoint.X = pointX;
            HighlightedPoint.Y = pointY;
            HighlightedPoint.IsVisible = true;

            // render if the highlighted point chnaged
            if (LastHighlightedIndex != pointIndex)
            {
                LastHighlightedIndex = pointIndex;
                formsPlot2.Render();
            }

            // update the GUI to describe the highlighted point
            this.Title = $"Volume: {Math.Round(pointY)}";
        }


        private void FormsPlot1_AxesChanged(object sender, EventArgs e)
        {
            formsPlot2.Plot.MatchAxis(formsPlot1.Plot, horizontal: true, vertical: false);
            formsPlot2.Plot.MatchLayout(formsPlot1.Plot, horizontal: true, vertical: false);
            formsPlot2.Refresh();
        }
        //private void Load(Bet bet)
        //{
        //    try
        //    {
        //        Chart(bet.OpenPrice);
        //        if (bet.SymbolPrices != null)
        //        {
        //            DateTime startTime = bet.OpenTime.AddSeconds(-20);
        //            double bufferLower = Decimal.ToDouble(bet.PriceBufferLower);
        //            double bufferUpper = Decimal.ToDouble(bet.PriceBufferUpper);
        //            double distanceLower = Decimal.ToDouble(bet.PriceDistanceLower);
        //            double distanceUpper = Decimal.ToDouble(bet.PriceDistanceUpper);
        //            double takeProfit = Decimal.ToDouble(bet.PriceTakeProfit);
        //            double stopLoss = Decimal.ToDouble(bet.PriceStopLoss);
        //            IEnumerable<SymbolPrice> buyers = bet.SymbolPrices.Where(price => price != null).Where(price => price.BuyerIsMaker == false && price.DateTime >= startTime);
        //            IEnumerable<SymbolPrice> makers = bet.SymbolPrices.Where(price => price != null).Where(price => price.BuyerIsMaker == true && price.DateTime >= startTime);
        //            double[] buyersX = buyers.Select(price => price.DateTime.ToOADate()).ToArray();
        //            double[] buyersY = buyers.Select(price => Decimal.ToDouble(price.Price)).ToArray();
        //            double[] buyersQuantity = buyers.Select(price => Decimal.ToDouble(price.Quantity)).ToArray();
        //            double[] makersX = makers.Select(price => price.DateTime.ToOADate()).ToArray();
        //            double[] makersY = makers.Select(price => Decimal.ToDouble(price.Price)).ToArray();
        //            double[] makersQuantity = makers.Select(price => Decimal.ToDouble(price.Quantity)).ToArray();

        //            double[] xBuffer = { startTime.ToOADate(), bet.OpenTime.ToOADate() };
        //            double[] xDictance = { bet.OpenTime.ToOADate(), bet.CloseTime.AddSeconds(20).ToOADate() };

        //            if (buyers.Count() > 0) plt.Plot.AddScatter(buyersX, buyersY, color: Color.Green, lineWidth: 0, markerSize: 3);
        //            if (makers.Count() > 0) plt.Plot.AddScatter(makersX, makersY, color: Color.Red, lineWidth: 0, markerSize: 3);
        //            plt.Plot.AddScatterLines(xDictance, new double[] { distanceUpper, distanceUpper }, Color.Orange, lineStyle: LineStyle.Dash, label: $"D ▲ : {Math.Round(bet.DistanceUpper, 2)}");
        //            plt.Plot.AddScatterLines(xBuffer, new double[] { bufferUpper, bufferUpper }, Color.Gray, lineStyle: LineStyle.Dash, label: $"B ▲: {Math.Round(bet.BufferUpper, 2)}");
        //            plt.Plot.AddScatterLines(xBuffer, new double[] { bufferLower, bufferLower }, Color.Gray, lineStyle: LineStyle.Dash, label: $"B ▼ : {Math.Round(bet.BufferLower, 2)}");
        //            plt.Plot.AddScatterLines(xDictance, new double[] { distanceLower, distanceLower }, Color.Orange, lineStyle: LineStyle.Dash, label: $"D ▼ : {Math.Round(bet.DistanceLower, 2)}");
        //            plt.Plot.AddScatterLines(xDictance, new double[] { takeProfit, takeProfit }, Color.Green, lineStyle: LineStyle.Dash, label: $"TP : {Math.Round(bet.TakeProfit, 2)}");
        //            plt.Plot.AddScatterLines(xDictance, new double[] { stopLoss, stopLoss }, Color.Red, lineStyle: LineStyle.Dash, label: $"SL : {Math.Round(bet.StopLoss, 2)}");
        //            plt.Plot.AddPoint(bet.OpenTime.ToOADate(), Decimal.ToDouble(bet.OpenPrice), color: Color.Orange, size: 8);
        //            plt.Plot.AddPoint(bet.CloseTime.ToOADate(), Decimal.ToDouble(bet.ClosePrice), color: Color.DeepSkyBlue, size: 8);
        //            plt.Render();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //}
        //private void Chart(decimal price)
        //{
        //    string format;
        //    if (price > 10000m) format = "N0";
        //    else if (price > 100m) format = "N1";
        //    else if (price > 100m) format = "N2";
        //    else if (price > 10m) format = "N3";
        //    else if (price > 1m) format = "N4";
        //    else if (price > 0.1m) format = "N5";
        //    else if (price > 0.01m) format = "N6";
        //    else if (price > 0.001m) format = "N7";
        //    else format = "N8";
        //    plt.Plot.Layout(padding: 12);
        //    plt.Plot.Style(figureBackground: Color.Black, dataBackground: Color.Black);
        //    //plt.Plot.Frameless();
        //    plt.Plot.XAxis.TickLabelStyle(color: Color.White);
        //    plt.Plot.XAxis.TickMarkColor(ColorTranslator.FromHtml("#333333"));
        //    plt.Plot.XAxis.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

        //    plt.Plot.YAxis.Ticks(true);
        //    plt.Plot.YAxis.Grid(true);
        //    plt.Plot.YAxis2.Ticks(false);
        //    plt.Plot.YAxis2.Grid(false);
        //    plt.Plot.YAxis.TickLabelStyle(color: Color.White);
        //    plt.Plot.YAxis.TickMarkColor(ColorTranslator.FromHtml("#333333"));
        //    plt.Plot.YAxis.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

        //    plt.Plot.XAxis.TickLabelFormat("ss:f", dateTimeFormat: true);
        //    plt.Plot.YAxis.TickLabelFormat(format, dateTimeFormat: false);

        //    var legend = plt.Plot.Legend();
        //    legend.FillColor = Color.Transparent;
        //    legend.OutlineColor = Color.Transparent;
        //    legend.Font.Color = Color.White;
        //    legend.Font.Bold = true;
        //}

    }
}
