namespace VolumeShot.Models
{
    public class ConfigSelectedSymbol
    {
        public string UserName { get; set; }
        public string Symbol { get; set; }
        public ConfigSelectedSymbol() { }
        public ConfigSelectedSymbol(string userName, string symbol) {
            UserName = userName;
            Symbol = symbol;
        }
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (!(obj is ConfigSelectedSymbol)) return false;
            return (this.UserName == ((ConfigSelectedSymbol)obj).UserName)
                && (this.Symbol == ((ConfigSelectedSymbol)obj).Symbol);
        }
    }
}
