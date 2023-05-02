using Binance.Net.Clients;
using Binance.Net.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using VolumeShot.Command;
using VolumeShot.Models;

namespace VolumeShot.ViewModels
{
    internal class LoginViewModel
    {
        private string path = $"{Directory.GetCurrentDirectory()}/configs/users";
        private string pathHistory = $"{Directory.GetCurrentDirectory()}/history/";
        public Login Login { get; set; } = new();
        public BinanceClient Client { get; set; }
        public BinanceSocketClient SocketClient { get; set; }
        private RelayCommand? _authorizeCommand;
        public RelayCommand AuthorizeCommand
        {
            get
            {
                return _authorizeCommand ?? (_authorizeCommand = new RelayCommand(obj => {
                    Authorize();
                }));
            }
        }
        private RelayCommand? _registrationCommand;
        public RelayCommand RegistrationCommand
        {
            get
            {
                return _registrationCommand ?? (_registrationCommand = new RelayCommand(obj => {
                    Registration();
                }));
            }
        }
        public LoginViewModel() {

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                ObservableCollection<User>? users = JsonConvert.DeserializeObject<ObservableCollection<User>>(json);
                if(users != null && users.Count > 0)
                {
                    Login.Users = users;
                    Login.SelectedUser = users[0];
                    foreach (var item in users)
                    {
                        string userHistory = $"{pathHistory}{item.Name}/";
                        if(!Directory.Exists(userHistory))Directory.CreateDirectory(userHistory);
                    }
                }
            }
        }
        private void Authorize()
        {
            if(Login.SelectedUser != null)
            {
                User user = Login.SelectedUser;
                LoginBinanceAsync(user.IsTestnet, user.ApiKey, user.SecretKey);
            }
        }
        private void Registration()
        {
            User user = new();
            user.Name = Login.Name;
            user.ApiKey = Login.ApiKey;
            user.SecretKey = Login.SecretKey;
            user.IsTestnet = Login.IsTestnet;

            Login.Users.Add(user);
            Login.SelectedUser = user;
            string json = JsonConvert.SerializeObject(Login.Users);
            File.WriteAllText(path, json);

            string userHistory = $"{pathHistory}{user.Name}/";
            if (!Directory.Exists(userHistory)) Directory.CreateDirectory(userHistory);

            Login.Name = "";
            Login.ApiKey = "";
            Login.SecretKey = ""; 
            Login.IsTestnet = false;
        }
        private async void LoginBinanceAsync(bool testnet, string apiKey, string secretKey)
        {
            await Task.Run(() => {
                Login.IsLoading = true;
                try
                {
                    if (testnet)
                    {
                        // ------------- Test Api ----------------
                        BinanceClientOptions clientOption = new();
                        clientOption.UsdFuturesApiOptions.BaseAddress = "https://testnet.binancefuture.com";
                        Client = new(clientOption);

                        BinanceSocketClientOptions socketClientOption = new BinanceSocketClientOptions();
                        socketClientOption.UsdFuturesStreamsOptions.AutoReconnect = true;
                        socketClientOption.UsdFuturesStreamsOptions.ReconnectInterval = TimeSpan.FromMinutes(1);
                        socketClientOption.UsdFuturesStreamsOptions.BaseAddress = "wss://stream.binancefuture.com";
                        SocketClient = new BinanceSocketClient(socketClientOption);
                        // ------------- Test Api ----------------
                    }
                    else
                    {
                        // ------------- Real Api ----------------
                        Client = new();
                        SocketClient = new();
                        // ------------- Real Api ----------------
                    }
                    Client.SetApiCredentials(new BinanceApiCredentials(apiKey, secretKey));
                    SocketClient.SetApiCredentials(new BinanceApiCredentials(apiKey, secretKey));

                    if (CheckLogin())
                    {
                        Login.IsLogin = true;
                        MessageBox.Show("Login binance succes!");

                    }
                    else
                    {
                        MessageBox.Show("Login binance failed!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                Login.IsLoading = false;
            });
            
        }
        private bool CheckLogin()
        {
            try
            {
                var result = Client.UsdFuturesApi.Account.GetAccountInfoAsync().Result;
                if (!result.Success) return false;
                else return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
