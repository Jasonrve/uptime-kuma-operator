using k8s.Models;
using KubeOps.Abstractions.Controller;
using KubeOps.Abstractions.Queue;
using KubeOps.Abstractions.Rbac;
using OperatorTester.Entities;
using UptimeKuma.Utils;

namespace UptimeKuma.Controller;

[EntityRbac(typeof(V1Ingress), Verbs = RbacVerb.All)]
public class IngressController : IEntityController<V1Ingress>
{
    private readonly ILogger<HttpUptimeEntityController> _logger;
    private readonly UptimeKumaManager uptimeKumaManager;
    private readonly EntityRequeue<V1Ingress> requeue;

    public IngressController(ILogger<HttpUptimeEntityController> logger, UptimeKumaManager uptimeKumaManager, EntityRequeue<V1Ingress> requeue)
    {
        this._logger = logger;
        this.uptimeKumaManager = uptimeKumaManager;
        this.requeue = requeue;
    }

    public Task ReconcileAsync(V1Ingress entity, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"entity {entity.Name()} called {nameof(ReconcileAsync)}.");
        if (!uptimeKumaManager.ServicesReady())
        {
            requeue(entity, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }
        return uptimeKumaManager.AddIngressEntity(entity);
    }

    public Task DeletedAsync(V1Ingress entity, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"entity {entity.Name()} called {nameof(DeletedAsync)}.");
        if (!uptimeKumaManager.ServicesReady())
        {
            requeue(entity, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }
        return uptimeKumaManager.RemoveIngressEntity(entity);
    }
}