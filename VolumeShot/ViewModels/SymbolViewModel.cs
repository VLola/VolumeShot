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
        private string path = $"{Directory.GetCurrentDirectory()}/log/binance/";
        public Symbol Symbol { get; set; } = new();
        public ExchangeViewModel ExchangeViewModel { get; set; }
        public BinanceClient client { get; set; }
        public BinanceSocketClient socketClient { get; set; }
        public SymbolViewModel(BinanceFuturesUsdtSymbol binanceFuturesUsdtSymbol, decimal volume, BinanceSocketClient _socketClient, BinanceClient _client, bool isTestnet) {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            socketClient = _socketClient;
            client = _client;
            ExchangeViewModel = new ExchangeViewModel(binanceFuturesUsdtSymbol, _socketClient, _client);
            Symbol.Exchange = ExchangeViewModel.Exchange;
            Symbol.IsTestnet = isTestnet;
            Symbol.Name = binanceFuturesUsdtSymbol.Name;
            Symbol.Volume = volume;
            Symbol.PropertyChanged += Symbol_PropertyChanged;
            Symbol.Exchange.PropertyChanged += Exchange_PropertyChanged;
        }

        private void Exchange_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsWait")
            {
                if (Symbol.Exchange.IsWait)
                {
                    WaitAsync();
                }
            }
        }

        private async void WaitAsync()
        {
            await Task.Run(async () => {
                await Task.Delay(10000);
                Symbol.Exchange.IsWait = false;
                if (Symbol.IsRun) ReDistanceChengeVolumeAsync();
            });
        }

        private async void Run()
        {
            await Task.Run(async () =>
            {
                while (Symbol.IsRun)
                {
                    await Task.Delay(1000);
                    await CheckBufferAsync(); 
                }
            });
        }
        private void Order_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsRemove")
            {
                if(sender != null)
                {
                    Order order = (Order)sender;
                    order.PropertyChanged -= Order_PropertyChanged;
                    Symbol.Exchange.Orders.Remove(order);
                }
            }
        }
        private void Symbol_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Volume")
            {
                SaveVolumeAsync();
                if(Symbol.IsRun) ReDistanceChengeVolumeAsync();
            }
            else if (e.PropertyName == "IsRun")
            {
                if (Symbol.IsRun) {
                    SubscribeAsync();
                    Run();
                }
            }
            else if (e.PropertyName == "IsTrading")
            {
                if (Symbol.IsTrading)
                {
                    if (Symbol.IsRun) ReDistanceChengeVolumeAsync();
                }
                else
                {
                    ExchangeViewModel.ClosePositionsAsync();
                }
            }
            else if (e.PropertyName == "DistanceUpper")
            {
                Symbol.BufferUpper = Symbol.DistanceUpper / 5;
            }
            else if (e.PropertyName == "DistanceLower")
            {
                Symbol.BufferLower = Symbol.DistanceLower / 5;
            }
            if(e.PropertyName == "BestBidPrice"/* || e.PropertyName == "BestAskPrice"*/)
            {
                // Stop loss
                if (!ExchangeViewModel.Exchange.IsWorkedStopLoss)
                {
                    if (ExchangeViewModel.Exchange.IsOpenShortOrder && Symbol.BestBidPrice >= ExchangeViewModel.Exchange.StopLossShortPrice || ExchangeViewModel.Exchange.IsOpenLongOrder && Symbol.BestAskPrice <= ExchangeViewModel.Exchange.StopLossLongPrice)
                    {
                        ExchangeViewModel.ClosePositionsAsync();
                    }
                }
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
        private async Task CheckOpenOrder()
        {
            await Task.Run(async () =>
            {
                await Task.Delay(1500);
                await ReDistanceAsync();
            });
        }
        private async void ReDistanceChengeVolumeAsync()
        {
            ReBuffers();
            if (Symbol.IsTrading && !Symbol.Exchange.IsWait) await ExchangeViewModel.SetDistances(distanceUpper: Symbol.DistanceUpper, distanceLower: Symbol.DistanceLower, askPrice: Symbol.BestAskPrice, bidPrice: Symbol.BestBidPrice, bufferUpper: Symbol.BufferUpper, bufferLower: Symbol.BufferLower, bufferUpperPrice: Symbol.BufferUpperPrice, bufferLowerPrice: Symbol.BufferLowerPrice, volume: Symbol.Volume);
        }
        private async Task ReDistanceAsync()
        {
            if (!ExchangeViewModel.Exchange.IsOpenShortOrder && !ExchangeViewModel.Exchange.IsOpenLongOrder && Symbol.DistanceLower > 0m && Symbol.DistanceUpper > 0m && Symbol.BestAskPrice > 0m && Symbol.BestBidPrice > 0m)
            {
                if (Symbol.BufferLowerPrice >= Symbol.BestAskPrice || Symbol.BufferUpperPrice <= Symbol.BestBidPrice)
                {
                    ReBuffers();
                    if (Symbol.IsTrading && !Symbol.Exchange.IsWait) await ExchangeViewModel.SetDistances(distanceUpper: Symbol.DistanceUpper, distanceLower: Symbol.DistanceLower, askPrice: Symbol.BestAskPrice, bidPrice: Symbol.BestBidPrice, bufferUpper: Symbol.BufferUpper, bufferLower: Symbol.BufferLower, bufferUpperPrice: Symbol.BufferUpperPrice, bufferLowerPrice: Symbol.BufferLowerPrice, volume: Symbol.Volume);
                }
            }
            else if (!ExchangeViewModel.Exchange.IsOpenShortOrder && !ExchangeViewModel.Exchange.IsOpenLongOrder)
            {
                Symbol.BestAskPriceLast = Symbol.BestAskPrice;
                Symbol.BestBidPriceLast = Symbol.BestBidPrice;
            }
        }
        private void ReBuffers()
        {
            Symbol.BestAskPriceLast = Symbol.BestAskPrice;
            Symbol.BestBidPriceLast = Symbol.BestBidPrice;
            Symbol.BufferLowerPrice = Symbol.BestBidPriceLast - (Symbol.BestBidPriceLast / 100 * Symbol.BufferLower);
            Symbol.BufferUpperPrice = Symbol.BestAskPriceLast + (Symbol.BestAskPriceLast / 100 * Symbol.BufferUpper);
        }
        private async Task CheckBufferAsync()
        {
            await Task.Run(async () =>
            {
                if (Symbol.BufferLowerPrice == 0m || Symbol.BufferUpperPrice == 0m)
                {
                    await ReDistanceAsync();
                }
                if (!ExchangeViewModel.Exchange.IsOpenShortOrder && !ExchangeViewModel.Exchange.IsOpenLongOrder)
                {
                    if (Symbol.BufferLowerPrice >= Symbol.BestAskPrice || Symbol.BufferUpperPrice <= Symbol.BestBidPrice)
                    {
                        await CheckOpenOrder();
                    }
                }
            });
        }
        private async void SubscribeAsync()
        {
            try
            {
                await SubscribeToAggregatedTradeUpdatesAsync();
                await SubscribeToBookTickerUpdatesAsync();
                await GetOrderBookAsync();
                await SubscribeToOrderBookUpdatesAsync();
            }
            catch (Exception ex)
            {
                Error.WriteLog(path, Symbol.Name, $"Exception SubscribeAsync {ex.Message}");
            }
        }
        private async Task SubscribeToAggregatedTradeUpdatesAsync()
        {
            await Task.Run(async () =>
            {
                int socketId = 0;
                var result = await socketClient.UsdFuturesStreams.SubscribeToAggregatedTradeUpdatesAsync(Symbol.Name, Message =>
                {
                    if (!Symbol.IsRun)
                    {
                        socketClient.UnsubscribeAsync(socketId);
                        Symbol.Exchange.Orders.Clear();
                    }
                    Symbol.BuyerIsMaker = Message.Data.BuyerIsMaker;
                    Symbol.TradeTime = Message.Data.TradeTime;
                    Symbol.Price = Message.Data.Price;
                    Order order = new Order(Message.Data.Price, Message.Data.BuyerIsMaker, Message.Data.TradeTime);
                    order.PropertyChanged += Order_PropertyChanged;
                    Symbol.Exchange.Orders.Add(order);
                    if (Symbol.Exchange.IsOpenLongOrder || Symbol.Exchange.IsOpenShortOrder)
                    {
                        Symbol.Exchange.OpenBetOrders.Add(order);
                    }
                });
                if (!result.Success) Error.WriteLog(path, Symbol.Name, $"Failed SubscribeToAggregatedTradeUpdatesAsync: {result.Error?.Message}");
                else socketId = result.Data.Id;
            });
        }
        private async Task SubscribeToBookTickerUpdatesAsync()
        {
            await Task.Run(async () =>
            {
                int socketId = 0;
                var result = await socketClient.UsdFuturesStreams.SubscribeToBookTickerUpdatesAsync(Symbol.Name, Message =>
                {
                    if (!Symbol.IsRun)
                    {
                        socketClient.UnsubscribeAsync(socketId);
                    }
                    Symbol.BestAskPrice = Message.Data.BestAskPrice;
                    Symbol.BestBidPrice = Message.Data.BestBidPrice;
                });
                if (!result.Success) Error.WriteLog(path, Symbol.Name, $"Failed SubscribeToBookTickerUpdatesAsync: {result.Error?.Message}");
                else socketId = result.Data.Id;
            });
        }
        private async Task GetOrderBookAsync()
        {
            await Task.Run(async () =>
            {
                var result = await client.UsdFuturesApi.ExchangeData.GetOrderBookAsync(Symbol.Name, limit: 1000);
                if (!result.Success) Error.WriteLog(path, Symbol.Name, $"Failed GetOrderBookAsync: {result.Error?.Message}");
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
                int socketId = 0;
                int i = 0;
                var result = await socketClient.UsdFuturesStreams.SubscribeToOrderBookUpdatesAsync(Symbol.Name, updateInterval: 500, Message =>
                {
                    try
                    {
                        if (!Symbol.IsRun)
                        {
                            socketClient.UnsubscribeAsync(socketId);
                            Symbol.OrderBook.Bids.Clear();
                            Symbol.OrderBook.Asks.Clear();
                        }
                        // Bids
                        Symbol.OrderBook.AddBids(Message.Data.Bids);

                        // Asks
                        Symbol.OrderBook.AddAsks(Message.Data.Asks);


                        decimal maxBid = Symbol.OrderBook.MaxBid();
                        decimal priceBid = Symbol.OrderBook.GetPriceBids(Symbol.Volume);

                        decimal minAsk = Symbol.OrderBook.MinAsk();
                        decimal priceAsk = Symbol.OrderBook.GetPriceAsks(Symbol.Volume);

                        // Lower

                        decimal percentBid = (maxBid - priceBid) / priceBid * 100;
                        if (!Symbol.IsTestnet)
                        {
                            if (percentBid >= 0.5m) Symbol.DistanceLower = percentBid;
                            else if (priceBid > 0m) Symbol.DistanceLower = 0.5m;
                            else Symbol.DistanceLower = 1m;
                        }
                        else Symbol.DistanceLower = percentBid;

                        // Upper

                        decimal percentAsk = (priceAsk - minAsk) / minAsk * 100;
                        if (!Symbol.IsTestnet)
                        {
                            if (percentAsk >= 0.5m) Symbol.DistanceUpper = percentAsk;
                            else if (priceAsk > 0m) Symbol.DistanceUpper = 0.5m;
                            else Symbol.DistanceUpper = 1m;
                        }
                        else Symbol.DistanceUpper = percentAsk;

                    }
                    catch (Exception ex)
                    {
                        Error.WriteLog(path, Symbol.Name, $"Failed SubscribeToOrderBookUpdatesAsync: {ex.Message}");
                    }

                });
                if (!result.Success) Error.WriteLog(path, Symbol.Name, $"Failed SubscribeToOrderBookUpdatesAsync: {result.Error?.Message}");
                else socketId = result.Data.Id;
            });
        }
    }
}
