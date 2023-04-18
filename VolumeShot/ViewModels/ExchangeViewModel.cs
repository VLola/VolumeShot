using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using Binance.Net.Objects.Models.Futures.Socket;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VolumeShot.Models;

namespace VolumeShot.ViewModels
{
    internal class ExchangeViewModel
    {
        private string path = $"{Directory.GetCurrentDirectory()}/log/exchange/";
        private string pathHistory = $"{Directory.GetCurrentDirectory()}/history/";
        public Exchange Exchange { get; set; }
        public BinanceClient client { get; set; }
        public BinanceSocketClient socketClient { get; set; }
        public ExchangeViewModel(BinanceFuturesUsdtSymbol binanceFuturesUsdtSymbol, BinanceSocketClient _socketClient, BinanceClient _client, string loginUser)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (!Directory.Exists(pathHistory)) Directory.CreateDirectory(pathHistory);
            socketClient = _socketClient;
            client = _client;
            Exchange = new(binanceFuturesUsdtSymbol);
            Exchange.LoginUser = loginUser;
            LoadHistoryAsync();
        }
        private async void LoadHistoryAsync()
        {
            await Task.Run(()=>{
                try
                {
                    if (File.Exists(pathHistory + Exchange.LoginUser + "/" + Exchange.Symbol))
                    {
                        string json = File.ReadAllText(pathHistory + Exchange.LoginUser + "/" + Exchange.Symbol);
                        ObservableCollection<Bet>? bets = JsonConvert.DeserializeObject<ObservableCollection<Bet>>(json);
                        if (bets != null && bets.Count > 0)
                        {
                            foreach (var item in bets)
                            {
                                Exchange.Bets.Add(item);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Exchange.Symbol, $"Exception LoadHistoryAsync: {ex.Message}");
                }
            });
        }
        public void AccountUpdate(BinanceFuturesStreamAccountUpdate AccountUpdate, string[] Symbols)
        {
            if (Symbols.Contains(Exchange.Symbol)) {
                foreach (var item in AccountUpdate.UpdateData.Positions)
                {
                    if(item.Symbol == Exchange.Symbol)
                    {
                        if (item.Quantity == 0m)
                        {
                            Exchange.IsWait = true;
                            if (item.PositionSide == PositionSide.Long)
                            {
                                if (Exchange.IsOpenLongOrder) 
                                {
                                    CloseBet(AccountUpdate.EventTime); 
                                }
                                Exchange.IsOpenLongOrder = false;
                            }
                            else if (item.PositionSide == PositionSide.Short)
                            {
                                if (Exchange.IsOpenShortOrder)
                                {
                                    CloseBet(AccountUpdate.EventTime);
                                }
                                Exchange.IsOpenShortOrder = false;
                            }
                        }
                        else
                        {
                            if (item.PositionSide == PositionSide.Long)
                            {
                                if (!Exchange.IsOpenLongOrder)
                                {
                                    OpenBet(item.PositionSide, AccountUpdate.EventTime, item.EntryPrice);
                                }
                                Exchange.IsOpenLongOrder = true;
                            }
                            else if (item.PositionSide == PositionSide.Short) 
                            {
                                if (!Exchange.IsOpenShortOrder)
                                {
                                    OpenBet(item.PositionSide, AccountUpdate.EventTime, item.EntryPrice);
                                }
                                Exchange.IsOpenShortOrder = true;
                            }

                        }
                    }
                }
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
                        if (OrderUpdate.UpdateData.Side == OrderSide.Buy && OrderUpdate.UpdateData.PositionSide == PositionSide.Long || OrderUpdate.UpdateData.Side == OrderSide.Sell && OrderUpdate.UpdateData.PositionSide == PositionSide.Short)
                        {
                            OpenOrder(OrderUpdate.UpdateData.OrderId, OrderUpdate.UpdateData.AveragePrice, OrderUpdate.UpdateData.Side, OrderUpdate.UpdateData.Quantity, OrderUpdate.UpdateData.UpdateTime);
                        }
                        else if (OrderUpdate.UpdateData.Side == OrderSide.Sell && OrderUpdate.UpdateData.PositionSide == PositionSide.Long || OrderUpdate.UpdateData.Side == OrderSide.Buy && OrderUpdate.UpdateData.PositionSide == PositionSide.Short)
                        {
                            Exchange.Quantity += OrderUpdate.UpdateData.Quantity;
                            Exchange.ClosePrice = OrderUpdate.UpdateData.AveragePrice;
                            ClearOrdersToSymbolAsync();
                        }
                    }
                    else if (OrderUpdate.UpdateData.Type == FuturesOrderType.Market)
                    {
                        if (OrderUpdate.UpdateData.Side == OrderSide.Sell && OrderUpdate.UpdateData.PositionSide == PositionSide.Long || OrderUpdate.UpdateData.Side == OrderSide.Buy && OrderUpdate.UpdateData.PositionSide == PositionSide.Short)
                        {
                            Exchange.Quantity += OrderUpdate.UpdateData.Quantity;
                            Exchange.ClosePrice = OrderUpdate.UpdateData.AveragePrice;
                            ClearOrdersToSymbolAsync();
                        }
                    }
                }
                Exchange.Fee += OrderUpdate.UpdateData.Fee;
                Exchange.Profit += OrderUpdate.UpdateData.RealizedProfit;
            }
        }
        private void OpenBet(PositionSide positionSide, DateTime openTime, decimal openPrice)
        {
            try
            {
                Exchange.OpenBetSymbolPrices.Clear();
                Exchange.IsWriteSymbolPrices = true;
                Bet bet = new Bet();
                bet.Symbol = Exchange.Symbol;
                bet.SymbolPrices = Exchange.SymbolPrices.ToList();
                bet.OpenTime = openTime;
                bet.OpenPrice = openPrice;
                if (positionSide == PositionSide.Long)
                {
                    bet.PriceTakeProfit = Exchange.TakeProfitLongPrice;
                    bet.TakeProfit = Exchange.TakeProfitLong;
                    bet.PriceStopLoss = Exchange.StopLossLongPrice;
                    bet.StopLoss = Exchange.StopLossLong;
                    bet.Position = "Long";
                }
                else if (positionSide == PositionSide.Short)
                {
                    bet.PriceTakeProfit = Exchange.TakeProfitShortPrice;
                    bet.TakeProfit = Exchange.TakeProfitShort;
                    bet.PriceStopLoss = Exchange.StopLossShortPrice;
                    bet.StopLoss = Exchange.StopLossShort;
                    bet.Position = "Short";
                }
                bet.BufferLower = Exchange.BufferLower;
                bet.BufferUpper = Exchange.BufferUpper;
                bet.PriceBufferLower = Exchange.BufferLowerPrice;
                bet.PriceBufferUpper = Exchange.BufferUpperPrice;
                bet.DistanceLower = Exchange.DistanceLower;
                bet.DistanceUpper = Exchange.DistanceUpper;
                bet.PriceDistanceLower = Exchange.DistanceLowerPrice;
                bet.PriceDistanceUpper = Exchange.DistanceUpperPrice;
                bet.Volume = Exchange.Volume;
                App.Current.Dispatcher.BeginInvoke(new Action(() => {
                    Exchange.Bets.Insert(0, bet);
                }));
            }
            catch (Exception ex)
            {
                Error.WriteLog(path, Exchange.Symbol, $"Exception OpenBet: {ex.Message}");
            }
        }
        private async void CloseBet(DateTime closeTime)
        {
            await Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(5000);
                    Exchange.IsWriteSymbolPrices = false;
                    Bet bet = Exchange.Bets[0];
                    bet.SymbolPrices.AddRange(Exchange.OpenBetSymbolPrices);
                    bet.CloseTime = closeTime;
                    bet.ClosePrice = Exchange.ClosePrice;
                    bet.Quantity = Exchange.Quantity;
                    bet.Usdt = Exchange.Quantity * Exchange.ClosePrice;
                    bet.Fee = Exchange.Fee;
                    bet.Profit = Exchange.Profit;
                    bet.Total = Exchange.Profit - Exchange.Fee;
                    if ((Exchange.Profit - Exchange.Fee) < 0m) bet.IsPositive = false;
                    else bet.IsPositive = true;
                    Exchange.Bet = bet;
                    SaveHistoryAsync();
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Exchange.Symbol, $"Exception CloseBet: {ex.Message}");
                }
            });
        }
        private async void SaveHistoryAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    string json = JsonConvert.SerializeObject(Exchange.Bets);
                    File.WriteAllText(pathHistory + Exchange.LoginUser + "/" + Exchange.Symbol, json);
                }
                catch (Exception ex)
                {
                    Error.WriteLog(path, Exchange.Symbol, $"Exception SaveHistoryAsync: {ex.Message}");
                }
            });
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
        public async Task SetDistances(decimal distanceUpper, decimal distanceLower, decimal askPrice, decimal bidPrice, decimal bufferUpper, decimal bufferLower, decimal bufferUpperPrice, decimal bufferLowerPrice, decimal volume)
        {
            try
            {
                await CancelAllOrdersAsync();
                if (!Exchange.IsOpenLongOrder && !Exchange.IsOpenShortOrder)
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

                    Exchange.Volume = volume;
                    Exchange.Fee = 0m;
                    Exchange.Profit = 0m;
                    Exchange.Quantity = 0m;

                    await OpenOrderLimitAsync(PositionSide.Long, OrderSide.Buy, priceDistanceLower, openQuantity);
                    await OpenOrderLimitAsync(PositionSide.Short, OrderSide.Sell, priceDistanceUpper, openQuantity);
                }
            }
            catch (Exception ex)
            {
                Error.WriteLog(path, Exchange.Symbol, $"Exception SetDistances: {ex.Message}");
            }
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
        public async void ClosePositionsAsync()
        {
            StopLossAsync();
            await CancelAllOrdersAsync();
            GetPositionInformationAsync();
        } 
        private async void StopLossAsync()
        {
            await Task.Run(async () => {
                Exchange.IsWorkedStopLoss = true;
                await Task.Delay(1000);
                Exchange.IsWorkedStopLoss = false;
            });
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
    }
}
