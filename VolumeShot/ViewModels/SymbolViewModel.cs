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
            RunAsync();
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
        }

        private async void RunAsync()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1500);
                    await CheckShotAsync();
                }
            });
        }
        private async Task CheckShotAsync()
        {
            await Task.Run(() => {
                if (Symbol.BestAskPriceLast > 0m)
                {
                    decimal price = Symbol.BestAskPriceLast + (Symbol.BestAskPriceLast / 100 * Symbol.DistanceLong);
                    if (price <= Symbol.BestBidPrice)
                    {
                        if (!Symbol.IsOpenLongOrder)
                        {
                            Symbol.IsOpenLongOrder = true;
                            Symbol.OpenLongOrderPrice = Symbol.BestAskPrice;
                        }
                    }
                }
                if(Symbol.BestBidPriceLast > 0m)
                {
                    decimal price = Symbol.BestBidPriceLast - (Symbol.BestBidPriceLast / 100 * Symbol.DistanceShort);
                    if (price >= Symbol.BestAskPrice)
                    {
                        if (!Symbol.IsOpenShortOrder)
                        {
                            Symbol.IsOpenShortOrder = true;
                            Symbol.OpenShortOrderPrice = Symbol.BestBidPrice;
                        }
                    }
                }
                Symbol.BestAskPriceLast = Symbol.BestAskPrice;
                Symbol.BestBidPriceLast = Symbol.BestBidPrice;
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
