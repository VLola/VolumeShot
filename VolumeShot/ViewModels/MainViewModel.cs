using Binance.Net.Clients;
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

namespace VolumeShot.ViewModels
{
    internal class MainViewModel
    {
        private string path = $"{Directory.GetCurrentDirectory()}/log/";
        string pathPositions = Directory.GetCurrentDirectory() + "/log/positions/";
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
        public MainViewModel()
        {
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
            SubscribeToUserDataUpdatesAsync();
            LoadSymbols();
            LoadChart();
            RunChartAsync();
        }

        private void LoadChart()
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
        private async void RunChartAsync()
        {
            await Task.Run(async () =>
            {
                while(true)
                {
                    await Task.Delay(100);
                    if(Main.SelectedSymbol != null)
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
        private async void SubscribeToUserDataUpdatesAsync()
        {
            var resultPositions = await client.UsdFuturesApi.Account.GetPositionInformationAsync();
            if (!resultPositions.Success)
            {
                Error.WriteLog(path, errorFile, $"Failed GetPositionInformationAsync: {resultPositions.Error?.Message}");
            }
            else
            {
                AddAllPositionsAsync(resultPositions.Data);
            }
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
                    var result = await client.UsdFuturesApi.Account.KeepAliveUserStreamAsync(listenKey);
                    if (!result.Success) Error.WriteLog(path, errorFile, $"Failed KeepAliveUserStreamAsync: {result.Error?.Message}");
                    else
                    {
                        Error.WriteLog(path, errorFile, "Success KeepAliveUserStreamAsync");
                    }
                    await Task.Delay(900000);
                }
            });
        }
        private void LoadSymbols()
        {
            List<BinanceFuturesUsdtSymbol> list = ListSymbols();
            if (list.Count > 0)
            {
                List<Config>? configs = new();
                string path = Directory.GetCurrentDirectory() + "/config";
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    configs = JsonConvert.DeserializeObject<List<Config>>(json);
                }
                foreach (var symbol in list)
                {
                    decimal volume = 500000m;
                    if (symbol.QuoteAsset == "USDT")
                    {
                        if (configs != null)
                        {
                            Config? config = configs.FirstOrDefault(conf => conf.Name == symbol.Name);
                            if (config != null) volume = config.Volume;
                        }
                        SymbolViewModel symbolViewModel = new(symbol, volume, socketClient, client, LoginViewModel.Login.SelectedUser.IsTestnet);
                        OnOrderUpdate += symbolViewModel.ExchangeViewModel.OrderUpdate;
                        OnAccountUpdate += symbolViewModel.ExchangeViewModel.AccountUpdate;
                        App.Current.Dispatcher.BeginInvoke(new Action(() => { 
                            Main.Symbols.Add(symbolViewModel.Symbol);
                        }));
                    }
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
