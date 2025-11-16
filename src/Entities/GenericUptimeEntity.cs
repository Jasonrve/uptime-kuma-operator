using k8s.Models;
using KubeOps.Abstractions.Entities;
using KubeOps.Abstractions.Entities.Attributes;
using UptimeKuma.Models;

namespace UptimeKuma.Operator.Entities
{
    [KubernetesEntity(Group = "uptime.kuma", ApiVersion = "v1", Kind = "GenericMonitor")]
    [GenericAdditionalPrinterColumn(".spec.status.status", "status", "string")]
    public class GenericUptimeEntity : CustomKubernetesEntity<KumaMonitor, HttpMonitorEntity.EntityStatus>
    {
        public GenericUptimeEntity()
        {
            
        }
        public class EntityStatus
        {
            public EntityStatus()
            {
                
            }
        }



    }
}