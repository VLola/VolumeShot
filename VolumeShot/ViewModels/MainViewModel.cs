using Binance.Net.Clients;
using System.Collections.Generic;
using System.Linq;
using System;
using VolumeShot.Models;
using Binance.Net.Objects.Models.Futures;

namespace VolumeShot.ViewModels
{
    internal class MainViewModel
    {
        string errorFile = "Binance";
        public Main Main { get; set; } = new();
        public MainViewModel()
        {
            Load();
        }
        private void Load()
        {
            int i = 0;
            List<BinanceFuturesUsdtSymbol> list = ListSymbols();
            if (list.Count > 0)
            {
                foreach (var symbol in list)
                {
                    if (symbol.QuoteAsset == "USDT")
                    {
                        i++;
                        SymbolViewModel symbolViewModel = new(symbol);
                        Main.Symbols.Add(symbolViewModel.Symbol);
                    }
                    if (i > 25) break;
                }
            }
        }
        private List<BinanceFuturesUsdtSymbol> ListSymbols()
        {
            try
            {
                BinanceClient client = new();
                var result = client.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync().Result;
                if (!result.Success)
                {
                    Error.WriteLog("", errorFile, $"Failed ListSymbols {result.Error?.Message}");
                    return new();
                }
                return result.Data.Symbols.ToList();

            }
            catch (Exception ex)
            {
                Error.WriteLog("", errorFile, $"Exception ListSymbols: {ex.Message}");
                return new();
            }
        }
    }
}
