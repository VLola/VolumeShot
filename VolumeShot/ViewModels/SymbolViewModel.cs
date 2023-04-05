using Binance.Net.Clients;
using Binance.Net.Objects.Models.Futures;
using System;
using System.Linq;
using System.Threading.Tasks;
using VolumeShot.Models;

namespace VolumeShot.ViewModels
{
    internal class SymbolViewModel
    {
        public Symbol Symbol { get; set; } = new();
        public SymbolViewModel(BinanceFuturesUsdtSymbol binanceFuturesUsdtSymbol, BinanceSocketClient socketClient) {
            Symbol.Name = binanceFuturesUsdtSymbol.Name;
            Symbol.PropertyChanged += Symbol_PropertyChanged;
            SubscribeAsync(socketClient);
        }

        private void Symbol_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
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
        private void NewBet(decimal openPrice)
        {
            Symbol.Bet = new();
            Symbol.Bet.OpenTime = DateTime.UtcNow;
            Symbol.Bet.OpenPrice = openPrice;
            Symbol.Bet.PriceBufferLower = Symbol.BestBidPriceLast - (Symbol.BestBidPriceLast / 100 * Symbol.BufferLower);
            Symbol.Bet.PriceDistanceLower = Symbol.BestBidPriceLast - (Symbol.BestBidPriceLast / 100 * Symbol.DistanceLower);
            Symbol.Bet.PriceBufferUpper = Symbol.BestAskPriceLast + (Symbol.BestAskPriceLast / 100 * Symbol.BufferUpper);
            Symbol.Bet.PriceDistanceUpper = Symbol.BestAskPriceLast + (Symbol.BestAskPriceLast / 100 * Symbol.DistanceUpper);
            if (Symbol.IsOpenLongOrder)
            {
                Symbol.Bet.PriceStopLoss = Symbol.OpenLongOrderPrice - (Symbol.OpenLongOrderPrice / 100 * Symbol.StopLoss);
                Symbol.Bet.PriceTakeProfit = Symbol.OpenLongOrderPrice + (Symbol.OpenLongOrderPrice / 100 * Symbol.TakeProfit);
            }
            if (Symbol.IsOpenShortOrder)
            {
                Symbol.Bet.PriceStopLoss = Symbol.OpenShortOrderPrice + (Symbol.OpenShortOrderPrice / 100 * Symbol.StopLoss);
                Symbol.Bet.PriceTakeProfit = Symbol.OpenShortOrderPrice - (Symbol.OpenShortOrderPrice / 100 * Symbol.TakeProfit);
            }
        }
        private void CloseBet(decimal closePrice)
        {
            Symbol.Bet.CloseTime = DateTime.UtcNow;
            Symbol.Bet.ClosePrice = closePrice;
            Order[] orders = Symbol.Orders.ToArray();
            Symbol.Bet.Orders = orders.Where(order => order.DateTime >= Symbol.Bet.OpenTime.AddSeconds(-20));
            Symbol.Orders.Clear();
            Symbol.Bets.Add(Symbol.Bet);
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
        private async void SubscribeAsync(BinanceSocketClient socketClient)
        {
            try
            {
                var result = await socketClient.UsdFuturesStreams.SubscribeToBookTickerUpdatesAsync(Symbol.Name, Message =>
                {
                    Symbol.BestAskPrice = Message.Data.BestAskPrice;
                    Symbol.BestBidPrice = Message.Data.BestBidPrice;
                    Symbol.Orders.Add(new Order() { BestAskPrice = Message.Data.BestAskPrice , BestBidPrice = Message.Data.BestBidPrice , DateTime = DateTime.UtcNow });
                });
                if (!result.Success)
                {
                    Error.WriteLog("binance", Symbol.Name, $"Failed SubscribeAsync: {result.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                Error.WriteLog("binance", Symbol.Name, $"Exception SubscribeAsync {ex.Message}");
            }
        }
    }
}
