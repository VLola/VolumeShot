using System;

namespace VolumeShot.Models
{
    public class Order
    {
        public decimal BestAskPrice { get; set; }
        public decimal BestBidPrice { get; set; }
        public DateTime DateTime { get; set; }

    }
}
