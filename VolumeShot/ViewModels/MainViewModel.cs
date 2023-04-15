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
using System.Collections.ObjectModel;

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
        string errorFile = "Main";
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
                    if (Main.SelectedFullSymbol != null) Main.SelectedFullSymbol.IsVisible = true;
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
        public MainViewModel()
        {
            if (!Directory.Exists(pathHistory)) Directory.CreateDirectory(pathHistory);
            if (!Directory.Exists(pathOrders)) Directory.CreateDirectory(pathOrders);
            if (!Directory.Exists(pathConfigs)) Directory.CreateDirectory(pathConfigs);
            if (!Directory.Exists(pathPositions)) Directory.CreateDirectory(pathPositions);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            LoginViewModel.Login.PropertyChanged += Login_PropertyChanged;
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
            client = LoginViewModel.Client;
            socketClient = LoginViewModel.SocketClient;
            LoadBetsAsync();
            GetOpenOrdersAsync();
            GetPositionInformationAsync();
            SubscribeToUserDataUpdatesAsync();
            LoadSymbols();
            LoadChart();
            RunChartAsync();
        }
        private async void LoadBetsAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    List<Bet> allBets = new();
                    string[] files = Directory.GetFiles(pathHistory);
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
                    App.Current.Dispatcher.Invoke(() => {
                        foreach (var bet in allBets)
                        {
                            Main.Bets.Add(bet);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, errorFile, $"Exception CancelOrderAsync: {ex?.Message}");
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
                Error.WriteLog(path, errorFile, $"Exception LoadChart: {ex?.Message}");
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
                            Error.WriteLog(path, errorFile, $"Exception RunChartAsync: {ex?.Message}");
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
        private async void CancelAllOrdersAsync()
        {
            try
            {
                List<Order> list = Main.Orders.ToList();
                if (list.Count > 0)
                {
                    foreach (var order in list)
                    {
                        var result = await client.UsdFuturesApi.Trading.CancelOrderAsync(order.Symbol, order.OrderId);
                        if (!result.Success)
                        {
                            Error.WriteLog(path, errorFile, $"Failed CancelOrderAsync: {result.Error?.Message}");
                        }
                    }
                }
            }
            catch(Exception ex) 
            {
                Error.WriteLog(path, errorFile, $"Exception CancelOrderAsync: {ex?.Message}");
            }
        }
        private async void GetOpenOrdersAsync()
        {
            try
            {
                var result = await client.UsdFuturesApi.Trading.GetOpenOrdersAsync();
                if (!result.Success)
                {
                    Error.WriteLog(path, errorFile, $"Failed GetOpenOrdersAsync: {result.Error?.Message}");
                }
                else
                {
                    AddAllOrdersAsync(result.Data);
                }
            }
            catch (Exception ex)
            {
                Error.WriteLog(path, errorFile, $"Exception GetOpenOrdersAsync: {ex?.Message}");
            }
        }
        private async void GetPositionInformationAsync()
        {
            try
            {
                var result = await client.UsdFuturesApi.Account.GetPositionInformationAsync();
                if (!result.Success)
                {
                    Error.WriteLog(path, errorFile, $"Failed GetPositionInformationAsync: {result.Error?.Message}");
                }
                else
                {
                    AddAllPositionsAsync(result.Data);
                }
            }
            catch (Exception ex)
            {
                Error.WriteLog(path, errorFile, $"Exception GetPositionInformationAsync: {ex?.Message}");
            }
        }
        private async void SubscribeToUserDataUpdatesAsync()
        {
            try
            {
                var listenKey = await client.UsdFuturesApi.Account.StartUserStreamAsync();
                if (!listenKey.Success)
                {
                    Error.WriteLog(path, errorFile, $"Failed to start user stream: listenKey");
                }
                else
                {
                    KeepAliveUserStreamAsync(listenKey.Data);
                    Error.WriteLog(path, errorFile, $"Listen Key Created");
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
                        },
                        onOrderUpdate =>
                        {
                            OnOrderUpdate?.Invoke(onOrderUpdate.Data);
                            AddOrder(onOrderUpdate.Data.UpdateData);
                        },
                        onListenKeyExpired => { },
                        onStrategyUpdate => { },
                        onGridUpdate => { });
                    if (!result.Success)
                    {
                        Error.WriteLog(path, errorFile, $"Failed UserDataUpdates: {result.Error?.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Error.WriteLog(path, errorFile, $"Exception SubscribeToUserDataUpdatesAsync: {ex?.Message}");
            }
        }
        private async void AddAllOrdersAsync(IEnumerable<BinanceFuturesOrder> futuresOrders)
        {
            await Task.Run(() =>
            {
                App.Current.Dispatcher.Invoke(() => {
                    foreach (var item in futuresOrders)
                    {
                        Main.Orders.Insert(0, new Order(item, client));
                    }
                });
            });
        }
        private async void AddOrder(BinanceFuturesStreamOrderUpdateData orderUpdateData)
        {
            await Task.Run(() =>
            {
                if (orderUpdateData.Status != Binance.Net.Enums.OrderStatus.Canceled && orderUpdateData.Status != Binance.Net.Enums.OrderStatus.Filled)
                {
                    Order order = new(orderUpdateData, client);
                    App.Current.Dispatcher.Invoke(() => {
                        Main.Orders.Insert(0, order);
                    });
                }
                else
                {
                    Order? order = Main.Orders.FirstOrDefault(order=>order.OrderId == orderUpdateData.OrderId);
                    if (order != null)
                    {
                        App.Current.Dispatcher.Invoke(() => {
                            Main.Orders.Remove(order);
                        });
                    }
                }
            });
        } 
        private async void AddAllPositionsAsync(IEnumerable<BinancePositionDetailsUsdt> positions)
        {
            await Task.Run(() => {
                foreach (var item in positions)
                {
                    if(item.Quantity != 0m)
                    {
                        Position positionNew = new Position(item, client, socketClient);
                        positionNew.PropertyChanged += PositionNew_PropertyChanged;
                        App.Current.Dispatcher.Invoke(() => {
                            Main.Positions.Add(positionNew);
                        });
                    }
                }
            });
        }
        private async void AddSymbolPositionsAsync(IEnumerable<BinanceFuturesStreamPosition> positions)
        {
            await Task.Run(() =>
            {
                foreach (var item in positions)
                {
                    Position? position = Main.Positions.FirstOrDefault(position => position.Symbol == item.Symbol && position.PositionSide == item.PositionSide);
                    if (position != null)
                    {
                        position.Quantity = item.Quantity;
                        position.Price = item.EntryPrice;
                    }
                    else
                    {
                        if(item.Quantity != 0m)
                        {
                            Position positionNew = new Position(item, client, socketClient);
                            positionNew.PropertyChanged += PositionNew_PropertyChanged;
                            App.Current.Dispatcher.Invoke(() => {
                                Main.Positions.Add(positionNew);
                            });
                        }
                    }
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
                        App.Current.Dispatcher.Invoke(() => {
                            Main.Positions.Remove(position);
                        });
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
                        if (!result.Success) Error.WriteLog(path, errorFile, $"Failed KeepAliveUserStreamAsync: {result.Error?.Message}");
                        else
                        {
                            Error.WriteLog(path, errorFile, "Success KeepAliveUserStreamAsync");
                        }
                    }
                    catch (Exception ex)
                    {
                        Error.WriteLog(path, errorFile, $"Exception KeepAliveUserStreamAsync: {ex?.Message}");
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
                    List<string>? symbols = new();
                    string pathFileSymbols = pathConfigs + "symbols";
                    if (File.Exists(pathFileSymbols))
                    {
                        string json = File.ReadAllText(pathFileSymbols);
                        symbols = JsonConvert.DeserializeObject<List<string>>(json);
                    }
                    foreach (var symbol in list)
                    {
                        if (symbol.QuoteAsset == "USDT")
                        {
                            decimal volume = 500000m;
                            if (configs != null)
                            {
                                Config? config = configs.FirstOrDefault(conf => conf.Name == symbol.Name);
                                if (config != null) volume = config.Volume;
                            }
                            SymbolViewModel symbolViewModel = new(symbol, volume, socketClient, client, LoginViewModel.Login.SelectedUser.IsTestnet);
                            OnOrderUpdate += symbolViewModel.ExchangeViewModel.OrderUpdate;
                            OnAccountUpdate += symbolViewModel.ExchangeViewModel.AccountUpdate;
                            symbolViewModel.Symbol.PropertyChanged += Symbol_PropertyChanged;
                            symbolViewModel.Symbol.Exchange.PropertyChanged += Exchange_PropertyChanged;
                            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                                Main.FullSymbols.Add(symbolViewModel.Symbol);
                            }));
                            if (symbols != null && symbols.Count > 0)
                            {
                                if (symbols.Contains(symbol.Name))
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
                Error.WriteLog(path, errorFile, $"Exception LoadSymbols: {ex?.Message}");
            }
        }

        private void Exchange_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Bet")
            {
                Exchange? exchange = sender as Exchange;
                if(exchange != null)
                {
                    App.Current.Dispatcher.BeginInvoke(new Action(() => {
                        Main.Bets.Insert(0, exchange.Bet);
                    }));
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
                                List<string>? symbols = new();
                                string pathFileSymbols = pathConfigs + "symbols";
                                if (File.Exists(pathFileSymbols))
                                {
                                    string json = File.ReadAllText(pathFileSymbols);
                                    symbols = JsonConvert.DeserializeObject<List<string>>(json);
                                }
                                if (symbols != null)
                                {
                                    symbols.Add(symbol.Name);
                                    File.WriteAllText(pathFileSymbols, JsonConvert.SerializeObject(symbols));
                                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        Main.Symbols.Add(symbol);
                                    }));
                                }
                            }
                        }
                        else
                        {
                            List<string>? symbols = new();
                            string pathFileSymbols = pathConfigs + "symbols";
                            if (File.Exists(pathFileSymbols))
                            {
                                string json = File.ReadAllText(pathFileSymbols);
                                symbols = JsonConvert.DeserializeObject<List<string>>(json);
                            }
                            if(symbols != null && symbols.Count > 0)
                            {
                                symbols.Remove(symbol.Name);
                                File.WriteAllText(pathFileSymbols, JsonConvert.SerializeObject(symbols));
                                App.Current.Dispatcher.BeginInvoke(new Action(() => {
                                    Main.Symbols.Remove(symbol);
                                }));
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    Error.WriteLog(path, errorFile, $"Exception Symbol_PropertyChanged: {ex.Message}");
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
                    Error.WriteLog(path, errorFile, $"Failed ListSymbols {result.Error?.Message}");
                    return new();
                }
                return result.Data.Symbols.ToList();

            }
            catch (Exception ex)
            {
                Error.WriteLog(path, errorFile, $"Exception ListSymbols: {ex.Message}");
                return new();
            }
        }
    }
}
