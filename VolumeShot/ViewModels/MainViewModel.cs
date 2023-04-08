using Binance.Net.Clients;
using System.Collections.Generic;
using System.Linq;
using System;
using VolumeShot.Models;
using Binance.Net.Objects.Models.Futures;
using System.IO;
using Newtonsoft.Json;
using CryptoExchange.Net.Interfaces;
using System.Threading.Tasks;
using Binance.Net.Objects.Models.Futures.Socket;

namespace VolumeShot.ViewModels
{
    internal class MainViewModel
    {
        string errorFile = "Binance";
        public Main Main { get; set; } = new();
        public BinanceClient client = new();
        public BinanceSocketClient socketClient = new();
        public delegate void AccountOnOrderUpdate(BinanceFuturesStreamOrderUpdate OrderUpdate);
        public event AccountOnOrderUpdate? OnOrderUpdate;
        public MainViewModel()
        {
            Load();
        }
        private async void SubscribeToUserDataUpdatesAsync()
        {
            var listenKey = await client.UsdFuturesApi.Account.StartUserStreamAsync();
            if (!listenKey.Success)
            {
                Error.WriteLog("", errorFile, $"Failed to start user stream: listenKey");
            }
            else
            {
                KeepAliveUserStreamAsync(listenKey.Data);
                Error.WriteLog("", errorFile, $"Listen Key Created");
                var result = await socketClient.UsdFuturesStreams.SubscribeToUserDataUpdatesAsync(listenKey: listenKey.Data,
                    onLeverageUpdate => { },
                    onMarginUpdate => { },
                    onAccountUpdate => { },
                    onOrderUpdate =>
                    {
                        OnOrderUpdate?.Invoke(onOrderUpdate.Data);
                    },
                    onListenKeyExpired => { },
                    onStrategyUpdate => { },
                    onGridUpdate => { });
                if (!result.Success)
                {
                    Error.WriteLog("", errorFile, $"Failed UserDataUpdates: {result.Error?.Message}");
                }
            }
        }
        private async void KeepAliveUserStreamAsync(string listenKey)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    var result = await client.UsdFuturesApi.Account.KeepAliveUserStreamAsync(listenKey);
                    if (!result.Success) Error.WriteLog("", errorFile, $"Failed KeepAliveUserStreamAsync: {result.Error?.Message}");
                    else
                    {
                        Error.WriteLog("", errorFile, "Success KeepAliveUserStreamAsync");
                    }
                    await Task.Delay(900000);
                }
            });
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
                        if (configs != null)
                        {
                            Config? config = configs.FirstOrDefault(conf => conf.Name == symbol.Name);
                            if (config != null) volume = config.Volume;
                        }
                        SymbolViewModel symbolViewModel = new(symbol, volume);
                        OnOrderUpdate += symbolViewModel.OrderUpdate;
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
