using System;
using System.Threading.Tasks;

namespace VolumeShot.Models
{
    public class Order : Changed
    {
        public decimal Price { get; set; }
        public bool BuyerIsMaker { get; set; }
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
        public Order(decimal price, bool buyerIsMaker, DateTime dateTime)
        {
            Price = price;
            BuyerIsMaker = buyerIsMaker;
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
