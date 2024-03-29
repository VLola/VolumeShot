﻿using Binance.Net.Clients;
using System.Collections.Generic;
using System.Linq;
using System;
using VolumeShot.Models;
using Binance.Net.Objects.Models.Futures;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Binance.Net.Objects.Models.Futures.Socket;
using ScottPlot;
using System.Drawing;
using VolumeShot.Command;
using System.Windows;
using System.Reflection;
using Binance.Net;

namespace VolumeShot.ViewModels
{
    internal class MainViewModel
    {
        private string path = $"{Directory.GetCurrentDirectory()}/log/";
        private string pathConfigs = $"{Directory.GetCurrentDirectory()}/configs/";
        private string pathPositions = Directory.GetCurrentDirectory() + "/log/positions/";
        private string pathOrders = Directory.GetCurrentDirectory() + "/log/orders/";
        private string pathHistory = $"{Directory.GetCurrentDirectory()}/history/";
        private const double _second10 = 0.00011574074596865103;
        private const double _second60 = 0.0006944444394321181;
        public LoginViewModel LoginViewModel { get; set; } = new();
        public Main Main { get; set; } = new();
        public BinanceClient client { get; set; }
        public BinanceSocketClient socketClient { get; set; }
        public delegate void AccountOnOrderUpdate(BinanceFuturesStreamOrderUpdate OrderUpdate);
        public event AccountOnOrderUpdate? OnOrderUpdate;
        public delegate void AccountOnAccountUpdate(BinanceFuturesStreamAccountUpdate AccountUpdate, string[] Symbols);
        public event AccountOnAccountUpdate? OnAccountUpdate;

