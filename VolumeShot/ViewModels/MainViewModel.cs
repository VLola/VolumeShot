using Binance.Net.Clients;
using System.Collections.Generic;
using System.Linq;
using System;
using VolumeShot.Models;
using Binance.Net.Objects.Models.Futures;
using System.IO;
using Newtonsoft.Json;

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
            List<BinanceFuturesUsdtSymbol> list = ListSymbols();
            if (list.Count > 0)
            {
                List<Config>? configs = new();
                string path = Directory.GetCurrentDirectory() + "/config";
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    configs = JsonConvert.DeserializeObject<List<Config>>(json);
                }
                foreach (var symbol in list)
                {
                    decimal volume = 500000m;
                    if (symbol.QuoteAsset == "USDT")
                    {
                        SymbolViewModel symbolViewModel = new(symbol, volume);
                        Main.Symbols.Add(symbolViewModel.Symbol);
                    }
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
