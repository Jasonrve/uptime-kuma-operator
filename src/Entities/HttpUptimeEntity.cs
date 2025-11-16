using k8s.Models;
using KubeOps.Abstractions.Entities;
using KubeOps.Abstractions.Entities.Attributes;
using System.Text.Json.Serialization;

namespace UptimeKuma.Operator.Entities
{
    [KubernetesEntity(Group = "uptime.kuma", ApiVersion = "v1", Kind = "HttpMonitor")]
    [GenericAdditionalPrinterColumn(".spec.status.status", "status", "string")]
    public class HttpMonitorEntity : CustomKubernetesEntity<HttpMonitorEntity.EntitySpec, HttpMonitorEntity.EntityStatus>
    {
        public HttpMonitorEntity()
        {
                
        }
        public class EntitySpec
        {
            public EntitySpec()
            {
                
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Type { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("url")]
            public string? Url { get; set; }
            [JsonPropertyName("method")]
            public string Method { get; set; } = "GET";
            [JsonPropertyName("interval")]
            public int Interval { get; set; } = 60;
            [JsonPropertyName("retryInterval")]
            public int RetryInterval { get; set; } = 60;
            [JsonPropertyName("resendInterval")]
            public int ResendInterval { get; set; } = 60;
            [JsonPropertyName("maxretries")]
            public int Maxretries { get; set; }
            [JsonPropertyName("timeout")]
            public int Timeout { get; set; } = 48;

        }

        public class EntityStatus
        {
            public EntityStatus()
            {
                
            }
            public string test { get; set; } = string.Empty;
        }

    }
}