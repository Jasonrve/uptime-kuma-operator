using k8s.Models;
using KubeOps.Abstractions.Entities;
using KubeOps.Abstractions.Entities.Attributes;
using System.Text.Json.Serialization;
using UptimeKuma.Models;
using static OperatorTester.Entities.DashboardUptimeEntity;

namespace OperatorTester.Entities
{
    [KubernetesEntity(Group = "uptime.kuma", ApiVersion = "v1", Kind = "Dashboard")]
    [GenericAdditionalPrinterColumn(".spec.status.status", "status", "string")]
    public class DashboardUptimeEntity : CustomKubernetesEntity<DashboardModel, EntityStatus>
    {
        public DashboardUptimeEntity()
        {
            
        }
        public class DashboardModel
        {
            public DashboardModel()
            {
                
            }
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [Ignore]
            public string? Slug { get { return Name; } }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Path { get; set; }
            [JsonPropertyName("group")]
            public List<Group> Group { get; set; } = new List<Group>();
            [JsonPropertyName("weight")]
            public int Weight { get; set; }
            [JsonPropertyName("description")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Description { get; set; }
            [JsonPropertyName("showCertificateExpiry")]
            public bool ShowCertificateExpiry { get; set; }

        }

        public class Group
        {
            public Group()
            {
                
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Name { get; set; }
            [JsonPropertyName("monitorList")]
            public List<string>? MonitorList { get; set; }
            [JsonPropertyName("weight")]
            public int Weight { get; set; }
        }

        public class EntityStatus
        {
            public EntityStatus()
            {
                
            }
            [JsonPropertyName("status")]
            public string Status { get; set; } = string.Empty;
            [JsonPropertyName("retries")]
            public int Retries { get; set; } = 0;
        }

    }
}