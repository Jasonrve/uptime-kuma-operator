using k8s.Models;
using KubeOps.Abstractions.Entities;
using KubeOps.Abstractions.Entities.Attributes;
using System.Text.Json.Serialization;

namespace OperatorTester.Entities
{
    [KubernetesEntity(Group = "uptime.kuma", ApiVersion = "v1", Kind = "Notification")]
    [GenericAdditionalPrinterColumn(".spec.status.status", "status", "string")]
    public class NotificationUptimeEntity : CustomKubernetesEntity<NotificationUptimeEntity.NotificationUptimeModel, NotificationUptimeEntity.EntityStatus>
    {
        public NotificationUptimeEntity()
        {
            
        }
        public class NotificationUptimeModel
        {
            public NotificationUptimeModel()
            {
                
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? type { get; set; }
            [Ignore]
            public string? name { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("url")]
            public string? url { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("webhookURL")]
            public string? webhookURL { get; set; }
            [JsonPropertyName("isDefault")]
            public bool isDefault { get; set; } = false;
            [JsonPropertyName("webhookContentType")]
            public string webhookContentType { get; set; } = "json";
            [JsonPropertyName("applyExisting")]
            public bool applyExisting { get; set; } = false;
        }

        public class EntityStatus
        {
            public EntityStatus()
            {
                
            }
            public string Status { get; set; } = string.Empty;
        }

    }
}