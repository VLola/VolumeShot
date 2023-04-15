using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using Binance.Net.Objects.Models.Futures.Socket;
using System;
using System.IO;
using System.Threading.Tasks;
using VolumeShot.Command;

namespace VolumeShot.Models
{
    public class Order : Changed
    {
        string pathOrders = Directory.GetCurrentDirectory() + "/log/orders/";
        public BinanceClient client { get; set; }
        public long OrderId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public PositionSide PositionSide { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Usdt { get; set; }
        public DateTime UpdateTime { get; set; }
        public OrderSide Side { get; set; }
        public decimal StopPrice { get; set; }
        private OrderStatus _status { get; set; }
        public OrderStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }
        public Order(BinanceFuturesStreamOrderUpdateData order, BinanceClient client) {
            this.client = client;
            OrderId = order.OrderId;
            Symbol = order.Symbol;
            PositionSide = order.PositionSide;
            Quantity = order.Quantity;
            Price = order.Price;
            Status = order.Status;
            UpdateTime = order.UpdateTime;
            Side = order.Side;
            StopPrice = order.StopPrice;
            Usdt = Quantity * Price;
        }
        public Order(BinanceFuturesOrder order, BinanceClient client)
        {
            this.client = client;
            OrderId = order.Id;
            Symbol = order.Symbol;
            PositionSide = order.PositionSide;
            Quantity = order.Quantity;
            Price = order.Price;
            Status = order.Status;
            UpdateTime = order.UpdateTime;
            Side = order.Side;
            Usdt = Quantity * Price;
            if (order.StopPrice != null) StopPrice = (decimal)order.StopPrice;
        }

        private RelayCommand? _cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new RelayCommand(obj => {
                    CancelOrderAsync();
                }));
            }
        }
        private async void CancelOrderAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    var result = await client.UsdFuturesApi.Trading.CancelOrderAsync(symbol: Symbol, OrderId);
                    if (!result.Success)
                    {
                        Error.WriteLog(pathOrders, Symbol, $"Failed CancelOrderAsync: {result.Error?.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Error.WriteLog(pathOrders, Symbol, $"Exception CancelOrderAsync: {ex.Message}");
                }
            });
        } 
    }
}
