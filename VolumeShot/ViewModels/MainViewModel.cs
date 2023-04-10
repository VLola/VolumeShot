using Binance.Net.Clients;
using System.Collections.Generic;
using System.Linq;
using System;
using VolumeShot.Models;
using Binance.Net.Objects.Models.Futures;
using System.IO;
using Newtonsoft.Json;
using CryptoExchange.Net.Interfaces;
using System.Threading.Tasks;
using Binance.Net.Objects.Models.Futures.Socket;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using System.Windows;
using System.Security;
using ScottPlot.Drawing.Colormaps;
using ScottPlot;
using System.Drawing;

namespace VolumeShot.ViewModels
{
    internal class MainViewModel
    {
        private const double _second10 = 0.00011574074596865103;
        private const double _second60 = 0.0006944444394321181;
        string errorFile = "Binance";
        public Main Main { get; set; } = new();
        public BinanceClient client { get; set; }
        public BinanceSocketClient socketClient { get; set; }
        public delegate void AccountOnOrderUpdate(BinanceFuturesStreamOrderUpdate OrderUpdate);
        public event AccountOnOrderUpdate? OnOrderUpdate;
        public MainViewModel()
        {
            Binance("a4c675ddfa8005fdabf5580700bd87b2d0dff9108b1caa8295f5540e6cf118e5", "211c4565fb98ad121a10ce2cce9c31456890786cbce501ad426b0bbace6e1102", true);
            //Binance("L1YfcwRDUtoaAChO3OMLMWICx33zAZyj6hqWhjAfqAoIE9uAPHq9zYtlns4m7kFJ", "Ja5eBLb3yIFzivc2N5kbofPE2RTolMIyEBjZJsr51aCV3X98yGMhSpTeUGh75n0H", false);
            SubscribeToUserDataUpdatesAsync();
            Load();
            LoadChart();
            RunChart();
        }
        private void LoadChart()
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                Main.WpfPlot = new();
                Main.WpfPlot.Plot.AddScatterLines(Main.x, Main.bufferLower, Color.Gray, lineStyle: LineStyle.Dash);
                Main.WpfPlot.Plot.AddScatterLines(Main.x, Main.bufferUpper, Color.Gray, lineStyle: LineStyle.Dash);
                Main.WpfPlot.Plot.AddScatterLines(Main.x, Main.distanceLower, Color.Orange, lineStyle: LineStyle.Dash);
                Main.WpfPlot.Plot.AddScatterLines(Main.x, Main.distanceUpper, Color.Orange, lineStyle: LineStyle.Dash);
                Main.WpfPlot.Plot.AddScatterLines(Main.x, Main.takeProfit, Color.Green, lineStyle: LineStyle.Dash);
                Main.WpfPlot.Plot.AddScatterLines(Main.x, Main.stopLoss, Color.Red, lineStyle: LineStyle.Dash);
                Main.WpfPlot.Plot.RenderLock();
                Main.WpfPlot.Render();
                Main.WpfPlot.Plot.RenderUnlock();
            });
        }
        private async void RunChart()
        {
            await Task.Run(async () =>
            {
                while(true)
                {
                    await Task.Delay(100);
                    if(Main.SelectedSymbol != null)
                    {
                        if (Main.SelectedSymbol.BufferLowerPrice > 0m &&
                        Main.SelectedSymbol.BufferUpperPrice > 0m &&
                        Main.SelectedSymbol.Exchange.DistanceLowerPrice > 0m &&
                        Main.SelectedSymbol.Exchange.DistanceUpperPrice > 0m)
                        {
                            double dateTimeNew = DateTime.UtcNow.ToOADate();
                            double dateTimeOld = DateTime.UtcNow.AddMinutes(-1).ToOADate();
                            Main.x[0] = dateTimeOld;
                            Main.x[1] = dateTimeNew;
                            Main.bufferLower[0] = Main.bufferLower[1] = Decimal.ToDouble(Main.SelectedSymbol.BufferLowerPrice);
                            Main.bufferUpper[0] = Main.bufferUpper[1] = Decimal.ToDouble(Main.SelectedSymbol.BufferUpperPrice);
                            Main.distanceLower[0] = Main.distanceLower[1] = Decimal.ToDouble(Main.SelectedSymbol.Exchange.DistanceLowerPrice);
                            Main.distanceUpper[0] = Main.distanceUpper[1] = Decimal.ToDouble(Main.SelectedSymbol.Exchange.DistanceUpperPrice);
                            if (Main.SelectedSymbol.Exchange.IsOpenLongOrder)
                            {
                                Main.takeProfit[0] = Main.takeProfit[1] = Decimal.ToDouble(Main.SelectedSymbol.Exchange.TakeProfitLong);
                                Main.stopLoss[0] = Main.stopLoss[1] = Decimal.ToDouble(Main.SelectedSymbol.Exchange.StopLossLong);
                            }
                            else if (Main.SelectedSymbol.Exchange.IsOpenShortOrder)
                            {
                                Main.takeProfit[0] = Main.takeProfit[1] = Decimal.ToDouble(Main.SelectedSymbol.Exchange.TakeProfitShort);
                                Main.stopLoss[0] = Main.stopLoss[1] = Decimal.ToDouble(Main.SelectedSymbol.Exchange.StopLossShort);
                            }
                            else
                            {
                                Main.takeProfit[0] = Main.takeProfit[1] = 0;
                                Main.stopLoss[0] = Main.stopLoss[1] = 0;
                            }
                            SetY(Main.distanceLower[0], Main.distanceUpper[0]);
                            AutoAxisX(dateTimeNew);
                        }
                    }
                }
            });
        }
        private void SetY(double lowerDistance, double upperDistance)
        {
            Main.WpfPlot.Dispatcher.Invoke(new Action(() =>
            {
                Main.WpfPlot.Plot.RenderLock();
                Main.WpfPlot.Plot.SetAxisLimitsY(yMin: lowerDistance - (lowerDistance / 300), yMax: upperDistance + (upperDistance / 300));
                Main.WpfPlot.Render();
                Main.WpfPlot.Plot.RenderUnlock();
            }));
        }
        private void AutoAxisX(double dateTime)
        {
            Main.WpfPlot.Dispatcher.Invoke(new Action(() =>
            {
                Main.WpfPlot.Plot.RenderLock();
                Main.WpfPlot.Plot.SetAxisLimitsX(xMin: dateTime - _second60, xMax: dateTime + _second10);
                Main.WpfPlot.Render();
                Main.WpfPlot.Plot.RenderUnlock();
            }));
        }
        private void Binance(string apiKey, string secretKey, bool isTestnet)
        {
            if (isTestnet)
            {
                // ------------- Test Api ----------------
                BinanceClientOptions clientOption = new();
                clientOption.UsdFuturesApiOptions.BaseAddress = "https://testnet.binancefuture.com";
                client = new(clientOption);

                BinanceSocketClientOptions socketClientOption = new BinanceSocketClientOptions();
                socketClientOption.UsdFuturesStreamsOptions.AutoReconnect = true;
                socketClientOption.UsdFuturesStreamsOptions.ReconnectInterval = TimeSpan.FromMinutes(1);
                socketClientOption.UsdFuturesStreamsOptions.BaseAddress = "wss://stream.binancefuture.com";
                socketClient = new BinanceSocketClient(socketClientOption);
                // ------------- Test Api ----------------
            }
            else
            {
                // ------------- Real Api ----------------
                client = new();
                socketClient = new();
                // ------------- Real Api ----------------
            }

            try
            {
                client.SetApiCredentials(new BinanceApiCredentials(apiKey, secretKey));
                socketClient.SetApiCredentials(new BinanceApiCredentials(apiKey, secretKey));

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async void SubscribeToUserDataUpdatesAsync()
        {
            var listenKey = await client.UsdFuturesApi.Account.StartUserStreamAsync();
            if (!listenKey.Success)
            {
                Error.WriteLog("", errorFile, $"Failed to start user stream: listenKey");
            }
            else
            {
                KeepAliveUserStreamAsync(listenKey.Data);
                Error.WriteLog("", errorFile, $"Listen Key Created");
                var result = await socketClient.UsdFuturesStreams.SubscribeToUserDataUpdatesAsync(listenKey: listenKey.Data,
                    onLeverageUpdate => { },
                    onMarginUpdate => { },
                    onAccountUpdate => { },
                    onOrderUpdate =>
                    {
                        OnOrderUpdate?.Invoke(onOrderUpdate.Data);
                    },
                    onListenKeyExpired => { },
                    onStrategyUpdate => { },
                    onGridUpdate => { });
                if (!result.Success)
                {
                    Error.WriteLog("", errorFile, $"Failed UserDataUpdates: {result.Error?.Message}");
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
                    if (!result.Success) Error.WriteLog("", errorFile, $"Failed KeepAliveUserStreamAsync: {result.Error?.Message}");
                    else
                    {
                        Error.WriteLog("", errorFile, "Success KeepAliveUserStreamAsync");
                    }
                    await Task.Delay(900000);
                }
            });
        }
        private void Load()
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
                        SymbolViewModel symbolViewModel = new(symbol, volume, socketClient, client);
                        OnOrderUpdate += symbolViewModel.ExchangeViewModel.OrderUpdate;
                        Main.Symbols.Add(symbolViewModel.Symbol);
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
                    Error.WriteLog("", errorFile, $"Failed ListSymbols {result.Error?.Message}");
                    return new();
                }
                return result.Data.Symbols.ToList();

            }
            catch (Exception ex)
            {
                Error.WriteLog("", errorFile, $"Exception ListSymbols: {ex.Message}");
                return new();
            }
        }
    }
}
