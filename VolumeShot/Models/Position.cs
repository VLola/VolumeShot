using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using Binance.Net.Objects.Models.Futures.Socket;
using CryptoExchange.Net.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Shapes;
using VolumeShot.Command;

namespace VolumeShot.Models
{
    public class Position : Changed
    {
        string pathPositions = Directory.GetCurrentDirectory() + "/log/positions/"; 
        private BinanceClient client { get; set; }
        private BinanceSocketClient socketClient { get; set; }
        public string Symbol { get; set; }
        public PositionSide PositionSide { get; set; }
        public DateTime OpenTime { get; set; }
        private bool _isPositive { get; set; }
        public bool IsPositive
        {
            get { return _isPositive; }
            set
            {
                _isPositive = value;
                OnPropertyChanged("IsPositive");
            }
        }
        private decimal _pnlPercent { get; set; }
        public decimal PnlPercent
        {
            get { return _pnlPercent; }
            set
            {
                _pnlPercent = value;
                OnPropertyChanged("PnlPercent");
            }
        }
        private decimal _pnl { get; set; }
        public decimal Pnl
        {
            get { return _pnl; }
            set
            {
                _pnl = value;
                OnPropertyChanged("Pnl");
            }
        }
        private decimal _price { get; set; }
        public decimal Price
        {
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged("Price");
                Usdt = Quantity * Price;
            }
        }
        private decimal _usdt { get; set; }
        public decimal Usdt
        {
            get { return _usdt; }
            set
            {
                _usdt = value;
                OnPropertyChanged("Usdt");
            }
        }
        private decimal _quantity { get; set; }
        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                if(value < 0m) _quantity = -value;
                else _quantity = value;
                OnPropertyChanged("Quantity");
                Usdt = Quantity * Price;
            }
        }
        public Position(BinanceFuturesStreamPosition position, BinanceClient client, BinanceSocketClient socketClient)
        {
            this.client = client;
            this.socketClient = socketClient;
            Symbol = position.Symbol;
            Quantity = position.Quantity;
            Price = position.EntryPrice;
            PositionSide = position.PositionSide;
            OpenTime = DateTime.Now;
            SubscribeToAggregatedTradeUpdatesAsync();
        }
        public Position(BinancePositionDetailsUsdt position, BinanceClient client, BinanceSocketClient socketClient)
        {
            this.client = client;
            this.socketClient = socketClient;
            Symbol = position.Symbol;
            Quantity = position.Quantity;
            Price = position.EntryPrice;
            PositionSide = position.PositionSide;
            OpenTime = position.UpdateTime.ToLocalTime();
            SubscribeToAggregatedTradeUpdatesAsync();
        }


        private RelayCommand? _closePositionCommand;
        public RelayCommand ClosePositionCommand
        {
            get
            {
                return _closePositionCommand ?? (_closePositionCommand = new RelayCommand(obj => {
                    ClosePosition();
                }));
            }
        }
        private void ClosePosition()
        {
            if(PositionSide == PositionSide.Short) OpenOrderMarketAsync(OrderSide.Buy);
            else if(PositionSide == PositionSide.Long) OpenOrderMarketAsync(OrderSide.Sell);
        }
        private async void OpenOrderMarketAsync(OrderSide side)
        {
            await Task.Run(async () =>
            {
                try
                {
                    var result = await client.UsdFuturesApi.Trading.PlaceOrderAsync(
                        symbol: Symbol,
                        side: side,
                        type: FuturesOrderType.Market,
                        quantity: Quantity,
                        positionSide: PositionSide);
                    if (!result.Success)
                    {
                        Error.WriteLog(pathPositions, Symbol, $"Failed OpenOrderMarketAsync: {result.Error?.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(pathPositions, Symbol, $"Exception OpenOrderMarketAsync: {ex.Message}");
                }
            });
        }
        private async void SubscribeToAggregatedTradeUpdatesAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    int socketId = 0;
                    var result = await socketClient.UsdFuturesStreams.SubscribeToAggregatedTradeUpdatesAsync(Symbol, Message =>
                    {
                        if (Quantity == 0m)
                        {
                            socketClient.UnsubscribeAsync(socketId);
                        }
                        else
                        {
                            if(PositionSide == PositionSide.Long)
                            {
                                Pnl = (Message.Data.Price - Price) * Quantity;
                                PnlPercent = (Message.Data.Price - Price) / Price * 100;
                            }
                            else if(PositionSide == PositionSide.Short)
                            {
                                Pnl = (Price - Message.Data.Price) * Quantity;
                                PnlPercent = (Price - Message.Data.Price) / Message.Data.Price * 100;
                            }
                            if (Pnl < 0m) IsPositive = false;
                            else IsPositive = true;
                        }
                    });
                    if (!result.Success) Error.WriteLog(pathPositions, Symbol, $"Failed SubscribeToAggregatedTradeUpdatesAsync: {result.Error?.Message}");
                    else socketId = result.Data.Id;
                }
                catch (Exception ex)
                {
                    Error.WriteLog(pathPositions, Symbol, $"Exception SubscribeToAggregatedTradeUpdatesAsync: {ex.Message}");
                }
                
            });
        }
    }
}
