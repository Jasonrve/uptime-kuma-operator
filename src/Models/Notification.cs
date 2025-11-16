namespace UptimeKuma.Models
{
    public class Notification
    {
        public int id { get; set; }
        public string? name { get; set; }
        public bool active { get; set; }
        public int userId { get; set; }
        public bool isDefault { get; set; }
        public string? config { get; set; }
    }
}
