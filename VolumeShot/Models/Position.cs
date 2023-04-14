using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures.Socket;
using System;
using System.IO;
using System.Threading.Tasks;
using VolumeShot.Command;

namespace VolumeShot.Models
{
    public class Position : Changed
    {
        string pathPositions = Directory.GetCurrentDirectory() + "/log/positions/"; 
        private BinanceClient client { get; set; }
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal Usdt { get; set; }
        public PositionSide PositionSide { get; set; }
        public DateTime OpenTime { get; set; }
        private decimal _quantity { get; set; }
        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                if(value < 0m) _quantity = -value;
                else _quantity = value;
                OnPropertyChanged("Quantity");
            }
        }
        public Position(BinanceFuturesStreamPosition position, BinanceClient client)
        {
            this.client = client;
            Symbol = position.Symbol;
            Quantity = position.Quantity;
            Price = position.EntryPrice;
            PositionSide = position.PositionSide;
            Usdt = Quantity * Price;
            OpenTime = DateTime.UtcNow;
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
    }
}
