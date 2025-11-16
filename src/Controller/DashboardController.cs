using k8s.Models;
using KubeOps.Abstractions.Controller;
using KubeOps.Abstractions.Events;
using KubeOps.Abstractions.Finalizer;
using KubeOps.Abstractions.Queue;
using KubeOps.Abstractions.Rbac;
using KubeOps.KubernetesClient;
using OperatorTester.Entities;
using System.Data;
using UptimeKuma.Operator.Entities;
using UptimeKuma.Utils;

namespace UptimeKuma.Controller;

[EntityRbac(typeof(DashboardUptimeEntity), Verbs = RbacVerb.All)]
public class DashboardEntityController : IEntityController<DashboardUptimeEntity>
{
    private readonly IKubernetesClient client;
    private readonly ILogger<HttpUptimeEntityController> _logger;
    private readonly UptimeKumaManager uptimeKumaManager;
    private readonly EntityRequeue<DashboardUptimeEntity> requeue;

    public DashboardEntityController(IKubernetesClient client, ILogger<HttpUptimeEntityController> logger, UptimeKumaManager uptimeKumaManager, EntityRequeue<DashboardUptimeEntity> requeue)
    {
        this.client = client;
        this._logger = logger;
        this.uptimeKumaManager = uptimeKumaManager;
        this.requeue = requeue;
    }

    public async Task ReconcileAsync(DashboardUptimeEntity entity, CancellationToken cancellationToken)
    {
        if (!uptimeKumaManager.ServicesReady())
        {
            requeue(entity, TimeSpan.FromSeconds(10));
            return;
        }

        bool result = await uptimeKumaManager.AddDashboardUptimeEntity(entity);

        if (result)
        {
            entity.Status.Status = "Registered";
            entity.Status.Retries = 0;
            await client.UpdateStatusAsync(entity);
            _logger.LogWarning($"Could not Reconcile {entity.Name()}. Stopping to retry");
        }
        else if (!result && entity.Status.Retries <= 6)
        {
            requeue(entity, TimeSpan.FromSeconds(10));
        }
        else
        {
            requeue(entity, TimeSpan.FromSeconds(60));
        }

        entity.Status.Retries++;
        entity.Status.Status = "Unsuccesfull";
        await client.UpdateStatusAsync(entity);

    }

    public async Task DeletedAsync(DashboardUptimeEntity entity, CancellationToken cancellationToken)
    {
        if (!uptimeKumaManager.ServicesReady())
        {
            requeue(entity, TimeSpan.FromSeconds(10));
            return;
        }

        await uptimeKumaManager.RemoveDashboardUptimeEntity(entity);
    }
}