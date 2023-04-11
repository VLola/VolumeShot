using System;
using System.Threading.Tasks;

namespace VolumeShot.Models
{
    public class Order : Changed
    {
        public decimal BestAskPrice { get; set; }
        public decimal BestBidPrice { get; set; }
        public DateTime DateTime { get; set; }
        private bool _isRemove { get; set; }
        public bool IsRemove
        {
            get { return _isRemove; }
            set
            {
                _isRemove = value;
                OnPropertyChanged("IsRemove");
            }
        }
        public Order(decimal bestAskPrice, decimal bestBidPrice, DateTime dateTime)
        {
            BestAskPrice = bestAskPrice;
            BestBidPrice = bestBidPrice;
            DateTime = dateTime;
            RunAsync();
        }
        private async void RunAsync()
        {
            await Task.Run(async () => {
                await Task.Delay(60000);
                IsRemove = true;
            });
        }
    }
}
