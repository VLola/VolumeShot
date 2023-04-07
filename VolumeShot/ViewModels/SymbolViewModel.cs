using Binance.Net.Clients;
using Binance.Net.Objects.Models.Futures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VolumeShot.Models;

namespace VolumeShot.ViewModels
{
    internal class SymbolViewModel
    {
        public Symbol Symbol { get; set; } = new();
        public SymbolViewModel(BinanceFuturesUsdtSymbol binanceFuturesUsdtSymbol, decimal volume) {
            Symbol.Name = binanceFuturesUsdtSymbol.Name;
            Symbol.Volume = volume;
            Symbol.PropertyChanged += Symbol_PropertyChanged;
        }

        private void Symbol_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Volume")
            {
                SaveVolumeAsync();
            }
            if (e.PropertyName == "IsRun")
            {
                if(Symbol.IsRun) SubscribeAsync();
            }
            if (e.PropertyName == "BestBidPrice")
            {
                if (Symbol.IsOpenShortOrder)
                {
                    // Stop Loss Short
                    decimal price = Symbol.OpenShortOrderPrice + (Symbol.OpenShortOrderPrice / 100 * Symbol.StopLoss);
                    if (Symbol.BestBidPrice >= price)
                    {
                        CloseBet(Symbol.BestBidPrice);
                        Symbol.BestAskPriceLast = Symbol.BestAskPrice;
                        Symbol.BestBidPriceLast = Symbol.BestBidPrice;
                        Symbol.ShortMinus += 1;
                        Symbol.IsOpenShortOrder = false;
                    }
                }
                if (Symbol.IsOpenLongOrder)
                {
                    // Take Profit Long
                    decimal price = Symbol.OpenLongOrderPrice + (Symbol.OpenLongOrderPrice / 100 * Symbol.TakeProfit);
                    if (Symbol.BestBidPrice >= price)
                    {
                        CloseBet(Symbol.BestBidPrice);
                        Symbol.BestAskPriceLast = Symbol.BestAskPrice;
                        Symbol.BestBidPriceLast = Symbol.BestBidPrice;
                        Symbol.LongPlus += 1;
                        Symbol.IsOpenLongOrder = false;
                    }
                }
            }
            else if (e.PropertyName == "BestAskPrice")
            {
                if (Symbol.IsOpenShortOrder)
                {
                    // Take Profit Short
                    decimal price = Symbol.OpenShortOrderPrice - (Symbol.OpenShortOrderPrice / 100 * Symbol.TakeProfit);
                    if (Symbol.BestAskPrice <= price)
                    {
                        CloseBet(Symbol.BestAskPrice);
                        Symbol.BestAskPriceLast = Symbol.BestAskPrice;
                        Symbol.BestBidPriceLast = Symbol.BestBidPrice;
                        Symbol.ShortPlus += 1;
                        Symbol.IsOpenShortOrder = false;
                    }
                }
                if (Symbol.IsOpenLongOrder)
                {
                    // Stop Loss Long
                    decimal price = Symbol.OpenLongOrderPrice - (Symbol.OpenLongOrderPrice / 100 * Symbol.StopLoss);
                    if (Symbol.BestAskPrice <= price)
                    {
                        CloseBet(Symbol.BestAskPrice);
                        Symbol.BestAskPriceLast = Symbol.BestAskPrice;
                        Symbol.BestBidPriceLast = Symbol.BestBidPrice;
                        Symbol.LongMinus += 1;
                        Symbol.IsOpenLongOrder = false;
                    }
                }
            }
            if(e.PropertyName == "BestBidPrice" || e.PropertyName == "BestAskPrice")
            {
                CheckBuffersAsync();
            }
        }
        private async void SaveVolumeAsync()
        {
            await Task.Run(() =>
            {
                List<Config>? configs = new();
                string path = Directory.GetCurrentDirectory() + "/config";
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    configs = JsonConvert.DeserializeObject<List<Config>>(json);
                    if (configs != null)
                    {
                        Config? config = configs.FirstOrDefault(conf => conf.Name == Symbol.Name);
                        if (config != null)
                        {
                            configs.Remove(config);
                        }
                        configs.Add(new Config() { Name = Symbol.Name, Volume = Symbol.Volume });
                        string json1 = JsonConvert.SerializeObject(configs);
                        File.WriteAllText(path, json1);
                    }
                }
                else
                {
                    configs.Add(new Config() { Name = Symbol.Name, Volume = Symbol.Volume });
                    string json = JsonConvert.SerializeObject(configs);
                    File.WriteAllText(path, json);
                }
            });
        }
        private void NewBet(decimal openPrice)
        {
            Bet bet = new();
            bet.OpenTime = DateTime.UtcNow;
            bet.OpenPrice = openPrice;
            bet.PriceBufferLower = Symbol.BestBidPriceLast - (Symbol.BestBidPriceLast / 100 * Symbol.BufferLower);
            bet.PriceDistanceLower = Symbol.BestBidPriceLast - (Symbol.BestBidPriceLast / 100 * Symbol.DistanceLower);
            bet.PriceBufferUpper = Symbol.BestAskPriceLast + (Symbol.BestAskPriceLast / 100 * Symbol.BufferUpper);
            bet.PriceDistanceUpper = Symbol.BestAskPriceLast + (Symbol.BestAskPriceLast / 100 * Symbol.DistanceUpper);
            if (Symbol.IsOpenLongOrder)
            {
                bet.PriceStopLoss = Symbol.OpenLongOrderPrice - (Symbol.OpenLongOrderPrice / 100 * Symbol.StopLoss);
                bet.PriceTakeProfit = Symbol.OpenLongOrderPrice + (Symbol.OpenLongOrderPrice / 100 * Symbol.TakeProfit);
            }
            if (Symbol.IsOpenShortOrder)
            {
                bet.PriceStopLoss = Symbol.OpenShortOrderPrice + (Symbol.OpenShortOrderPrice / 100 * Symbol.StopLoss);
                bet.PriceTakeProfit = Symbol.OpenShortOrderPrice - (Symbol.OpenShortOrderPrice / 100 * Symbol.TakeProfit);
            }
            App.Current.Dispatcher.Invoke(new Action(() => {
                Symbol.Bets.Add(bet);
            }));
        }
        private void CloseBet(decimal closePrice)
        {
            int count = Symbol.Bets.Count - 1;
            Bet bet = Symbol.Bets[count];
            bet.CloseTime = DateTime.UtcNow;
            bet.ClosePrice = closePrice;
            Order[] orders = Symbol.Orders.ToArray();
            bet.Orders = orders.Where(order => order.DateTime >= bet.OpenTime.AddSeconds(-20));
            Symbol.Orders.Clear();
        }

        private async void CheckBuffersAsync()
        {
            await Task.Run(async () =>
            {
                await CheckBufferUpperAsync();
                await CheckBufferLowerAsync();
            });
        }
        private async void ReBuffers()
        {
            await Task.Run(async() =>
            {
                await Task.Delay(1500);
                if (!Symbol.IsOpenShortOrder && !Symbol.IsOpenLongOrder)
                {
                    Symbol.BestAskPriceLast = Symbol.BestAskPrice;
                    Symbol.BestBidPriceLast = Symbol.BestBidPrice;
                }
            });
        }
        private async Task CheckBufferLowerAsync()
        {
            await Task.Run(async () =>
            {
                if (Symbol.BestBidPriceLast > 0m)
                {
                    if(!Symbol.IsOpenShortOrder && !Symbol.IsOpenLongOrder)
                    {
                        decimal price = Symbol.BestBidPriceLast - (Symbol.BestBidPriceLast / 100 * Symbol.BufferLower);
                        if (price >= Symbol.BestAskPrice)
                        {
                            ReBuffers();
                            await CheckDistanceLowerAsync();
                        }
                    }
                }
                else
                {
                    Symbol.BestAskPriceLast = Symbol.BestAskPrice;
                    Symbol.BestBidPriceLast = Symbol.BestBidPrice;
                }
            });
        }
        private async Task CheckDistanceLowerAsync()
        {
            await Task.Run(async () =>
            {
                decimal price = Symbol.BestBidPriceLast - (Symbol.BestBidPriceLast / 100 * Symbol.DistanceLower);
                if (price >= Symbol.BestAskPrice)
                {
                    if (!Symbol.IsOpenShortOrder && !Symbol.IsOpenLongOrder)
                    {
                        Symbol.IsOpenLongOrder = true;
                        Symbol.OpenLongOrderPrice = Symbol.BestAskPrice;
                        NewBet(Symbol.BestAskPrice);
                    }
                }
            });
        }
        private async Task CheckBufferUpperAsync()
        {
            await Task.Run(async () =>
            {
                if (Symbol.BestAskPriceLast > 0m)
                {
                    if(!Symbol.IsOpenShortOrder && !Symbol.IsOpenLongOrder)
                    {
                        decimal price = Symbol.BestAskPriceLast + (Symbol.BestAskPriceLast / 100 * Symbol.BufferUpper);
                        if (price <= Symbol.BestBidPrice)
                        {
                            ReBuffers();
                            await CheckDistanceUpperAsync();
                        }
                    }
                }
                else
                {
                    Symbol.BestAskPriceLast = Symbol.BestAskPrice;
                    Symbol.BestBidPriceLast = Symbol.BestBidPrice;
                }
            });
        }
        private async Task CheckDistanceUpperAsync()
        {
            await Task.Run(async () =>
            {
                decimal price = Symbol.BestAskPriceLast + (Symbol.BestAskPriceLast / 100 * Symbol.DistanceUpper);
                if (price <= Symbol.BestBidPrice)
                {
                    if (!Symbol.IsOpenShortOrder && !Symbol.IsOpenLongOrder)
                    {
                        Symbol.IsOpenShortOrder = true;
                        Symbol.OpenShortOrderPrice = Symbol.BestBidPrice;
                        NewBet(Symbol.BestBidPrice);
                    }
                }
            });
        }
        private async void SubscribeAsync()
        {
            try
            {
                await SubscribeToBookTickerUpdatesAsync();
                await GetOrderBookAsync();
                await SubscribeToOrderBookUpdatesAsync();
            }
            catch (Exception ex)
            {
                Error.WriteLog("binance", Symbol.Name, $"Exception SubscribeAsync {ex.Message}");
            }
        }
        private async Task SubscribeToBookTickerUpdatesAsync()
        {
            await Task.Run(async () =>
            {
                BinanceSocketClient socketClient = new();
                int socketId = 0;
                var result = await socketClient.UsdFuturesStreams.SubscribeToBookTickerUpdatesAsync(Symbol.Name, Message =>
                {
                    if (!Symbol.IsRun)
                    {
                        socketClient.UnsubscribeAsync(socketId);
                        Symbol.Orders.Clear();
                    }
                    Symbol.BestAskPrice = Message.Data.BestAskPrice;
                    Symbol.BestBidPrice = Message.Data.BestBidPrice;
                    Symbol.Orders.Add(new Order() { BestAskPrice = Message.Data.BestAskPrice, BestBidPrice = Message.Data.BestBidPrice, DateTime = DateTime.UtcNow });
                });
                if (!result.Success) Error.WriteLog("binance", Symbol.Name, $"Failed SubscribeToBookTickerUpdatesAsync: {result.Error?.Message}");
                else socketId = result.Data.Id;
            });
        }
        private async Task GetOrderBookAsync()
        {
            await Task.Run(async () =>
            {
                BinanceClient client = new BinanceClient();
                var result = await client.UsdFuturesApi.ExchangeData.GetOrderBookAsync(Symbol.Name, limit: 1000);
                if (!result.Success) Error.WriteLog("binance", Symbol.Name, $"Failed GetOrderBookAsync: {result.Error?.Message}");
                else
                {
                    Symbol.OrderBook.Bids.Clear();
                    Symbol.OrderBook.Asks.Clear();
                    Symbol.OrderBook.AddBids(result.Data.Bids);
                    Symbol.OrderBook.AddAsks(result.Data.Asks);
                }
            });
        }
        private async Task SubscribeToOrderBookUpdatesAsync()
        {
            await Task.Run(async () =>
            {
                BinanceSocketClient socketClient = new();
                int socketId = 0;
                int i = 0;
                var result = await socketClient.UsdFuturesStreams.SubscribeToOrderBookUpdatesAsync(Symbol.Name, updateInterval: 500, Message =>
                {
                    if (!Symbol.IsRun)
                    {
                        socketClient.UnsubscribeAsync(socketId);
                        Symbol.OrderBook.Bids.Clear();
                        Symbol.OrderBook.Asks.Clear();
                    }

                    try
                    {

                        // Bids
                        Symbol.OrderBook.AddBids(Message.Data.Bids);

                        // Asks
                        Symbol.OrderBook.AddAsks(Message.Data.Asks);


                        decimal maxBid = Symbol.OrderBook.MaxBid();
                        decimal percentBid = Symbol.OrderBook.GetPriceBids(Symbol.Volume);
                        Symbol.DistanceLower = (maxBid - percentBid) / percentBid * 100;

                        decimal minAsk = Symbol.OrderBook.MinAsk();
                        decimal percentAsk = Symbol.OrderBook.GetPriceAsks(Symbol.Volume);
                        Symbol.DistanceUpper = (percentAsk - minAsk) / minAsk * 100;
                    }
                    catch (Exception ex)
                    {
                        Error.WriteLog("binance", Symbol.Name, $"Failed SubscribeToOrderBookUpdatesAsync: {ex.Message}");
                    }

                });
                if (!result.Success) Error.WriteLog("binance", Symbol.Name, $"Failed SubscribeToOrderBookUpdatesAsync: {result.Error?.Message}");
                else socketId = result.Data.Id;
            });
        }
    }
}
