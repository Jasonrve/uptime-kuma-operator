using k8s.Models;
using KubeOps.Abstractions.Controller;
using KubeOps.Abstractions.Finalizer;
using KubeOps.Abstractions.Queue;
using KubeOps.Abstractions.Rbac;
using KubeOps.KubernetesClient;
using UptimeKuma.Operator.Entities;
using UptimeKuma.Utils;

namespace UptimeKuma.Controller;

[EntityRbac(typeof(HttpMonitorEntity), Verbs = RbacVerb.All)]
public class HttpUptimeEntityController : IEntityController<HttpMonitorEntity>
{
    private readonly IKubernetesClient client;
    private readonly ILogger<HttpUptimeEntityController> _logger;
    private readonly UptimeKumaManager uptimeKumaManager;
    private readonly EntityRequeue<HttpMonitorEntity> requeue;

    public HttpUptimeEntityController(IKubernetesClient client,ILogger<HttpUptimeEntityController> logger, UptimeKumaManager uptimeKumaManager, EntityRequeue<HttpMonitorEntity> requeue)
    {
        this.client = client;
        this._logger = logger;
        this.uptimeKumaManager = uptimeKumaManager;
        this.requeue = requeue;
    }

    public Task ReconcileAsync(HttpMonitorEntity entity, CancellationToken cancellationToken)
    {
        if (!uptimeKumaManager.ServicesReady())
        {
            requeue(entity, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }
        return uptimeKumaManager.ReconcileHttpUptimeEntity(entity);
    }

    public Task DeletedAsync(HttpMonitorEntity entity, CancellationToken cancellationToken)
    {
        if (!uptimeKumaManager.ServicesReady())
        {
            requeue(entity, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }
        return uptimeKumaManager.RemoveHttpUptimeEntity(entity);
    }
}
