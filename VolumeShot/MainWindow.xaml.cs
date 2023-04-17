using System.Windows;
using VolumeShot.ViewModels;

namespace VolumeShot
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
            MainViewModel mainViewModel = new MainViewModel();
            DataContext = mainViewModel;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Close a window?", "Volume shot", MessageBoxButton.OKCancel);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    e.Cancel = false;
                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }
    }
}
