using System;
using System.Collections.Generic;
using VolumeShot.Command;
using VolumeShot.Views;

namespace VolumeShot.Models
{
    public class Bet
    {
        public IEnumerable<Order>? Orders { get; set; }
        public decimal PriceBufferLower { get; set; }
        public decimal PriceBufferUpper { get; set; }
        public decimal PriceDistanceLower { get; set; }
        public decimal PriceDistanceUpper { get; set; }
        public decimal PriceTakeProfit { get; set; }
        public decimal PriceStopLoss { get; set; }
        public decimal BufferLower { get; set; }
        public decimal BufferUpper { get; set; }
        public decimal DistanceLower { get; set; }
        public decimal DistanceUpper { get; set; }
        public decimal TakeProfit { get; set; }
        public decimal StopLoss { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal ClosePrice { get; set; }

        private RelayCommand? _showCommand;
        public RelayCommand ShowCommand
        {
            get
            {
                return _showCommand ?? (_showCommand = new RelayCommand(obj => {
                    ChartWindow chartWindow = new(this);
                    chartWindow.Show();
                }));
            }
        }
    }
}
