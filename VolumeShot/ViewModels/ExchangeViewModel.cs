using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using Binance.Net.Objects.Models.Futures.Socket;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VolumeShot.Models;

namespace VolumeShot.ViewModels
{
    internal class ExchangeViewModel
    {
        private string path = $"{Directory.GetCurrentDirectory()}/log/exchange/";
        public Exchange Exchange { get; set; }
        public BinanceClient client { get; set; }
        public BinanceSocketClient socketClient { get; set; }
        public ExchangeViewModel(BinanceFuturesUsdtSymbol binanceFuturesUsdtSymbol, BinanceSocketClient _socketClient, BinanceClient _client)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            socketClient = _socketClient;
            client = _client;
            Exchange = new(binanceFuturesUsdtSymbol);
            Exchange.PropertyChanged += Exchange_PropertyChanged;
        }

        private void Exchange_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsOpenShortOrder")
            {

            }
            else if (e.PropertyName == "IsOpenLongOrder")
            {

            }
        }

        public void OrderUpdate(BinanceFuturesStreamOrderUpdate OrderUpdate)
        {
            if (OrderUpdate.UpdateData.Symbol == Exchange.Symbol)
            {
                if (OrderUpdate.UpdateData.Status == OrderStatus.Filled)
                {
                    if (OrderUpdate.UpdateData.Type == FuturesOrderType.Limit)
                    {
                        if(!Exchange.IsOpenLongOrder && !Exchange.IsOpenShortOrder)
                        {
                            if (OrderUpdate.UpdateData.Side == OrderSide.Buy && OrderUpdate.UpdateData.PositionSide == PositionSide.Long)
                            {
                                OpenBet(OrderUpdate.UpdateData.PositionSide, OrderUpdate.UpdateData.UpdateTime, OrderUpdate.UpdateData.AveragePrice);
                                Exchange.IsOpenLongOrder = true;
                                OpenOrder(OrderUpdate.UpdateData.OrderId, OrderUpdate.UpdateData.AveragePrice, OrderUpdate.UpdateData.Side, OrderUpdate.UpdateData.Quantity, OrderUpdate.UpdateData.UpdateTime);
                            }
                            else if (OrderUpdate.UpdateData.Side == OrderSide.Sell && OrderUpdate.UpdateData.PositionSide == PositionSide.Short)
                            {
                                OpenBet(OrderUpdate.UpdateData.PositionSide, OrderUpdate.UpdateData.UpdateTime, OrderUpdate.UpdateData.AveragePrice);
                                Exchange.IsOpenShortOrder = true;
                                OpenOrder(OrderUpdate.UpdateData.OrderId, OrderUpdate.UpdateData.AveragePrice, OrderUpdate.UpdateData.Side, OrderUpdate.UpdateData.Quantity, OrderUpdate.UpdateData.UpdateTime);
                            }
                        }
                        else if(Exchange.IsOpenLongOrder || Exchange.IsOpenShortOrder)
                        {
                            if (OrderUpdate.UpdateData.Side == OrderSide.Sell && OrderUpdate.UpdateData.PositionSide == PositionSide.Long || OrderUpdate.UpdateData.Side == OrderSide.Buy && OrderUpdate.UpdateData.PositionSide == PositionSide.Short)
                            {
                                CloseBet(OrderUpdate.UpdateData.UpdateTime, OrderUpdate.UpdateData.AveragePrice);
                                ClearOrdersToSymbolAsync();
                            }
                        }
                    }
                    else if (OrderUpdate.UpdateData.Type == FuturesOrderType.Market)
                    {
                        if (Exchange.IsOpenLongOrder || Exchange.IsOpenShortOrder)
                        {
                            if (OrderUpdate.UpdateData.Side == OrderSide.Sell && OrderUpdate.UpdateData.PositionSide == PositionSide.Long || OrderUpdate.UpdateData.Side == OrderSide.Buy && OrderUpdate.UpdateData.PositionSide == PositionSide.Short)
                            {
                                CloseBet(OrderUpdate.UpdateData.UpdateTime, OrderUpdate.UpdateData.AveragePrice);
                                ClearOrdersToSymbolAsync();
                            }
                        }
                    }
                }
                // Write log
                string json = JsonConvert.SerializeObject(OrderUpdate.UpdateData);
                Error.WriteLog(path, Exchange.Symbol, json);
            }
        }
        private void OpenBet(PositionSide positionSide, DateTime openTime, decimal openPrice)
        {
            Exchange.OpenBetOrders.Clear();
            Bet bet = new Bet();
            bet.Orders = Exchange.Orders.ToList();
            bet.OpenTime = openTime;
            bet.OpenPrice = openPrice;
            if(positionSide == PositionSide.Long)
            {
                bet.PriceTakeProfit = Exchange.TakeProfitLongPrice;
                bet.TakeProfit = Exchange.TakeProfitLong;
                bet.PriceStopLoss = Exchange.StopLossLongPrice;
                bet.StopLoss = Exchange.StopLossLong;
            }
            else if (positionSide == PositionSide.Short)
            {
                bet.PriceTakeProfit = Exchange.TakeProfitShortPrice;
                bet.TakeProfit = Exchange.TakeProfitShort;
                bet.PriceStopLoss = Exchange.StopLossShortPrice;
                bet.StopLoss = Exchange.StopLossShort;
            }
            bet.BufferLower = Exchange.BufferLower;
            bet.BufferUpper = Exchange.BufferUpper;
            bet.PriceBufferLower = Exchange.BufferLowerPrice;
            bet.PriceBufferUpper = Exchange.BufferUpperPrice;
            bet.DistanceLower = Exchange.DistanceLower;
            bet.DistanceUpper = Exchange.DistanceUpper;
            bet.PriceDistanceLower = Exchange.DistanceLowerPrice; 
            bet.PriceDistanceUpper = Exchange.DistanceUpperPrice;
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                Exchange.Bets.Insert(0, bet);
            }));
        }
        private void CloseBet(DateTime closeTime, decimal closePrice)
        {
            Exchange.Bets[0].Orders.AddRange(Exchange.OpenBetOrders);
            Exchange.Bets[0].CloseTime = closeTime;
            Exchange.Bets[0].ClosePrice = closePrice;
        }
        private async void OpenOrder(long orderId, decimal price, OrderSide side, decimal quantity, DateTime time)
        {
            if (side == OrderSide.Buy)
            {
                await CancelAllOrdersAsync();
                OpenOrderTakeProfitAsync(PositionSide.Long, OrderSide.Sell, Exchange.TakeProfitLongPrice, quantity);
            }
            else
            {
                await CancelAllOrdersAsync();
                OpenOrderTakeProfitAsync(PositionSide.Short, OrderSide.Buy, Exchange.TakeProfitShortPrice, quantity);
            }
        }
        public async Task SetDistances(decimal distanceUpper, decimal distanceLower, decimal askPrice, decimal bidPrice, decimal bufferUpper, decimal bufferLower, decimal bufferUpperPrice, decimal bufferLowerPrice)
        {
            decimal openQuantity = RoundQuantity(Exchange.Usdt / askPrice);

            if (openQuantity * askPrice < 10.5m)
            {
                openQuantity += Exchange.StepSize;
            }

            decimal priceDistanceLower = RoundPriceDecimal(bidPrice - (bidPrice / 100 * distanceLower));
            Exchange.DistanceLowerPrice = priceDistanceLower;
            Exchange.DistanceLower = distanceLower;
            Exchange.TakeProfitLong = distanceLower / Exchange.DenominatorTakeProfit;
            Exchange.StopLossLong = distanceLower / Exchange.DenominatorStopLoss;
            Exchange.TakeProfitLongPrice = RoundPriceDecimal(priceDistanceLower + (priceDistanceLower / 100 * distanceLower / Exchange.DenominatorTakeProfit));
            Exchange.StopLossLongPrice = RoundPriceDecimal(priceDistanceLower - (priceDistanceLower / 100 * distanceLower / Exchange.DenominatorStopLoss));

            decimal priceDistanceUpper = RoundPriceDecimal(askPrice + (askPrice / 100 * distanceUpper));
            Exchange.DistanceUpperPrice = priceDistanceUpper;
            Exchange.DistanceUpper = distanceUpper;
            Exchange.TakeProfitShort = distanceUpper / Exchange.DenominatorTakeProfit;
            Exchange.StopLossShort = distanceUpper / Exchange.DenominatorStopLoss;
            Exchange.TakeProfitShortPrice = RoundPriceDecimal(priceDistanceUpper - (priceDistanceUpper / 100 * distanceUpper / Exchange.DenominatorTakeProfit));
            Exchange.StopLossShortPrice = RoundPriceDecimal(priceDistanceUpper + (priceDistanceUpper / 100 * distanceUpper / Exchange.DenominatorStopLoss));
            // Buffers
            Exchange.BufferLowerPrice = bufferLowerPrice;
            Exchange.BufferUpperPrice = bufferUpperPrice;
            Exchange.BufferLower = bufferLower;
            Exchange.BufferUpper = bufferUpper;

            await CancelAllOrdersAsync();
            await OpenOrderLimitAsync(PositionSide.Long, OrderSide.Buy, priceDistanceLower, openQuantity);
            await OpenOrderLimitAsync(PositionSide.Short, OrderSide.Sell, priceDistanceUpper, openQuantity);
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
        public async void ClearOrdersToSymbolAsync()
        {
            await CancelAllOrdersAsync();
            GetPositionInformationAsync();
            Exchange.IsOpenLongOrder = false;
            Exchange.IsOpenShortOrder = false;
        }
        private async Task CancelAllOrdersAsync()
        {
            await Task.Run(async() =>
            {
                try
                {
                    var result = await client.UsdFuturesApi.Trading.CancelAllOrdersAsync(symbol: Exchange.Symbol);
                    if (!result.Success)
                    {
                        Error.WriteLog(path, Exchange.Symbol, $"Failed CancelAllOrdersAsync: {result.Error?.Message}");
                    }
                    else
                    {
                        Error.WriteLog(path, Exchange.Symbol, $"CancelAllOrdersAsync");
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Exchange.Symbol, $"Exception CancelAllOrdersAsync: {ex.Message}");
                }
            });
        }
        private async void GetPositionInformationAsync()
        {
            await Task.Run(async() =>
            {
                try
                {
                    var result = await client.UsdFuturesApi.Account.GetPositionInformationAsync(symbol: Exchange.Symbol);
                    if (!result.Success)
                    {
                        Error.WriteLog(path, Exchange.Symbol, $"Failed GetPositionInformationAsync: {result.Error?.Message}");
                    }
                    else
                    {
                        foreach (var item in result.Data.ToList())
                        {
                            if (item.Quantity != 0m)
                            {
                                if (item.Quantity > 0m)
                                {
                                    OpenOrderMarketAsync(item.PositionSide, OrderSide.Sell, item.Quantity);
                                }
                                else
                                {
                                    OpenOrderMarketAsync(item.PositionSide, OrderSide.Buy, -item.Quantity);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Exchange.Symbol, $"Exception GetPositionInformationAsync: {ex.Message}");
                }
            });
        }
        private async void OpenOrderMarketAsync(PositionSide positionSide, OrderSide side, decimal quantity)
        {
            await Task.Run(async () =>
            {
                try
                {
                    var result = await client.UsdFuturesApi.Trading.PlaceOrderAsync(
                        symbol: Exchange.Symbol,
                        side: side,
                        type: FuturesOrderType.Market,
                        quantity: quantity,
                        positionSide: positionSide);
                    if (!result.Success)
                    {
                        Error.WriteLog(path, Exchange.Symbol, $"Failed OpenOrderMarketAsync: {result.Error?.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Exchange.Symbol, $"Exception OpenOrderMarketAsync: {ex.Message}");
                }
            });
        }
        private async Task OpenOrderTakeProfitAsync(PositionSide positionSide, OrderSide side, decimal price, decimal quantity)
        {
            await Task.Run(async () =>
            {
                try
                {
                    var result = await client.UsdFuturesApi.Trading.PlaceOrderAsync(
                            symbol: Exchange.Symbol,
                            side: side,
                            type: FuturesOrderType.Limit,
                            price: price,
                            quantity: quantity,
                            positionSide: positionSide,
                            timeInForce: TimeInForce.GoodTillCanceled);
                    if (!result.Success)
                    {
                        Error.WriteLog(path, Exchange.Symbol, $"Failed OpenOrderTakeProfitAsync: {result.Error?.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Exchange.Symbol, $"Exception OpenOrderTakeProfitAsync: {ex.Message}");
                }
            });
        }
        private async Task OpenOrderLimitAsync(PositionSide positionSide, OrderSide side, decimal price, decimal quantity)
        {
            await Task.Run(async () =>
            {
                try
                {
                    var result = await client.UsdFuturesApi.Trading.PlaceOrderAsync(
                            symbol: Exchange.Symbol,
                            side: side,
                            type: FuturesOrderType.Limit,
                            price: price,
                            quantity: quantity,
                            positionSide: positionSide,
                            timeInForce: TimeInForce.GoodTillCanceled);
                    if (!result.Success)
                    {
                        Error.WriteLog(path, Exchange.Symbol, $"Failed OpenOrderLimitAsync: {result.Error?.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Exchange.Symbol, $"Exception OpenOrderLimitAsync: {ex.Message}");
                }
            });
        }
        private async Task<decimal> OrderInfoAsync(long orderId)
        {
            var result = await client.UsdFuturesApi.Trading.GetOrderAsync(Exchange.Symbol, orderId);
            if (!result.Success)
            {
                Error.WriteLog(path, Exchange.Symbol, $"Failed OrderInfoAsync: {result.Error?.Message}");
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
                Error.WriteLog(path, Exchange.Symbol, $"Failed CancelLimitOrderAsync: {result.Error?.Message}");
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
