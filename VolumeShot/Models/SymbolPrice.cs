using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VolumeShot.Models
{
    public class SymbolPrice
    {
        public decimal Price { get; set; }
        public bool BuyerIsMaker { get; set; }
        public DateTime DateTime { get; set; }
        public SymbolPrice() { }
        public SymbolPrice(decimal price, bool buyerIsMaker, DateTime dateTime, List<SymbolPrice> prices)
        {
            Price = price;
            BuyerIsMaker = buyerIsMaker;
            DateTime = dateTime;
            RunAsync(prices);
        }
        private async void RunAsync(List<SymbolPrice> prices)
        {
            await Task.Run(async () => {

                await Task.Delay(20000);

                try
                {
                    prices.Remove(this);
                }
                catch (Exception ex)
                {
                    Error.WriteLog(LogDirectory.Path, "Prices", $"Exception prices: {ex?.Message}");
                }
            });
        }
    }
}
