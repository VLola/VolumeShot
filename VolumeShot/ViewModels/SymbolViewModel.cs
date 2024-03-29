﻿using Binance.Net;
using Binance.Net.Clients;
using Binance.Net.Objects.Models.Futures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VolumeShot.Command;
using VolumeShot.Models;

namespace VolumeShot.ViewModels
{
    internal class SymbolViewModel
    {
        private string pathConfigs = $"{Directory.GetCurrentDirectory()}/configs/";
        private string path = $"{Directory.GetCurrentDirectory()}/log/symbol/";
        public Symbol Symbol { get; set; } = new();
        public ExchangeViewModel ExchangeViewModel { get; set; }
        public BinanceClient client { get; set; }
        public BinanceSocketClient socketClient { get; set; }

        
        public SymbolViewModel(General general, BinanceFuturesUsdtSymbol binanceFuturesUsdtSymbol, decimal volume, BinanceSocketClient _socketClient, BinanceClient _client, bool isTestnet, string loginUser) {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            socketClient = _socketClient;
            client = _client;
            ExchangeViewModel = new ExchangeViewModel(general, binanceFuturesUsdtSymbol, _socketClient, _client, loginUser);
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
            if (e.PropertyName == "VolumeSave")
            {
                Symbol.Volume = Symbol.Exchange.VolumeSave;
            }
        }

        private async void WaitAsync()
        {
            await Task.Run(async () => {
                await Task.Delay(10000);
                Symbol.Exchange.IsWait = false;
                if (Symbol.IsRun) Symbol.IsRedistance = true;
            });
        }

        private async void Run()
        {
            await Task.Run(async () =>
            {
                while (Symbol.IsRun)
                {
                    await Task.Delay(1);
                    await CheckBufferAsync(); 
                }
            });
        }

