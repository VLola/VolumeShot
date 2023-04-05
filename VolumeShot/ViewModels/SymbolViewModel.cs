using Binance.Net.Clients;
using Binance.Net.Objects.Models.Futures;
using System;
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
                        Symbol.ShortPlus += 1;
                        Symbol.IsOpenShortOrder = false;
                    }
                }
                if (Symbol.IsOpenLongOrder)
                {
                    // Stop Loss Long
                    decimal price = Symbol.OpenLongOrderPrice + (Symbol.OpenLongOrderPrice / 100 * Symbol.StopLoss);
                    if (Symbol.BestAskPrice <= price)
                    {
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

        private async void CheckBuffersAsync()
        {
            await Task.Run(async () =>
            {
                await CheckBufferUpperAsync();
                await CheckBufferLowerAsync();
            });
        }
        private async Task CheckBufferLowerAsync()
        {
            await Task.Run(async () =>
            {
                if (Symbol.BestBidPriceLast > 0m && !Symbol.IsOpenShortOrder && !Symbol.IsOpenLongOrder)
                {
                    decimal price = Symbol.BestBidPriceLast + (Symbol.BestBidPriceLast / 100 * Symbol.BufferLower);
                    if (price >= Symbol.BestAskPrice)
                    {
                        await CheckDistanceLowerAsync();
                    }
                }
            });
        }
        private async Task CheckDistanceLowerAsync()
        {
            await Task.Run(async () =>
            {
                await Task.Delay(1500);
                decimal price = Symbol.BestBidPriceLast + (Symbol.BestBidPriceLast / 100 * Symbol.DistanceLower);
                if (price >= Symbol.BestAskPrice)
                {
                    if (!Symbol.IsOpenShortOrder && !Symbol.IsOpenLongOrder)
                    {
                        Symbol.IsOpenLongOrder = true;
                        Symbol.OpenLongOrderPrice = Symbol.BestAskPrice;
                    }
                }
                else
                {
                    if (!Symbol.IsOpenShortOrder && !Symbol.IsOpenLongOrder)
                    {
                        Symbol.BestAskPriceLast = Symbol.BestAskPrice;
                        Symbol.BestBidPriceLast = Symbol.BestBidPrice;
                    }
                }
            });
        }
        private async Task CheckBufferUpperAsync()
        {
            await Task.Run(async () =>
            {
                if (Symbol.BestAskPriceLast > 0m && !Symbol.IsOpenShortOrder && !Symbol.IsOpenLongOrder)
                {
                    decimal price = Symbol.BestAskPriceLast + (Symbol.BestAskPriceLast / 100 * Symbol.BufferUpper);
                    if (price <= Symbol.BestBidPrice)
                    {
                        await CheckDistanceUpperAsync();
                    }
                }
            });
        }
        private async Task CheckDistanceUpperAsync()
        {
            await Task.Run(async () =>
            {
                await Task.Delay(1000);
                decimal price = Symbol.BestAskPriceLast + (Symbol.BestAskPriceLast / 100 * Symbol.DistanceUpper);
                if (price <= Symbol.BestBidPrice)
                {
                    if (!Symbol.IsOpenShortOrder && !Symbol.IsOpenLongOrder)
                    {
                        Symbol.IsOpenShortOrder = true;
                        Symbol.OpenShortOrderPrice = Symbol.BestBidPrice;
                    }
                }
                else
                {
                    if (!Symbol.IsOpenShortOrder && !Symbol.IsOpenLongOrder)
                    {
                        Symbol.BestAskPriceLast = Symbol.BestAskPrice;
                        Symbol.BestBidPriceLast = Symbol.BestBidPrice;
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
