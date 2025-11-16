using k8s.Models;
using KubeOps.Abstractions.Controller;
using KubeOps.Abstractions.Finalizer;
using KubeOps.Abstractions.Queue;
using KubeOps.Abstractions.Rbac;
using KubeOps.KubernetesClient;
using OperatorTester.Entities;
using UptimeKuma.Utils;

namespace UptimeKuma.Controller;

[EntityRbac(typeof(NotificationUptimeEntity), Verbs = RbacVerb.All)]
public class NotificationUptimeEntityController : IEntityController<NotificationUptimeEntity>
{
    private readonly IKubernetesClient client;
    private readonly ILogger<NotificationUptimeEntityController> _logger;
    private readonly UptimeKumaManager uptimeKumaManager;
    private readonly EntityRequeue<NotificationUptimeEntity> requeue;

    public NotificationUptimeEntityController(IKubernetesClient client, ILogger<NotificationUptimeEntityController> logger, UptimeKumaManager uptimeKumaManager, EntityRequeue<NotificationUptimeEntity> requeue)
    {
        this.client = client;
        this._logger = logger;
        this.uptimeKumaManager = uptimeKumaManager;
        this.requeue = requeue;
    }

    public Task DeletedAsync(NotificationUptimeEntity entity, CancellationToken cancellationToken)
    {
        if (!uptimeKumaManager.ServicesReady())
            return Task.Run(() => { requeue(entity, TimeSpan.FromSeconds(10)); });
        return uptimeKumaManager.RemoveNotification(entity);
    }

    public Task ReconcileAsync(NotificationUptimeEntity entity, CancellationToken cancellationToken)
    {
        if (!uptimeKumaManager.ServicesReady())
            return Task.Run(() => { requeue(entity, TimeSpan.FromSeconds(10)); });
        return uptimeKumaManager.AddNotification(entity);
    }
}