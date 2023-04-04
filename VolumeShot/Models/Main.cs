using System.Collections.ObjectModel;

namespace VolumeShot.Models
{
    public class Main : Changed
    {
        public ObservableCollection<Symbol> Symbols { get; set; } = new();
    }
}
