namespace UptimeKuma.Models
{
    public class MonitorDashboard
    {
        public int id { get; set; }
        public string? name { get; set; }
        public int sendUrl { get; set; }
        public string? type { get; set; }
        public List<object>? tags { get; set; }
        public string? url { get; set; }
    }

    public class PublicGroupDashboardList
    {
        public int id { get; set; }
        public string? name { get; set; }
        public int weight { get; set; }
        public List<MonitorDashboard>? monitorList { get; set; }
    }

    public class KumaDashboard
    {
        public bool ok { get; set; }
        public List<PublicGroupDashboardList>? publicGroupList { get; set; }
    }


}
