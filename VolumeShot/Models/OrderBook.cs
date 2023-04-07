using Binance.Net.Objects.Models;
using System.Collections.Generic;
using System.Linq;

namespace VolumeShot.Models
{
    public class OrderBook
    {
        public SortedDictionary<decimal, BinanceOrderBookEntry> Asks = new();
        public SortedDictionary<decimal, BinanceOrderBookEntry> Bids = new(new IntegerDecreaseComparer());
        public void AddAsks(IEnumerable<BinanceOrderBookEntry> binanceOrderBookEntries)
        {
            IEnumerable<decimal> list = binanceOrderBookEntries.Where(order => order.Quantity == 0m).Select(order => order.Price);
            foreach (var entry in list) Asks.Remove(entry);
            foreach (var entry in binanceOrderBookEntries) if(entry.Quantity != 0m) Asks[entry.Price] = entry;
        }
        public decimal MinAsk()
        {
            return Asks.Min(order => order.Key);
        }
        public decimal GetPriceAsks(decimal volume)
        {
            decimal result = 0m;
            decimal sum = 0m;
            foreach (var item in Asks)
            {
                sum += (item.Value.Quantity * item.Value.Price);
                if (sum >= volume)
                {
                    return item.Key;
                }
            }
            return result;
        }
        public void AddBids(IEnumerable<BinanceOrderBookEntry> binanceOrderBookEntries)
        {
            IEnumerable<decimal> list = binanceOrderBookEntries.Where(order => order.Quantity == 0m).Select(order => order.Price);
            foreach (var entry in list) Bids.Remove(entry);
            foreach (var entry in binanceOrderBookEntries) if (entry.Quantity != 0m) Bids[entry.Price] = entry;
        }
        public decimal MaxBid()
        {
            return Bids.Max(order => order.Key);
        }
        public decimal GetPriceBids(decimal volume)
        {
            decimal result = 0m;
            decimal sum = 0m;
            foreach (var item in Bids)
            {
                sum += (item.Value.Quantity * item.Value.Price);
                if (sum >= volume)
                {
                    return item.Key;
                }
            }
            return result;
        }
        public class IntegerDecreaseComparer : IComparer<decimal>
        {
            public int Compare(decimal x, decimal y) { return -x.CompareTo(y); }
        }
    }
}
