using k8s.Models;
using KubeOps.Abstractions.Controller;
using KubeOps.Abstractions.Events;
using KubeOps.Abstractions.Finalizer;
using KubeOps.Abstractions.Queue;
using KubeOps.Abstractions.Rbac;
using KubeOps.KubernetesClient;
using OperatorTester.Entities;
using System.Data;
using System.Xml.Linq;
using UptimeKuma.Operator.Entities;
using UptimeKuma.Services;
using UptimeKuma.Utils;

namespace UptimeKuma.Controller;

[EntityRbac(typeof(ConvertUptimeEntity), Verbs = RbacVerb.All)]
public class ConvertController : IEntityController<ConvertUptimeEntity>
{
    private readonly IKubernetesClient client;
    private readonly ILogger<HttpUptimeEntityController> _logger;
    private readonly UptimeKumaManager uptimeKumaManager;
    private readonly KubernetesService kubernetesService;
    private readonly EntityRequeue<ConvertUptimeEntity> requeue;

    public ConvertController(IKubernetesClient client, ILogger<HttpUptimeEntityController> logger, UptimeKumaManager uptimeKumaManager, KubernetesService kubernetesService, EntityRequeue<ConvertUptimeEntity> requeue)
    {
        this.client = client;
        this._logger = logger;
        this.uptimeKumaManager = uptimeKumaManager;
        this.kubernetesService = kubernetesService;
        this.requeue = requeue;
    }

    public Task DeletedAsync(ConvertUptimeEntity entity, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task ReconcileAsync(ConvertUptimeEntity entity, CancellationToken cancellationToken)
    {
        if (!uptimeKumaManager.ServicesReady())
        {
            requeue(entity, TimeSpan.FromSeconds(10));
            entity.Status.Status = "Waiting..";
            await client.UpdateStatusAsync(entity);
            return;
        }

        Models.KumaMonitor? kumaObj = uptimeKumaManager?.GetMonitorList(false)?.Values?.FirstOrDefault(x =>
        {
            return x.name?.Equals(entity.Name()) ?? false;
        });

        if (kumaObj != null)
        {

            var newEntity = new GenericUptimeEntity() { 
                Spec = kumaObj,
                Metadata = new V1ObjectMeta
                {
                    Name = entity.Name(),
                    NamespaceProperty = entity.Namespace(),
                },
            };
            newEntity.Initialize();
            await kubernetesService.CreateGenericUptimeEntity(newEntity);
        }

        entity.Status.Status = "Converted";
        await client.UpdateStatusAsync(entity);

    }
}