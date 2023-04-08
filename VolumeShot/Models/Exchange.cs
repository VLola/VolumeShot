using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolumeShot.Models
{
    internal class Exchange
    {
        public string Symbol { get; set; }
        public decimal MinQuantity { get; set; }
        public decimal StepSize { get; set; }
        public decimal TickSize { get; set; }
        public Exchange(BinanceFuturesUsdtSymbol binanceFuturesUsdtSymbol) { 
            Symbol = binanceFuturesUsdtSymbol.Name;
            MinQuantity = binanceFuturesUsdtSymbol.LotSizeFilter.MinQuantity;
            StepSize = binanceFuturesUsdtSymbol.LotSizeFilter.StepSize;
            TickSize = binanceFuturesUsdtSymbol.PriceFilter.TickSize;
        }
    }
}
