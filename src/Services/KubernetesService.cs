using k8s;
using k8s.Models;
using KubeOps.KubernetesClient;
using OperatorTester.Entities;
using UptimeKuma.Controller;
using UptimeKuma.Operator.Entities;

namespace UptimeKuma.Services
{
    public class KubernetesService
    {
        private IKubernetesClient client;

        public KubernetesService()
        {
            var config = KubernetesClientConfiguration.BuildDefaultConfig();
            client = new KubernetesClient(config);
        }


        public async Task<List<V1Ingress>> GetIngressItems()
        {
            return (await client.ListAsync<V1Ingress>()).ToList();

        }

        public async Task<List<HttpMonitorEntity>> GetHttpMonitorEntityItems()
        {
            return (await client.ListAsync<HttpMonitorEntity>()).ToList();

        }

        public async Task<List<GenericUptimeEntity>> GetGenericUptimeEntityItems()
        {
            return (await client.ListAsync<GenericUptimeEntity>()).ToList();

        }

        public async Task<GenericUptimeEntity> CreateGenericUptimeEntity(GenericUptimeEntity entity)
        { 
            return await client.CreateAsync(entity);
        }
    }
}
