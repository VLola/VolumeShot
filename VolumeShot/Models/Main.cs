using System.Collections.ObjectModel;

namespace VolumeShot.Models
{
    public class Main : Changed
    {
        public ObservableCollection<Symbol> Symbols { get; set; } = new();
        private Symbol _selectedSymbol { get; set; }
        public Symbol SelectedSymbol
        {
            get { return _selectedSymbol; }
            set
            {
                _selectedSymbol = value;
                OnPropertyChanged("SelectedSymbol");
            }
        }
    }
}
