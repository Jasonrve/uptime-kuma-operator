using k8s.Models;
using KubeOps.Abstractions.Controller;
using KubeOps.Abstractions.Finalizer;
using KubeOps.Abstractions.Queue;
using KubeOps.Abstractions.Rbac;
using KubeOps.KubernetesClient;
using UptimeKuma.Operator.Entities;
using UptimeKuma.Utils;

namespace UptimeKuma.Controller;

[EntityRbac(typeof(GenericUptimeEntity), Verbs = RbacVerb.All)]
public class GenericUptimeEntityController : IEntityController<GenericUptimeEntity>
{
    private readonly IKubernetesClient client;
    private readonly ILogger<GenericUptimeEntityController> _logger;
    private readonly UptimeKumaManager uptimeKumaManager;
    private readonly EntityRequeue<GenericUptimeEntity> requeue;

    public GenericUptimeEntityController(IKubernetesClient client, ILogger<GenericUptimeEntityController> logger, UptimeKumaManager uptimeKumaManager, EntityRequeue<GenericUptimeEntity> requeue)
    {
        this.client = client;
        _logger = logger;
        this.uptimeKumaManager = uptimeKumaManager;
        this.requeue = requeue;
    }

    public Task ReconcileAsync(GenericUptimeEntity entity, CancellationToken cancellationToken)
    {
        if (!uptimeKumaManager.ServicesReady())
        {
            requeue(entity, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }
        return uptimeKumaManager.ReconcileGenericUptimeEntity(entity);
    }

    public Task DeletedAsync(GenericUptimeEntity entity, CancellationToken cancellationToken)
    {
        if (!uptimeKumaManager.ServicesReady())
        {
            requeue(entity, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }
        return uptimeKumaManager.RemoveGenericUptimeEntity(entity);
    }
}