using System.Collections.ObjectModel;
using System.Windows;
using VolumeShot.Models;

namespace VolumeShot.Views
{
    public partial class HistoryWindow : Window
    {
        public HistoryWindow(ObservableCollection<Bet> bets)
        {
            InitializeComponent();
            DataContext = bets;
        }
    }
}