        private void Symbol_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Volume")
            {
                //SaveVolumeAsync();
                if(Symbol.IsRun) Symbol.IsRedistance = true;
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
                    if (Symbol.IsRun) Symbol.IsRedistance = true;
                }
                else
                {
                    ExchangeViewModel.ClosePositionsAsync();
                }
            }
            else if (e.PropertyName == "IsSaveVolume")
            {
                SaveVolumeAsync();
            }
        }


        private async void SaveVolumeAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    List<Config>? configs = new();
                    string pathFile = pathConfigs + "config";
                    if (File.Exists(pathFile))
                    {
                        string json = File.ReadAllText(pathFile);
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
                            File.WriteAllText(pathFile, json1);
                        }
                    }
                    else
                    {
                        configs.Add(new Config() { Name = Symbol.Name, Volume = Symbol.Volume });
                        string json = JsonConvert.SerializeObject(configs);
                        File.WriteAllText(pathFile, json);
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Symbol.Name, $"Exception SaveVolumeAsync: {ex.Message}");
                }
            });
        }
        private void ReBuffers()
        {
            try
            {
                Symbol.BufferLowerPrice = Symbol.Price - (Symbol.Price / 100 * Symbol.BufferLower);
                Symbol.BufferUpperPrice = Symbol.Price + (Symbol.Price / 100 * Symbol.BufferUpper);
            }
            catch (Exception ex)
            {
                Error.WriteLog(path, Symbol.Name, $"Exception ReBuffers: {ex.Message}");
            }
        }
        private async Task ReDistanceAsync(Method method)
        {
            ReBuffers();
            if (Symbol.IsTrading && !Symbol.Exchange.IsWait && !Symbol.Exchange.IsCanceledOrders && !ExchangeViewModel.Exchange.IsOpenShortOrder && !ExchangeViewModel.Exchange.IsOpenLongOrder) await ExchangeViewModel.CancelAllOrdersAsync(method);
            await Task.Delay(500);
            ReBuffers();
            if (Symbol.IsTrading && !Symbol.Exchange.IsWait && !ExchangeViewModel.Exchange.IsOpenShortOrder && !ExchangeViewModel.Exchange.IsOpenLongOrder) await ExchangeViewModel.SetDistances(method, distanceUpper: Symbol.DistanceUpper, distanceLower: Symbol.DistanceLower, price: Symbol.Price, bufferUpper: Symbol.BufferUpper, bufferLower: Symbol.BufferLower, bufferUpperPrice: Symbol.BufferUpperPrice, bufferLowerPrice: Symbol.BufferLowerPrice, volume: Symbol.Volume);
        }
        private async Task CheckBufferAsync()
        {
            await Task.Run(async () =>
            {
                //if (Symbol.BufferLowerPrice == 0m || Symbol.BufferUpperPrice == 0m)
                //{
                //    await ReDistanceAsync(Method.CheckBufferAsync1);
                //}
                //else 
                
                if (!ExchangeViewModel.Exchange.IsOpenShortOrder && !ExchangeViewModel.Exchange.IsOpenLongOrder)
                {
                    if (Symbol.IsRedistance)
                    {
                        Symbol.IsRedistance = false;
                        await ReDistanceAsync(Method.ReDistanceAsync1);
                    }
                    else if (Symbol.BufferLowerPrice >= Symbol.Price || Symbol.BufferUpperPrice <= Symbol.Price)
                    {
                        await ReDistanceAsync(Method.ReDistanceAsync2);
                    }
                }
            });
        }
        private async void SubscribeAsync()
        {
            try
            {
                await SubscribeToAggregatedTradeUpdatesAsync();
                await GetOrderBookAsync();
                await SubscribeToOrderBookUpdatesAsync();
            }
            catch (Exception ex)
            {
                Symbol.IsRun = false;
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
                    try
                    {
                        // Stop loss
                        if (!ExchangeViewModel.Exchange.IsWorkedStopLoss && ExchangeViewModel.Exchange.IsOpenShortOrder && Message.Data.Price >= ExchangeViewModel.Exchange.StopLossShortPrice || !ExchangeViewModel.Exchange.IsWorkedStopLoss && ExchangeViewModel.Exchange.IsOpenLongOrder && Message.Data.Price <= ExchangeViewModel.Exchange.StopLossLongPrice)
                        {
                            ExchangeViewModel.ClosePositionsAsync();
                        }
                        if (!Symbol.IsRun)
                        {
                            socketClient.UnsubscribeAsync(socketId);
                            Symbol.Exchange.SymbolPrices.Clear();
                        }
                        Symbol.BuyerIsMaker = Message.Data.BuyerIsMaker;
                        Symbol.TradeTime = Message.Data.TradeTime;
                        Symbol.Price = Message.Data.Price;
                        SymbolPrice symbolPrice = new SymbolPrice(Message.Data.Price, Message.Data.BuyerIsMaker, Message.Data.TradeTime, Symbol.Exchange.SymbolPrices, (Message.Data.Quantity * Message.Data.Price));
                        Symbol.Exchange.SymbolPrices.Add(symbolPrice);
                        if (Symbol.Exchange.IsWriteSymbolPrices)
                        {
                            Symbol.Exchange.OpenBetSymbolPrices.Add(symbolPrice);
                        }
                    }
                    catch (Exception ex)
                    {
                        Error.WriteLog(path, Symbol.Name, $"Exception SubscribeToAggregatedTradeUpdatesAsync: {ex.Message}");
                    }
                });
                if (!result.Success)
                {
                    Symbol.IsRun = false;
                    Error.WriteLog(path, Symbol.Name, $"Failed SubscribeToAggregatedTradeUpdatesAsync: {result.Error?.Message}");
                }
                else socketId = result.Data.Id;
            });
        }
        private async Task GetOrderBookAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    var result = await client.UsdFuturesApi.ExchangeData.GetOrderBookAsync(Symbol.Name, limit: 1000);
                    if (!result.Success)
                    {
                        Symbol.IsRun = false;
                        Error.WriteLog(path, Symbol.Name, $"Failed GetOrderBookAsync: {result.Error?.Message}");
                    }
                    else
                    {
                        Symbol.OrderBook.Bids.Clear();
                        Symbol.OrderBook.Asks.Clear();
                        Symbol.OrderBook.AddBids(result.Data.Bids);
                        Symbol.OrderBook.AddAsks(result.Data.Asks);

                        var weight = BinanceHelpers.UsedWeight(result.ResponseHeaders);
                        ExchangeViewModel.Exchange.General.Requests = weight;
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Symbol.Name, $"Exception GetOrderBookAsync: {ex.Message}");
                }
            });
        }
        private async Task SubscribeToOrderBookUpdatesAsync()
        {
            await Task.Run(async () =>
            {
                int socketId = 0;
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
                        else
                        {
                            // Bids
                            Symbol.OrderBook.AddBids(Message.Data.Bids);

                            // Asks
                            Symbol.OrderBook.AddAsks(Message.Data.Asks);


                            decimal maxBid = Symbol.OrderBook.MaxBid();
                            decimal priceBid = Symbol.OrderBook.GetPriceBids(Symbol.Volume);

                            decimal minAsk = Symbol.OrderBook.MinAsk();
                            decimal priceAsk = Symbol.OrderBook.GetPriceAsks(Symbol.Volume);

                            // Lower
                            if (priceBid != 0m)
                            {
                                decimal percentBid = (maxBid - priceBid) / priceBid * 100;
                                if (!Symbol.IsTestnet)
                                {
                                    if (percentBid >= 0.5m) Symbol.DistanceLower = percentBid;
                                    else Symbol.DistanceLower = 0.5m;
                                }
                                else
                                {
                                    if (percentBid > 0.001m) Symbol.DistanceLower = percentBid;
                                    else Symbol.DistanceLower = 0.001m;
                                }
                            }

                            // Upper
                            if (minAsk != 0m)
                            {
                                decimal percentAsk = (priceAsk - minAsk) / minAsk * 100;
                                if (!Symbol.IsTestnet)
                                {
                                    if (percentAsk >= 0.5m) Symbol.DistanceUpper = percentAsk;
                                    else Symbol.DistanceUpper = 0.5m;
                                }
                                else
                                {
                                    if (percentAsk > 0.001m) Symbol.DistanceUpper = percentAsk;
                                    else Symbol.DistanceUpper = 0.001m;
                                }
                            }

                            Symbol.BufferUpper = Symbol.DistanceUpper / 5;
                            Symbol.BufferLower = Symbol.DistanceLower / 5;
                        }
                    }
                    catch (Exception ex)
                    {
                        Error.WriteLog(path, Symbol.Name, $"Exception SubscribeToOrderBookUpdatesAsync: {ex.Message}");
                    }

                });
                if (!result.Success)
                {
                    Symbol.IsRun = false;
                    Error.WriteLog(path, Symbol.Name, $"Failed SubscribeToOrderBookUpdatesAsync: {result.Error?.Message}");
                }
                else socketId = result.Data.Id;
            });
        }
    }
}
