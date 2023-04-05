using System.Collections.Generic;

namespace VolumeShot.Models
{
    public class Bet
    {
        public IEnumerable<Order>? Orders { get; set; }
        public decimal PriceBufferLower { get; set; }
        public decimal PriceBuffeUpper { get; set; }
        public decimal PriceDistanceLower { get; set; }
        public decimal PriceDistanceUpper { get; set; }
        public decimal PriceTakeProfit { get; set; }
        public decimal PriceStopLoss { get; set; }
    }
}
