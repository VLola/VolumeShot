namespace VolumeShot.Models
{
    public class User
    {
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public bool IsTestnet { get; set; }
    }
}
