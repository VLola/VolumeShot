using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using System;
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
