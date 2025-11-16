using k8s.Models;
using KubeOps.Abstractions.Entities;
using KubeOps.Abstractions.Entities.Attributes;
using System.Text.Json.Serialization;
using UptimeKuma.Models;
using static OperatorTester.Entities.ConvertUptimeEntity;

namespace OperatorTester.Entities
{
    [KubernetesEntity(Group = "uptime.kuma", ApiVersion = "v1", Kind = "Convert")]
    [GenericAdditionalPrinterColumn(".spec.status.status", "status", "string")]
    public class ConvertUptimeEntity : CustomKubernetesEntity<ConvertModel, EntityStatus>
    {
        public ConvertUptimeEntity()
        { 
        
        }
            
        public class ConvertModel
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }

        public class EntityStatus
        {
            [JsonPropertyName("status")]
            public string Status { get; set; }
        }

    }
}