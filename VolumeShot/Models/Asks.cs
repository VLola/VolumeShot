using Binance.Net.Objects.Models;
using System.Collections.Generic;
using System.Linq;

namespace VolumeShot.Models
{
    public class Asks
    {
        public SortedDictionary<decimal, BinanceOrderBookEntry> Orders = new();
        public void Remove()
        {
            IEnumerable<decimal> list = Orders.Where(order => order.Value.Quantity == 0m).Select(order => order.Key);
            foreach (var entry in list)
            {
                Orders.Remove(entry);
            }
        }
        public void Add(IEnumerable<BinanceOrderBookEntry> binanceOrderBookEntries)
        {
            foreach (var entry in binanceOrderBookEntries)
            {
                Orders[entry.Price] = entry;
            }
        }
        public decimal Min()
        {
            return Orders.Min(order => order.Key);
        }
        public decimal Max()
        {
            return Orders.Max(order => order.Key);
        }
        public decimal GetPrice(decimal volume)
        {
            decimal result = 0m;
            decimal sum = 0m;
            foreach (var item in Orders)
            {
                sum += (item.Value.Quantity * item.Value.Price);
                if(sum >= volume)
                {
                    return item.Key;
                }
            }
            return result;
        }
    }
}