        private RelayCommand? _addSymbolCommand;
        public RelayCommand AddSymbolCommand
        {
            get
            {
                return _addSymbolCommand ?? (_addSymbolCommand = new RelayCommand(obj => {
                    AddSymbolToListAsync();
                }));
            }
        }
        private RelayCommand? _clearSymbolsCommand;
        public RelayCommand ClearSymbolsCommand
        {
            get
            {
                return _clearSymbolsCommand ?? (_clearSymbolsCommand = new RelayCommand(obj => {
                    ClearSymbolsAsync();
                }));
            }
        }
        private RelayCommand? _cancelAllOrdersCommand;
        public RelayCommand CancelAllOrdersCommand
        {
            get
            {
                return _cancelAllOrdersCommand ?? (_cancelAllOrdersCommand = new RelayCommand(obj => {
                    CancelAllOrdersAsync();
                }));
            }
        }
        private RelayCommand? _tradingAllSymbolsCommand;
        public RelayCommand TradingAllSymbolsCommand
        {
            get
            {
                return _tradingAllSymbolsCommand ?? (_tradingAllSymbolsCommand = new RelayCommand(obj => {
                    TradingAllSymbolsAsync();
                }));
            }
        }
        private RelayCommand? _tradingOffSymbolsCommand;
        public RelayCommand TradingOffSymbolsCommand
        {
            get
            {
                return _tradingOffSymbolsCommand ?? (_tradingOffSymbolsCommand = new RelayCommand(obj => {
                    TradingOffSymbolsAsync();
                }));
            }
        }
        private RelayCommand? _runOnSymbolsCommand;
        public RelayCommand RunOnSymbolsCommand
        {
            get
            {
                return _runOnSymbolsCommand ?? (_runOnSymbolsCommand = new RelayCommand(obj => {
                    RunOnSymbolsAsync();
                }));
            }
        }
        private RelayCommand? _runOffSymbolsCommand;
        public RelayCommand RunOffSymbolsCommand
        {
            get
            {
                return _runOffSymbolsCommand ?? (_runOffSymbolsCommand = new RelayCommand(obj => {
                    RunOffSymbolsAsync();
                }));
            }
        }
        private RelayCommand? _saveAllVolumeCommand;
        public RelayCommand SaveAllVolumeCommand
        {
            get
            {
                return _saveAllVolumeCommand ?? (_saveAllVolumeCommand = new RelayCommand(obj => {
                    SaveAllVolumeAsync();
                }));
            }
        }
        public MainViewModel()
        {
            Main.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(2);
            if (!Directory.Exists(pathHistory)) Directory.CreateDirectory(pathHistory);
            if (!Directory.Exists(pathOrders)) Directory.CreateDirectory(pathOrders);
            if (!Directory.Exists(pathConfigs)) Directory.CreateDirectory(pathConfigs);
            if (!Directory.Exists(pathPositions)) Directory.CreateDirectory(pathPositions);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            LoginViewModel.Login.PropertyChanged += Login_PropertyChanged;
        }
        private async void SaveAllVolumeAsync()
        {
            await Task.Run(async () =>
            {
                Main.IsSaveAllVolumes = true;
                try
                {
                    List<Symbol> symbols = Main.Symbols.ToList();
                    foreach (var item in symbols)
                    {
                        item.IsSaveVolume = true;
                        await Task.Delay(100);
                    }
                    MessageBox.Show("Save ok");
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception SaveAllVolumeAsync: {ex?.Message}");
                }
                Main.IsSaveAllVolumes = false;
            });
        }
        private async void RunOffSymbolsAsync()
        {
            await Task.Run(async () => {
                try
                {
                    List<Symbol> symbols = Main.Symbols.ToList();
                    foreach (var item in symbols)
                    {
                        if (item.IsRun)
                        {
                            item.IsRun = false;
                            await Task.Delay(100);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception RunOnSymbolsAsync: {ex?.Message}");
                }
            });
        }
        private async void RunOnSymbolsAsync()
        {
            await Task.Run(async () => {
                try
                {
                    List<Symbol> symbols = Main.Symbols.ToList();
                    foreach (var item in symbols)
                    {
                        if (!item.IsRun)
                        {
                            item.IsRun = true;
                            await Task.Delay(500);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception RunOnSymbolsAsync: {ex?.Message}");
                }
            });
        }
        private async void TradingOffSymbolsAsync()
        {
            await Task.Run(() => {
                try
                {
                    List<Symbol> symbols = Main.Symbols.ToList();
                    foreach (var item in symbols)
                    {
                        if (item.IsRun)
                        {
                            item.IsTrading = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception TradingOffSymbolsAsync: {ex?.Message}");
                }
            });
        }
        private async void TradingAllSymbolsAsync()
        {
            await Task.Run(() => {
                try
                {
                    List<Symbol> symbols = Main.Symbols.ToList();
                    foreach (var item in symbols)
                    {
                        if (item.IsRun)
                        {
                            item.IsTrading = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception TradingAllSymbolsAsync: {ex?.Message}");
                }
            });
        }
        private async void ClearSymbolsAsync()
        {
            await Task.Run(() => {
                try
                {
                    List<Symbol> symbols = Main.Symbols.ToList();
                    foreach (var item in symbols)
                    {
                        if (!item.IsRun)
                        {
                            item.IsVisible = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception ClearSymbolsAsync: {ex?.Message}");
                }
            });
        }
        private async void AddSymbolToListAsync()
        {
            await Task.Run(() => {
                try
                {
                    if (Main.IsAddList)
                    {
                        if (Main.FullSymbols != null && Main.FullSymbols.Count > 0)
                        {
                            List<ConfigSelectedSymbol>? configSelectedSymbols = new();
                            string pathFileSymbols = pathConfigs + "symbols";
                            if (File.Exists(pathFileSymbols))
                            {
                                string json = File.ReadAllText(pathFileSymbols);
                                configSelectedSymbols = JsonConvert.DeserializeObject<List<ConfigSelectedSymbol>>(json);
                            }
                            if (configSelectedSymbols != null)
                            {
                                List<Symbol>? list = Main.FullSymbols.Where(symbol => symbol.Volume >= Main.MinVolume && symbol.Volume < Main.MaxVolume).ToList();
                                if (list != null && list.Count > 0)
                                {
                                    bool check;
                                    MessageBoxResult result = MessageBox.Show("Show warnings?", "Volume shot", MessageBoxButton.YesNo);
                                    if (result == MessageBoxResult.Yes) check = true;
                                    else check = false;
                                    foreach (Symbol symbol in list)
                                    {
                                        ConfigSelectedSymbol? configSelectedSymbol = configSelectedSymbols.FirstOrDefault(conf => conf.Symbol == symbol.Name);
                                        if (configSelectedSymbol == null)
                                        {
                                            symbol.IsVisible = true;
                                        }
                                        else
                                        {
                                            if (check)
                                            {
                                                if (configSelectedSymbol.UserName == Main.LoginUser) MessageBox.Show($"Symbol already added {symbol.Name}");
                                                else MessageBox.Show($"Symbol: {symbol.Name} added by another user: {configSelectedSymbol.UserName}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Main.SelectedFullSymbol != null)
                        {
                            Symbol symbol = Main.SelectedFullSymbol;

                            List<ConfigSelectedSymbol>? configSelectedSymbols = new();
                            string pathFileSymbols = pathConfigs + "symbols";
                            if (File.Exists(pathFileSymbols))
                            {
                                string json = File.ReadAllText(pathFileSymbols);
                                configSelectedSymbols = JsonConvert.DeserializeObject<List<ConfigSelectedSymbol>>(json);
                            }
                            if (configSelectedSymbols != null)
                            {
                                ConfigSelectedSymbol? configSelectedSymbol = configSelectedSymbols.FirstOrDefault(conf => conf.Symbol == symbol.Name);
                                if (configSelectedSymbol == null)
                                {
                                    symbol.IsVisible = true;
                                }
                                else
                                {
                                    if (configSelectedSymbol.UserName == Main.LoginUser) MessageBox.Show("Symbol already added");
                                    else MessageBox.Show($"Symbol added by another user: {configSelectedSymbol.UserName}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception AddSymbolToListAsync: {ex?.Message}");
                }
            });
        }
        private void Login_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsLogin")
            {
                if (LoginViewModel.Login.IsLogin)
                {
                    LoadMain();
                }
            }
        }
        private void LoadMain()
        {
            Main.LoginUser = LoginViewModel.Login.SelectedUser.Name;
            client = LoginViewModel.Client;
            socketClient = LoginViewModel.SocketClient;
            BalanceFutureAsync();
            LoadBetsAsync();
            GetOpenOrdersAsync();
            GetPositionInformationAsync();
            SubscribeToUserDataUpdatesAsync();
            LoadSymbols();
            LoadChart();
            RunChartAsync();
            CheckPingAsync();
            RunTimeAsync();
        }
        private async void RunTimeAsync()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    Main.DateTime = DateTime.UtcNow;
                    if(Main.General.Orders > 0) Main.General.Orders -= (Math.Round((Main.General.Orders / 60)) + 1);
                }
            });
        }
        private async void CheckLimitsAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    Main.General.Orders -= Math.Round((Main.General.Orders / 60));
                    //if (!Main.IsMaxRequests && Main.General.Requests > 900)
                    //{
                    //    Main.IsMaxRequests = true;
                    //    MaxRequestsAsync();
                    //}
                    //else if (!Main.IsMaxRequests && Main.General.Orders > 600)
                    //{
                    //    Main.IsMaxRequests = true;
                    //    MaxOrdersAsync();
                    //}
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception CheckLimitsAsync: {ex?.Message}");
                }
            });
        }
        private async void MaxOrdersAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    List<Symbol>? fullList = Main.Symbols.Where(symbol => symbol.IsTrading == true).ToList();
                    int count = (fullList.Count / 2);
                    List<Symbol>? list = fullList.Where((symbol, index) => symbol.IsTrading == true && index < count).ToList();
                    if (list != null && list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            if (item.IsTrading) item.IsTrading = false;
                        }
                        string[] symbols = list.Select(item => item.Name).ToArray();
                        Error.WriteLog(path, Main.LoginUser, $"Orders == {Math.Round(Main.General.Orders)}, stop trading: {string.Join(", ", symbols)}");
                        while (true)
                        {
                            await Task.Delay(30000);
                            if (Main.General.Orders <= 300) break;
                        }
                        Error.WriteLog(path, Main.LoginUser, $"Orders == {Math.Round(Main.General.Orders)}, start trading: {string.Join(", ", symbols)}");
                        foreach (var item in list)
                        {
                            if (!item.IsTrading) item.IsTrading = true;
                        }
                    }
                    Main.IsMaxRequests = false;
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception MaxOrdersAsync: {ex?.Message}");
                }

            });
        }
        private async void MaxRequestsAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    List<Symbol>? fullList = Main.Symbols.Where(symbol => symbol.IsTrading == true).ToList();
                    int count = (fullList.Count / 2);
                    List<Symbol>? list = fullList.Where((symbol, index) => symbol.IsTrading == true && index < count).ToList();
                    if (list != null && list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            if (item.IsTrading) item.IsTrading = false;
                        }
                        string[] symbols = list.Select(item => item.Name).ToArray();
                        Error.WriteLog(path, Main.LoginUser, $"Requests == {Main.General.Requests}, stop trading: {string.Join(", ", symbols)}");
                        while (true)
                        {
                            await Task.Delay(30000);
                            if (Main.General.Requests <= 500) break;
                        }
                        Error.WriteLog(path, Main.LoginUser, $"Requests == {Main.General.Requests}, start trading: {string.Join(", ", symbols)}");
                        foreach (var item in list)
                        {
                            if (!item.IsTrading) item.IsTrading = true;
                        }
                    }
                    Main.IsMaxRequests = false;
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception MaxRequestsAsync: {ex?.Message}");
                }

            });
        }
        private async void CheckPingAsync()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    try
                    {
                        var result = await client.UsdFuturesApi.ExchangeData.PingAsync();
                        if (!result.Success)
                        {
                            Error.WriteLog(path, Main.LoginUser, $"Failed CheckPingAsync: {result.Error?.Message}");
                            Main.Ping = 10000;
                        }
                        else
                        {
                            Main.Ping = result.Data;
                        }
                    }
                    catch (Exception ex)
                    {
                        Error.WriteLog(path, Main.LoginUser, $"Exception CheckPingAsync: {ex?.Message}");
                        Main.Ping = 10000;
                    }
                    if (!Main.IsStopTrading)
                    {
                        if(Main.Ping > Main.PingMax)
                        {
                            Main.IsStopTrading = true;
                            StopTradingAsync();
                        }
                    }
                }
            });
        }
        private async void StopTradingAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    List<Symbol>? list = Main.Symbols.Where(symbol=>symbol.IsTrading == true).ToList();
                    if(list != null && list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            if (item.IsTrading) item.IsTrading = false;
                        }
                        string[] symbols = list.Select(item => item.Name).ToArray();
                        Error.WriteLog(path, Main.LoginUser, $"Ping > {Main.PingMax}, stop trading: {string.Join(", ", symbols)}");
                        while (true)
                        {
                            await Task.Delay(30000);
                            if (Main.Ping <= Main.PingMax) break;
                        }
                        Error.WriteLog(path, Main.LoginUser, $"Ping <= {Main.PingMax}, start trading: {string.Join(", ", symbols)}");
                        foreach (var item in list)
                        {
                            if (!item.IsTrading) item.IsTrading = true;
                        }
                    }
                    Main.IsStopTrading = false;
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception StopTradingAsync: {ex?.Message}");
                }
            });
        }
        private async void LoadBetsAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    List<Bet> allBets = new();
                    string[] files = Directory.GetFiles(pathHistory + Main.LoginUser + "/");
                    foreach (string file in files)
                    {
                        string json = File.ReadAllText(file);
                        List<Bet>? bets = JsonConvert.DeserializeObject<List<Bet>>(json);
                        if (bets != null && bets.Count > 0)
                        {
                            foreach (var item in bets)
                            {
                                allBets.Add(item);
                            }
                        }
                    }
                    allBets = allBets.OrderByDescending(bet=>bet.OpenTime).ToList();
                    Main.TotalHistory = allBets.Select(bet=>bet.Total).Sum();
                    App.Current.Dispatcher.Invoke(() => {
                        foreach (var bet in allBets)
                        {
                            Main.Bets.Add(bet);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception LoadBetsAsync: {ex?.Message}");
                }
            });
        }
        private void LoadChart()
        {
            try
            {
                Main.WpfPlot.Dispatcher.Invoke(new Action(() =>
                {
                    // Style
                    Main.WpfPlot.Plot.Layout(padding: 12);
                    Main.WpfPlot.Plot.Style(figureBackground: Color.Black, dataBackground: Color.Black);
                    Main.WpfPlot.Plot.Frameless();
                    Main.WpfPlot.Plot.XAxis.TickLabelStyle(color: Color.White);
                    Main.WpfPlot.Plot.XAxis.TickMarkColor(ColorTranslator.FromHtml("#333333"));
                    Main.WpfPlot.Plot.XAxis.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

                    Main.WpfPlot.Plot.YAxis.Ticks(false);
                    Main.WpfPlot.Plot.YAxis.Grid(false);
                    Main.WpfPlot.Plot.YAxis2.Ticks(true);
                    Main.WpfPlot.Plot.YAxis2.Grid(true);
                    Main.WpfPlot.Plot.YAxis2.TickLabelStyle(color: ColorTranslator.FromHtml("#00FF00"));
                    Main.WpfPlot.Plot.YAxis2.TickMarkColor(ColorTranslator.FromHtml("#333333"));
                    Main.WpfPlot.Plot.YAxis2.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

                    var legend = Main.WpfPlot.Plot.Legend();
                    legend.FillColor = Color.Transparent;
                    legend.OutlineColor = Color.Transparent;
                    legend.Font.Color = Color.White;
                    legend.Font.Bold = true;

                    // Lines

                    Main.WpfPlot.Plot.AddScatterLines(Main.x, Main.bufferLower, Color.Gray, lineStyle: LineStyle.Dash);
                    Main.WpfPlot.Plot.AddScatterLines(Main.x, Main.bufferUpper, Color.Gray, lineStyle: LineStyle.Dash);
                    Main.WpfPlot.Plot.AddScatterLines(Main.x, Main.distanceLower, Color.Orange, lineStyle: LineStyle.Dash);
                    Main.WpfPlot.Plot.AddScatterLines(Main.x, Main.distanceUpper, Color.Orange, lineStyle: LineStyle.Dash);
                    Main.WpfPlot.Plot.AddScatterLines(Main.x, Main.takeProfit, Color.Green, lineStyle: LineStyle.Dash);
                    Main.WpfPlot.Plot.AddScatterLines(Main.x, Main.stopLoss, Color.Red, lineStyle: LineStyle.Dash);
                    Main.WpfPlot.Plot.RenderLock();
                    Main.WpfPlot.Render();
                    Main.WpfPlot.Plot.RenderUnlock();
                }));

            }
            catch (Exception ex)
            {
                Error.WriteLog(path, Main.LoginUser, $"Exception LoadChart: {ex?.Message}");
            }
        }
        private async void RunChartAsync()
        {
            await Task.Run(async () =>
            {
                while(true)
                {
                    await Task.Delay(100);
                    if(Main.SelectedSymbol != null && Main.IsVisibleChart)
                    {
                        try
                        {

                            if (Main.SelectedSymbol.BufferLowerPrice > 0m &&
                                Main.SelectedSymbol.BufferUpperPrice > 0m)
                            {
                                double dateTimeNew = System.DateTime.UtcNow.ToOADate();
                                double dateTimeOld = System.DateTime.UtcNow.AddMinutes(-1).ToOADate();
                                Main.x[0] = dateTimeOld;
                                Main.x[1] = dateTimeNew;
                                Main.bufferLower[0] = Main.bufferLower[1] = Decimal.ToDouble(Main.SelectedSymbol.BufferLowerPrice);
                                Main.bufferUpper[0] = Main.bufferUpper[1] = Decimal.ToDouble(Main.SelectedSymbol.BufferUpperPrice);

                                if (Main.SelectedSymbol.Exchange.DistanceLowerPrice > 0m &&
                                    Main.SelectedSymbol.Exchange.DistanceUpperPrice > 0m)
                                {
                                    Main.distanceLower[0] = Main.distanceLower[1] = Decimal.ToDouble(Main.SelectedSymbol.Exchange.DistanceLowerPrice);
                                    Main.distanceUpper[0] = Main.distanceUpper[1] = Decimal.ToDouble(Main.SelectedSymbol.Exchange.DistanceUpperPrice);
                                    if (Main.SelectedSymbol.Exchange.IsOpenLongOrder)
                                    {
                                        Main.takeProfit[0] = Main.takeProfit[1] = Decimal.ToDouble(Main.SelectedSymbol.Exchange.TakeProfitLongPrice);
                                        Main.stopLoss[0] = Main.stopLoss[1] = Decimal.ToDouble(Main.SelectedSymbol.Exchange.StopLossLongPrice);
                                    }
                                    else if (Main.SelectedSymbol.Exchange.IsOpenShortOrder)
                                    {
                                        Main.takeProfit[0] = Main.takeProfit[1] = Decimal.ToDouble(Main.SelectedSymbol.Exchange.TakeProfitShortPrice);
                                        Main.stopLoss[0] = Main.stopLoss[1] = Decimal.ToDouble(Main.SelectedSymbol.Exchange.StopLossShortPrice);
                                    }
                                    else
                                    {
                                        Main.takeProfit[0] = Main.takeProfit[1] = 0;
                                        Main.stopLoss[0] = Main.stopLoss[1] = 0;
                                    }
                                    AutoAxis(dateTimeNew, Main.distanceLower[0], Main.distanceUpper[0]);
                                }
                                else
                                {

                                    AutoAxis(dateTimeNew, Main.bufferLower[0], Main.bufferUpper[0]);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Error.WriteLog(path, Main.LoginUser, $"Exception RunChartAsync: {ex?.Message}");
                        }
                    }
                }
            });
        }
        private void AutoAxis(double dateTime, double lowerDistance, double upperDistance)
        {
            Main.WpfPlot.Dispatcher.Invoke(new Action(() =>
            {
                Main.WpfPlot.Plot.RenderLock();
                Main.WpfPlot.Plot.SetAxisLimitsY(yMin: lowerDistance - (lowerDistance / 300), yMax: upperDistance + (upperDistance / 300));
                Main.WpfPlot.Plot.SetAxisLimitsX(xMin: dateTime - _second60, xMax: dateTime + _second10);
                Main.WpfPlot.Render();
                Main.WpfPlot.Plot.RenderUnlock();
            }));
        }
        private async void BalanceFutureAsync()
        {
            await Task.Run(async() => {
                try
                {
                    var result = await client.UsdFuturesApi.Account.GetAccountInfoAsync();
                    if (!result.Success)
                    {
                        Error.WriteLog(path, Main.LoginUser, $"Failed BalanceFutureAsync: {result.Error?.Message}");
                    }
                    else
                    {
                        Main.Balance = result.Data.TotalMarginBalance;
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception BalanceFutureAsync: {ex?.Message}");
                }
            });
        }
        private async void CancelAllOrdersAsync()
        {
            try
            {
                List<Symbol> symbols = Main.Symbols.ToList();
                List<Order> list = new();
                lock (Main.Orders) list = Main.Orders.ToList();
                if (list.Count > 0)
                {
                    foreach (var order in list)
                    {
                        var result = await client.UsdFuturesApi.Trading.CancelOrderAsync(order.Symbol, order.OrderId);
                        if (!result.Success)
                        {
                            Error.WriteLog(path, Main.LoginUser, $"Failed CancelOrderAsync: {result.Error?.Message}");
                        }
                        else
                        {
                            var weight = BinanceHelpers.UsedWeight(result.ResponseHeaders);
                            Main.General.Requests = weight;
                        }
                    }
                }
            }
            catch(Exception ex) 
            {
                Error.WriteLog(path, Main.LoginUser, $"Exception CancelOrderAsync: {ex?.Message}");
            }
        }
        private async void GetOpenOrdersAsync()
        {
            try
            {
                var result = await client.UsdFuturesApi.Trading.GetOpenOrdersAsync();
                if (!result.Success)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Failed GetOpenOrdersAsync: {result.Error?.Message}");
                }
                else
                {
                    AddAllOrdersAsync(result.Data);
                }
            }
            catch (Exception ex)
            {
                Error.WriteLog(path, Main.LoginUser, $"Exception GetOpenOrdersAsync: {ex?.Message}");
            }
        }
        private async void GetPositionInformationAsync()
        {
            try
            {
                var result = await client.UsdFuturesApi.Account.GetPositionInformationAsync();
                if (!result.Success)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Failed GetPositionInformationAsync: {result.Error?.Message}");
                }
                else
                {
                    AddAllPositionsAsync(result.Data);
                }
            }
            catch (Exception ex)
            {
                Error.WriteLog(path, Main.LoginUser, $"Exception GetPositionInformationAsync: {ex?.Message}");
            }
        }
        private async void SubscribeToUserDataUpdatesAsync()
        {
            try
            {
                var listenKey = await client.UsdFuturesApi.Account.StartUserStreamAsync();
                if (!listenKey.Success)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Failed to start user stream: listenKey");
                }
                else
                {
                    KeepAliveUserStreamAsync(listenKey.Data);
                    Error.WriteLog(path, Main.LoginUser, $"Listen Key Created");
                    var result = await socketClient.UsdFuturesStreams.SubscribeToUserDataUpdatesAsync(listenKey: listenKey.Data,
                        onLeverageUpdate => { },
                        onMarginUpdate => { },
                        onAccountUpdate => {
                            if (onAccountUpdate.Data.UpdateData.Reason == Binance.Net.Enums.AccountUpdateReason.Order)
                            {
                                string[] symbols = onAccountUpdate.Data.UpdateData.Positions.Select(pos => pos.Symbol).ToArray();
                                OnAccountUpdate?.Invoke(onAccountUpdate.Data, symbols);
                                AddSymbolPositionsAsync(onAccountUpdate.Data.UpdateData.Positions);
                            }
                            Main.Balance = onAccountUpdate.Data.UpdateData.Balances.ToList()[0].CrossWalletBalance;
                        },
                        onOrderUpdate =>
                        {
                            OnOrderUpdate?.Invoke(onOrderUpdate.Data);
                            AddOrder(onOrderUpdate.Data.UpdateData);
                            Main.General.Orders += 1;
                            WriteLogOrder(onOrderUpdate.Data.UpdateData);
                        },
                        onListenKeyExpired => { },
                        onStrategyUpdate => { },
                        onGridUpdate => { });
                    if (!result.Success)
                    {
                        Error.WriteLog(path, Main.LoginUser, $"Failed UserDataUpdates: {result.Error?.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Error.WriteLog(path, Main.LoginUser, $"Exception SubscribeToUserDataUpdatesAsync: {ex?.Message}");
            }
        }
        private async void AddAllOrdersAsync(IEnumerable<BinanceFuturesOrder> futuresOrders)
        {
            await Task.Run(() =>
            {
                try
                {
                    App.Current.Dispatcher.Invoke(() => {
                        foreach (var item in futuresOrders)
                        {
                            lock (Main.Orders) Main.Orders.Insert(0, new Order(item, client));
                        }
                    });
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception AddAllOrdersAsync: {ex.Message}");
                }
            });
        }
        private void WriteLogOrder(BinanceFuturesStreamOrderUpdateData orderUpdateData)
        {
            // Write log
            try
            {
                string json = JsonConvert.SerializeObject(orderUpdateData);
                Error.WriteLog(pathOrders, orderUpdateData.Symbol, json);
            }
            catch (Exception ex)
            {
                Error.WriteLog(path, Main.LoginUser, $"Exception WriteLogOrder: {ex.Message}");
            }
        }
        private async void AddOrder(BinanceFuturesStreamOrderUpdateData orderUpdateData)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (orderUpdateData.Status == Binance.Net.Enums.OrderStatus.New)
                    {
                        Order order = new(orderUpdateData, client);
                        App.Current.Dispatcher.Invoke(() => {
                            lock (Main.Orders) Main.Orders.Insert(0, order);
                        });
                    }
                    else if (orderUpdateData.Status == Binance.Net.Enums.OrderStatus.Canceled || orderUpdateData.Status == Binance.Net.Enums.OrderStatus.Filled || orderUpdateData.Status == Binance.Net.Enums.OrderStatus.Expired)
                    {
                        Order? order = null;
                        lock (Main.Orders) order = Main.Orders.FirstOrDefault(order => order.OrderId == orderUpdateData.OrderId);
                        if (order != null)
                        {
                            App.Current.Dispatcher.Invoke(() => {
                                lock (Main.Orders) Main.Orders.Remove(order);
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception AddOrder: {ex.Message}");
                }
            });
        } 
        private async void AddAllPositionsAsync(IEnumerable<BinancePositionDetailsUsdt> positions)
        {
            await Task.Run(() => {
                try
                {
                    foreach (var item in positions)
                    {
                        if (item.Quantity != 0m)
                        {
                            Position positionNew = new Position(item, client, socketClient);
                            positionNew.PropertyChanged += PositionNew_PropertyChanged;
                            App.Current.Dispatcher.Invoke(() => {
                                Main.Positions.Add(positionNew);
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception AddAllPositionsAsync: {ex.Message}");
                }
            });
        }
        private async void AddSymbolPositionsAsync(IEnumerable<BinanceFuturesStreamPosition> positions)
        {
            await Task.Run(() =>
            {
                try
                {
                    foreach (var item in positions)
                    {
                        Position? position = Main.Positions.ToArray().FirstOrDefault(position => position.Symbol == item.Symbol && position.PositionSide == item.PositionSide);
                        if (position != null)
                        {
                            position.Quantity = item.Quantity;
                            position.Price = item.EntryPrice;
                        }
                        else
                        {
                            if (item.Quantity != 0m)
                            {
                                Position positionNew = new Position(item, client, socketClient);
                                positionNew.PropertyChanged += PositionNew_PropertyChanged;
                                App.Current.Dispatcher.Invoke(() => {
                                    Main.Positions.Add(positionNew);
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Exception AddSymbolPositionsAsync: {ex.Message}");
                }
            });
        }

        private void PositionNew_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Quantity")
            {
                Position? position = sender as Position;
                if (position != null)
                {
                    if (position.Quantity == 0m)
                    {
                        try
                        {
                            App.Current.Dispatcher.Invoke(() => {
                                Main.Positions.Remove(position);
                            });
                        }
                        catch (Exception ex)
                        {
                            Error.WriteLog(path, Main.LoginUser, $"Exception PositionNew_PropertyChanged: {ex.Message}");
                        }
                    }
                }
            }
        }

        private async void KeepAliveUserStreamAsync(string listenKey)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var result = await client.UsdFuturesApi.Account.KeepAliveUserStreamAsync(listenKey);
                        if (!result.Success) Error.WriteLog(path, Main.LoginUser, $"Failed KeepAliveUserStreamAsync: {result.Error?.Message}");
                        else
                        {
                            Error.WriteLog(path, Main.LoginUser, "Success KeepAliveUserStreamAsync");
                        }
                    }
                    catch (Exception ex)
                    {
                        Error.WriteLog(path, Main.LoginUser, $"Exception KeepAliveUserStreamAsync: {ex?.Message}");
                    }
                    await Task.Delay(900000);
                }
            });
        }
        private void LoadSymbols()
        {
            try
            {
                List<BinanceFuturesUsdtSymbol> list = ListSymbols().OrderBy(item => item.Name).ToList();
                if (list.Count > 0)
                {
                    List<Config>? configs = new();
                    string pathFileConfig = pathConfigs + "config";
                    if (File.Exists(pathFileConfig))
                    {
                        string json = File.ReadAllText(pathFileConfig);
                        configs = JsonConvert.DeserializeObject<List<Config>>(json);
                    }
                    List<ConfigSelectedSymbol>? configSelectedSymbols = new();
                    string pathFileSymbols = pathConfigs + "symbols";
                    if (File.Exists(pathFileSymbols))
                    {
                        string json = File.ReadAllText(pathFileSymbols);
                        configSelectedSymbols = JsonConvert.DeserializeObject<List<ConfigSelectedSymbol>>(json);
                    }
                    foreach (var symbol in list)
                    {
                        if (symbol.Name == "RAYUSDT" || 
                            symbol.Name == "FTTUSDT" || 
                            symbol.Name == "CVCUSDT" || 
                            symbol.Name == "SRMUSDT" || 
                            symbol.Name == "SCUSDT" ||
                            symbol.Name == "HNTUSDT" ||
                            symbol.Name == "BTSUSDT" ||
                            symbol.Name == "BTCSTUSDT") continue;

                        if (symbol.QuoteAsset == "USDT")
                        {
                            decimal volume = 500000m;
                            if (configs != null)
                            {
                                Config? config = configs.FirstOrDefault(conf => conf.Name == symbol.Name);
                                if (config != null) volume = config.Volume;
                            }
                            SymbolViewModel symbolViewModel = new(Main.General , symbol, volume, socketClient, client, LoginViewModel.Login.SelectedUser.IsTestnet, Main.LoginUser);
                            OnOrderUpdate += symbolViewModel.ExchangeViewModel.OrderUpdate;
                            OnAccountUpdate += symbolViewModel.ExchangeViewModel.AccountUpdate;
                            symbolViewModel.Symbol.PropertyChanged += Symbol_PropertyChanged;
                            symbolViewModel.Symbol.Exchange.PropertyChanged += Exchange_PropertyChanged;
                            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                                Main.FullSymbols.Add(symbolViewModel.Symbol);
                            }));
                            if (configSelectedSymbols != null && configSelectedSymbols.Count > 0)
                            {
                                if (configSelectedSymbols.Contains(new ConfigSelectedSymbol(Main.LoginUser, symbol.Name)))
                                {
                                    App.Current.Dispatcher.BeginInvoke(new Action(() => {
                                        Main.Symbols.Add(symbolViewModel.Symbol);
                                    }));
                                }
                            }
                        }
                    }
                    Main.SelectedFullSymbol = Main.FullSymbols[0];
                }
            }
            catch (Exception ex)
            {
                Error.WriteLog(path, Main.LoginUser, $"Exception LoadSymbols: {ex?.Message}");
            }
        }

        private void Exchange_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Bet")
            {
                Exchange? exchange = sender as Exchange;
                if(exchange != null)
                {
                    try
                    {
                        Main.TotalHistory += exchange.Bet.Total;
                        App.Current.Dispatcher.BeginInvoke(new Action(() => {
                            Main.Bets.Insert(0, exchange.Bet);
                        }));
                    }
                    catch (Exception ex)
                    {
                        Error.WriteLog(path, Main.LoginUser, $"Exception Exchange_PropertyChanged: {ex.Message}");
                    }
                }
            }
        }

        private void Symbol_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsVisible")
            {
                try
                {
                    Symbol? symbol = sender as Symbol;
                    if (symbol != null)
                    {
                        if (symbol.IsVisible)
                        {
                            if (!Main.Symbols.Contains(symbol))
                            {
                                List<ConfigSelectedSymbol>? configSelectedSymbols = new();
                                string pathFileSymbols = pathConfigs + "symbols";
                                if (File.Exists(pathFileSymbols))
                                {
                                    string json = File.ReadAllText(pathFileSymbols);
                                    configSelectedSymbols = JsonConvert.DeserializeObject<List<ConfigSelectedSymbol>>(json);
                                }
                                if (configSelectedSymbols != null)
                                {
                                    configSelectedSymbols.Add(new ConfigSelectedSymbol(Main.LoginUser, symbol.Name));
                                    File.WriteAllText(pathFileSymbols, JsonConvert.SerializeObject(configSelectedSymbols));
                                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        Main.Symbols.Add(symbol);
                                    }));
                                }
                            }
                        }
                        else
                        {
                            List<ConfigSelectedSymbol>? configSelectedSymbols = new();
                            string pathFileSymbols = pathConfigs + "symbols";
                            if (File.Exists(pathFileSymbols))
                            {
                                string json = File.ReadAllText(pathFileSymbols);
                                configSelectedSymbols = JsonConvert.DeserializeObject<List<ConfigSelectedSymbol>>(json);
                            }
                            if (configSelectedSymbols != null && configSelectedSymbols.Count > 0)
                            {
                                ConfigSelectedSymbol? configSelectedSymbol = configSelectedSymbols.FirstOrDefault(conf => conf.Symbol == symbol.Name);
                                if (configSelectedSymbol != null)
                                {
                                    configSelectedSymbols.Remove(configSelectedSymbol);
                                    File.WriteAllText(pathFileSymbols, JsonConvert.SerializeObject(configSelectedSymbols));
                                    App.Current.Dispatcher.BeginInvoke(new Action(() => {
                                        Main.Symbols.Remove(symbol);
                                    }));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    Error.WriteLog(path, Main.LoginUser, $"Exception Symbol_PropertyChanged: {ex.Message}");
                }
            }
        }

        private List<BinanceFuturesUsdtSymbol> ListSymbols()
        {
            try
            {
                var result = client.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync().Result;
                if (!result.Success)
                {
                    Error.WriteLog(path, Main.LoginUser, $"Failed ListSymbols {result.Error?.Message}");
                    return new();
                }
                return result.Data.Symbols.ToList();

            }
            catch (Exception ex)
            {
                Error.WriteLog(path, Main.LoginUser, $"Exception ListSymbols: {ex.Message}");
                return new();
            }
        }
    }
}
