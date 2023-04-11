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
        private string path = "binance";
        private string directory = $"{Directory.GetCurrentDirectory()}/log/";
        public Symbol Symbol { get; set; } = new();
        public ExchangeViewModel ExchangeViewModel { get; set; }
        public BinanceClient client { get; set; }
        public BinanceSocketClient socketClient { get; set; }
        public SymbolViewModel(BinanceFuturesUsdtSymbol binanceFuturesUsdtSymbol, decimal volume, BinanceSocketClient _socketClient, BinanceClient _client) {

            if (!Directory.Exists(directory + path)) Directory.CreateDirectory(directory + path);
            socketClient = _socketClient;
            client = _client;
            ExchangeViewModel = new ExchangeViewModel(binanceFuturesUsdtSymbol, _socketClient, _client);
            Symbol.Exchange = ExchangeViewModel.Exchange;
            Symbol.Name = binanceFuturesUsdtSymbol.Name;
            Symbol.Volume = volume;
            Symbol.PropertyChanged += Symbol_PropertyChanged;
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

        public List<Order> Orders { get; set; } = new();
        private void Order_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsRemove")
            {
                if(sender != null)
                {
                    Order order = (Order)sender;
                    order.PropertyChanged -= Order_PropertyChanged;
                    Orders.Remove(order);
                }
            }
        }
        private void Symbol_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Volume")
            {
                SaveVolumeAsync();
            }
            else if (e.PropertyName == "IsRun")
            {
                if (Symbol.IsRun) {
                    SubscribeAsync();
                    Run();
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
                if (ExchangeViewModel.Exchange.IsOpenShortOrder && Symbol.BestBidPrice >= ExchangeViewModel.Exchange.StopLossShort || ExchangeViewModel.Exchange.IsOpenLongOrder && Symbol.BestAskPrice <= ExchangeViewModel.Exchange.StopLossLong)
                {
                    ExchangeViewModel.ClearOrdersToSymbolAsync();
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
        private async Task ReDistanceAsync()
        {
            if (!ExchangeViewModel.Exchange.IsOpenShortOrder && !ExchangeViewModel.Exchange.IsOpenLongOrder && Symbol.DistanceLower > 0m && Symbol.DistanceUpper > 0m && Symbol.BestAskPrice > 0m && Symbol.BestBidPrice > 0m)
            {
                if (Symbol.BufferLowerPrice >= Symbol.BestAskPrice || Symbol.BufferUpperPrice <= Symbol.BestBidPrice)
                {
                    ReBuffers();
                    await ExchangeViewModel.SetDistances(distanceUpper: Symbol.DistanceUpper, distanceLower: Symbol.DistanceLower, askPrice: Symbol.BestAskPrice, bidPrice: Symbol.BestBidPrice);
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
        //private void Symbol_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "Volume")
        //    {
        //        SaveVolumeAsync();
        //    }
        //    else if (e.PropertyName == "IsRun")
        //    {
        //        if (Symbol.IsRun) SubscribeAsync();
        //    }
        //    else if (e.PropertyName == "DistanceUpper")
        //    {
        //        Symbol.BufferUpper = Symbol.DistanceUpper / 5;
        //    }
        //    else if (e.PropertyName == "DistanceLower")
        //    {
        //        Symbol.BufferLower = Symbol.DistanceLower / 5;
        //    }
        //    //else if (e.PropertyName == "BestBidPrice")
        //    //{
        //    //    if (Symbol.IsOpenShortOrder)
        //    //    {
        //    //        // Stop Loss Short
        //    //        decimal price = Symbol.OpenShortOrderPrice + (Symbol.OpenShortOrderPrice / 100 * Symbol.StopLoss);
        //    //        if (Symbol.BestBidPrice >= price)
        //    //        {
        //    //            CloseBet(Symbol.BestBidPrice);
        //    //            Symbol.BestAskPriceLast = Symbol.BestAskPrice;
        //    //            Symbol.BestBidPriceLast = Symbol.BestBidPrice;
        //    //            Symbol.ShortMinus += 1;
        //    //            Symbol.IsOpenShortOrder = false;
        //    //        }
        //    //    }
        //    //    if (Symbol.IsOpenLongOrder)
        //    //    {
        //    //        // Take Profit Long
        //    //        decimal price = Symbol.OpenLongOrderPrice + (Symbol.OpenLongOrderPrice / 100 * Symbol.TakeProfit);
        //    //        if (Symbol.BestBidPrice >= price)
        //    //        {
        //    //            CloseBet(Symbol.BestBidPrice);
        //    //            Symbol.BestAskPriceLast = Symbol.BestAskPrice;
        //    //            Symbol.BestBidPriceLast = Symbol.BestBidPrice;
        //    //            Symbol.LongPlus += 1;
        //    //            Symbol.IsOpenLongOrder = false;
        //    //        }
        //    //    }
        //    //}
        //    //else if (e.PropertyName == "BestAskPrice")
        //    //{
        //    //    if (Symbol.IsOpenShortOrder)
        //    //    {
        //    //        // Take Profit Short
        //    //        decimal price = Symbol.OpenShortOrderPrice - (Symbol.OpenShortOrderPrice / 100 * Symbol.TakeProfit);
        //    //        if (Symbol.BestAskPrice <= price)
        //    //        {
        //    //            CloseBet(Symbol.BestAskPrice);
        //    //            Symbol.BestAskPriceLast = Symbol.BestAskPrice;
        //    //            Symbol.BestBidPriceLast = Symbol.BestBidPrice;
        //    //            Symbol.ShortPlus += 1;
        //    //            Symbol.IsOpenShortOrder = false;
        //    //        }
        //    //    }
        //    //    if (Symbol.IsOpenLongOrder)
        //    //    {
        //    //        // Stop Loss Long
        //    //        decimal price = Symbol.OpenLongOrderPrice - (Symbol.OpenLongOrderPrice / 100 * Symbol.StopLoss);
        //    //        if (Symbol.BestAskPrice <= price)
        //    //        {
        //    //            CloseBet(Symbol.BestAskPrice);
        //    //            Symbol.BestAskPriceLast = Symbol.BestAskPrice;
        //    //            Symbol.BestBidPriceLast = Symbol.BestBidPrice;
        //    //            Symbol.LongMinus += 1;
        //    //            Symbol.IsOpenLongOrder = false;
        //    //        }
        //    //    }
        //    //}
        //    if (e.PropertyName == "BestBidPrice" || e.PropertyName == "BestAskPrice")
        //    {
        //        CheckBuffersAsync();
        //    }
        //}
        //private void NewBet(decimal openPrice)
        //{
        //    Bet bet = new();
        //    bet.OpenTime = DateTime.UtcNow;
        //    bet.OpenPrice = openPrice;
        //    bet.PriceBufferLower = Symbol.BestBidPriceLast - (Symbol.BestBidPriceLast / 100 * Symbol.BufferLower);
        //    bet.PriceDistanceLower = Symbol.BestBidPriceLast - (Symbol.BestBidPriceLast / 100 * Symbol.DistanceLower);
        //    bet.PriceBufferUpper = Symbol.BestAskPriceLast + (Symbol.BestAskPriceLast / 100 * Symbol.BufferUpper);
        //    bet.PriceDistanceUpper = Symbol.BestAskPriceLast + (Symbol.BestAskPriceLast / 100 * Symbol.DistanceUpper);
        //    bet.BufferLower = Symbol.BufferLower;
        //    bet.DistanceLower = Symbol.DistanceLower;
        //    bet.BufferUpper = Symbol.BufferUpper;
        //    bet.DistanceUpper = Symbol.DistanceUpper;
        //    if (Symbol.IsOpenLongOrder)
        //    {
        //        Symbol.StopLoss = Symbol.DistanceLower / 2;
        //        Symbol.TakeProfit = Symbol.DistanceLower / 5;
        //        bet.PriceStopLoss = Symbol.OpenLongOrderPrice - (Symbol.OpenLongOrderPrice / 100 * Symbol.StopLoss);
        //        bet.PriceTakeProfit = Symbol.OpenLongOrderPrice + (Symbol.OpenLongOrderPrice / 100 * Symbol.TakeProfit);
        //        bet.StopLoss = Symbol.StopLoss;
        //        bet.TakeProfit = Symbol.TakeProfit;
        //    }
        //    if (Symbol.IsOpenShortOrder)
        //    {
        //        Symbol.StopLoss = Symbol.DistanceUpper / 2;
        //        Symbol.TakeProfit = Symbol.DistanceUpper / 5;
        //        bet.PriceStopLoss = Symbol.OpenShortOrderPrice + (Symbol.OpenShortOrderPrice / 100 * Symbol.StopLoss);
        //        bet.PriceTakeProfit = Symbol.OpenShortOrderPrice - (Symbol.OpenShortOrderPrice / 100 * Symbol.TakeProfit);
        //        bet.StopLoss = Symbol.StopLoss;
        //        bet.TakeProfit = Symbol.TakeProfit;
        //    }
        //    App.Current.Dispatcher.Invoke(new Action(() => {
        //        Symbol.Bets.Add(bet);
        //    }));
        //}
        //private void CloseBet(decimal closePrice)
        //{
        //    int count = Symbol.Bets.Count - 1;
        //    Bet bet = Symbol.Bets[count];
        //    bet.CloseTime = DateTime.UtcNow;
        //    bet.ClosePrice = closePrice;
        //    Order[] orders = Symbol.Orders.ToArray();
        //    bet.Orders = orders.Where(order => order.DateTime >= bet.OpenTime.AddSeconds(-20));
        //    Symbol.Orders.Clear();
        //}

        //private async void CheckBuffersAsync()
        //{
        //    await Task.Run(async () =>
        //    {
        //        await CheckBufferUpperAsync();
        //        await CheckBufferLowerAsync();
        //    });
        //}
        //private async void ReBuffers()
        //{
        //    await Task.Run(async() =>
        //    {
        //        await Task.Delay(1500);
        //        if (!Symbol.IsOpenShortOrder && !Symbol.IsOpenLongOrder)
        //        {
        //            Symbol.BestAskPriceLast = Symbol.BestAskPrice;
        //            Symbol.BestBidPriceLast = Symbol.BestBidPrice;
        //        }
        //    });
        //}
        //private async Task CheckBufferLowerAsync()
        //{
        //    await Task.Run(async () =>
        //    {
        //        if (Symbol.BestBidPriceLast > 0m && Symbol.DistanceUpper > 0m)
        //        {
        //            if(!Symbol.IsOpenShortOrder && !Symbol.IsOpenLongOrder)
        //            {
        //                decimal price = Symbol.BestBidPriceLast - (Symbol.BestBidPriceLast / 100 * Symbol.BufferLower);
        //                if (price >= Symbol.BestAskPrice)
        //                {
        //                    ReBuffers();
        //                    await CheckDistanceLowerAsync();
        //                }
        //            }
        //        }
        //        else
        //        {
        //            Symbol.BestAskPriceLast = Symbol.BestAskPrice;
        //            Symbol.BestBidPriceLast = Symbol.BestBidPrice;
        //        }
        //    });
        //}
        //private async Task CheckDistanceLowerAsync()
        //{
        //    await Task.Run(async () =>
        //    {
        //        decimal price = Symbol.BestBidPriceLast - (Symbol.BestBidPriceLast / 100 * Symbol.DistanceLower);
        //        if (price >= Symbol.BestAskPrice)
        //        {
        //            if (!Symbol.IsOpenShortOrder && !Symbol.IsOpenLongOrder)
        //            {
        //                Symbol.IsOpenLongOrder = true;
        //                Symbol.OpenLongOrderPrice = Symbol.BestAskPrice;
        //                NewBet(Symbol.BestAskPrice);
        //            }
        //        }
        //    });
        //}
        //private async Task CheckBufferUpperAsync()
        //{
        //    await Task.Run(async () =>
        //    {
        //        if (Symbol.BestAskPriceLast > 0m && Symbol.DistanceUpper > 0m)
        //        {
        //            if(!Symbol.IsOpenShortOrder && !Symbol.IsOpenLongOrder)
        //            {
        //                decimal price = Symbol.BestAskPriceLast + (Symbol.BestAskPriceLast / 100 * Symbol.BufferUpper);
        //                if (price <= Symbol.BestBidPrice)
        //                {
        //                    ReBuffers();
        //                    await CheckDistanceUpperAsync();
        //                }
        //            }
        //        }
        //        else
        //        {
        //            Symbol.BestAskPriceLast = Symbol.BestAskPrice;
        //            Symbol.BestBidPriceLast = Symbol.BestBidPrice;
        //        }
        //    });
        //}
        //private async Task CheckDistanceUpperAsync()
        //{
        //    await Task.Run(async () =>
        //    {
        //        decimal price = Symbol.BestAskPriceLast + (Symbol.BestAskPriceLast / 100 * Symbol.DistanceUpper);
        //        if (price <= Symbol.BestBidPrice)
        //        {
        //            if (!Symbol.IsOpenShortOrder && !Symbol.IsOpenLongOrder)
        //            {
        //                Symbol.IsOpenShortOrder = true;
        //                Symbol.OpenShortOrderPrice = Symbol.BestBidPrice;
        //                NewBet(Symbol.BestBidPrice);
        //            }
        //        }
        //    });
        //}
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
                Error.WriteLog(path, Symbol.Name, $"Exception SubscribeAsync {ex.Message}");
            }
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
                        Symbol.Orders.Clear();
                    }
                    Symbol.BestAskPrice = Message.Data.BestAskPrice;
                    Symbol.BestBidPrice = Message.Data.BestBidPrice;
                    if(Message.Data.TransactionTime != null) Symbol.DateTime = (DateTime)Message.Data.TransactionTime;
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
                        decimal percentBid = Symbol.OrderBook.GetPriceBids(Symbol.Volume);

                        decimal minAsk = Symbol.OrderBook.MinAsk();
                        decimal percentAsk = Symbol.OrderBook.GetPriceAsks(Symbol.Volume);

                        if (percentBid > 0m)
                        {
                            Symbol.DistanceLower = (maxBid - percentBid) / percentBid * 100;
                        }
                        else
                        {
                            Symbol.DistanceLower = 0.01m;
                        }
                        if (percentAsk > 0m)
                        {
                            Symbol.DistanceUpper = (percentAsk - minAsk) / minAsk * 100;
                        }
                        else
                        {
                            Symbol.DistanceUpper = 0.01m;
                        }
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
