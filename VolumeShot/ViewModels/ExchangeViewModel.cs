using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using Binance.Net.Objects.Models.Futures.Socket;
using System;
using System.Linq;
using System.Threading.Tasks;
using VolumeShot.Models;

namespace VolumeShot.ViewModels
{
    internal class ExchangeViewModel
    {
        public Exchange Exchange { get; set; }
        public BinanceClient client { get; set; }
        public BinanceSocketClient socketClient { get; set; }
        public ExchangeViewModel(BinanceFuturesUsdtSymbol binanceFuturesUsdtSymbol, BinanceSocketClient _socketClient, BinanceClient _client)
        {
            socketClient = _socketClient;
            client = _client;
            Exchange = new(binanceFuturesUsdtSymbol);
        }
        public void OrderUpdate(BinanceFuturesStreamOrderUpdate OrderUpdate)
        {
            if (OrderUpdate.UpdateData.Symbol == Exchange.Symbol)
            {

            }
        }
        public void SetDistances(decimal distanceLower, decimal distanceUpper, decimal price)
        {
            decimal openQuantity = RoundQuantity(Exchange.Usdt / price);

            if (openQuantity * price < 10.5m)
            {
                openQuantity += Exchange.StepSize;
            }

            decimal Quantity = openQuantity;
            decimal LowerDistance = RoundPriceDecimal(distanceLower);
            decimal UpperDistance = RoundPriceDecimal(distanceUpper);
        }
        private decimal RoundPriceDecimal(decimal price)
        {
            return Math.Round(price, Exchange.RoundPrice);
        }
        private decimal RoundQuantity(decimal quantity)
        {
            decimal quantity_final = 0m;
            if (Exchange.StepSize == 0.001m) quantity_final = Math.Round(quantity, 3);
            else if (Exchange.StepSize == 0.01m) quantity_final = Math.Round(quantity, 2);
            else if (Exchange.StepSize == 0.1m) quantity_final = Math.Round(quantity, 1);
            else if (Exchange.StepSize == 1m) quantity_final = Math.Round(quantity, 0);
            if (quantity_final < Exchange.MinQuantity) return Exchange.MinQuantity;
            return quantity_final;
        }
        private async void ClearOrdersToSymbolAsync()
        {
            await CancelAllOrdersAsync();
            GetPositionInformationAsync();
        }
        private async Task CancelAllOrdersAsync()
        {
            await Task.Run(async() =>
            {
                var result = await client.UsdFuturesApi.Trading.CancelAllOrdersAsync(symbol: Exchange.Symbol);
                if (!result.Success)
                {
                    Error.WriteLog("exchange", Exchange.Symbol, $"Failed CancelAllOrdersAsync: {result.Error?.Message}");
                }
                else
                {
                    Error.WriteLog("exchange", Exchange.Symbol, $"CancelAllOrdersAsync");
                }
            });
        }
        private async void GetPositionInformationAsync()
        {
            var result = await client.UsdFuturesApi.Account.GetPositionInformationAsync(symbol: Exchange.Symbol);
            if (!result.Success)
            {
                Error.WriteLog("exchange", Exchange.Symbol, $"Failed GetPositionInformationAsync: {result.Error?.Message}");
            }
            else
            {
                Error.WriteLog("exchange", Exchange.Symbol, $"GetPositionInformationAsync:");
                decimal quantity = result.Data.ToList()[0].Quantity;
                if (quantity != 0m)
                {
                    if (quantity > 0m)
                    {
                        OpenOrderMarketAsync(OrderSide.Sell, quantity);
                    }
                    else
                    {
                        OpenOrderMarketAsync(OrderSide.Buy, -quantity);
                    }
                }
            }
        }
        private async void OpenOrderMarketAsync(OrderSide side, decimal quantity)
        {
            try
            {
                await Task.Run(async () =>
                {
                    Error.WriteLog("exchange", Exchange.Symbol, $"Open market order");
                    var result = await client.UsdFuturesApi.Trading.PlaceOrderAsync(
                        symbol: Exchange.Symbol,
                        side: side,
                        type: FuturesOrderType.Market,
                        quantity: quantity,
                        positionSide: PositionSide.Both);
                    if (!result.Success)
                    {
                        Error.WriteLog("exchange", Exchange.Symbol, $"Failed OpenOrderMarketAsync: {result.Error?.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Error.WriteLog("exchange", Exchange.Symbol, $"Exception OpenOrderMarketAsync: {ex.Message}");
            }
        }
        private async Task<long> OpenOrderTakeProfitAsync(OrderSide side, decimal price, decimal quantity)
        {
            Error.WriteLog("exchange", Exchange.Symbol, $"Open take profit order");
            var result = await client.UsdFuturesApi.Trading.PlaceOrderAsync(
                    symbol: Exchange.Symbol,
                    side: side,
                    type: FuturesOrderType.Limit,
                    price: price,
                    quantity: quantity,
                    positionSide: PositionSide.Both,
                    timeInForce: TimeInForce.GoodTillCanceled);
            if (!result.Success)
            {
                Error.WriteLog("exchange", Exchange.Symbol, $"Failed OpenOrderTakeProfitAsync: {result.Error?.Message}");
                return 0;
            }
            else
            {
                return result.Data.Id;
            }
        }
        private async Task<long> OpenOrderLimitAsync(OrderSide side, decimal price, decimal quantity)
        {
            var result = await client.UsdFuturesApi.Trading.PlaceOrderAsync(
                    symbol: Exchange.Symbol,
                    side: side,
                    type: FuturesOrderType.Limit,
                    price: price,
                    quantity: quantity,
                    positionSide: PositionSide.Both,
                    timeInForce: TimeInForce.GoodTillCanceled);
            if (!result.Success)
            {
                Error.WriteLog("exchange", Exchange.Symbol, $"Failed OpenOrderLimitAsync: {result.Error?.Message}");
                return 0;
            }
            else
            {
                return result.Data.Id;
            }
        }
        private async Task<decimal> OrderInfoAsync(long orderId)
        {
            var result = await client.UsdFuturesApi.Trading.GetOrderAsync(Exchange.Symbol, orderId);
            if (!result.Success)
            {
                Error.WriteLog("exchange", Exchange.Symbol, $"Failed OrderInfoAsync: {result.Error?.Message}");
                return 0m;
            }
            else
            {
                return result.Data.Quantity;
            }
        }
        private async Task<bool> CancelLimitOrderAsync(long orderId)
        {
            var result = await client.UsdFuturesApi.Trading.CancelOrderAsync(symbol: Exchange.Symbol, orderId: orderId);
            if (!result.Success)
            {
                Error.WriteLog("exchange", Exchange.Symbol, $"Failed CancelLimitOrderAsync: {result.Error?.Message}");
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
