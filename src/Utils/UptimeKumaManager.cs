using UptimeKuma.Services;
using k8s.Models;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Threading;
using System.Text.Json;
using System.Xml.Linq;
using System;
using OperatorTester.Entities;
using Microsoft.Extensions.Logging;
using UptimeKuma.Models;
using UptimeKuma.Operator.Entities;
using System.Runtime.CompilerServices;

namespace UptimeKuma.Utils
{
    public class UptimeKumaManager
    {
        private Task? workerTask;
        private KubernetesService kubernetesService;
        private UptimeKumaService uptimeKumaService;
        private readonly SettingsManager settingsManager;
        private readonly ILogger<UptimeKumaManager> logger;
        private readonly CancellationTokenSource cancellationTokenSource;

        public UptimeKumaManager(KubernetesService kubernetesService,
                                 UptimeKumaService uptimeKumaService,
                                 SettingsManager settingsManager,
                                 ILogger<UptimeKumaManager> logger)
        {
            this.kubernetesService = kubernetesService;
            this.uptimeKumaService = uptimeKumaService;
            this.settingsManager = settingsManager;
            this.logger = logger;
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        public void Init()
        {
            workerTask = WorkerTask(TimeSpan.FromMinutes(1), cancellationTokenSource.Token);
            _ = InitialSyncAsync();
        }

        private async Task InitialSyncAsync()
        {
            // Wait until Uptime Kuma has provided initial data.
            while (!uptimeKumaService.IsInitCompleted())
            {
                logger.LogInformation("Waiting for uptime-kuma data for initial sync..");
                await Task.Delay(TimeSpan.FromSeconds(settingsManager.SyncDelay), cancellationTokenSource.Token);
            }

            logger.LogInformation("Running initial sync job..");
            await Update();
        }

        public async Task WorkerTask(TimeSpan interval, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!uptimeKumaService.IsInitCompleted())
                    {
                        logger.LogInformation($"Waiting for uptime-kuma data..");
                        await Task.Delay(TimeSpan.FromSeconds(settingsManager.SyncDelay), token);
                        continue;
                    }

                    await Update();
                    await Task.Delay(interval, token);
                }
                catch (Exception e)
                {
                    logger.LogError($"Error in worker task. ERROR:{e.Message}");
                }
            }
        }

        public async Task Update()
        {
            try
            {
                logger.LogInformation($"Running sync job..");
                //app.uptimekuma/name
                var rawKubeItems = await kubernetesService.GetIngressItems();

                if (!rawKubeItems?.Any() ?? true)
                {
                    await Console.Out.WriteLineAsync("No items returned from kubernetes.. returning");
                    return;
                }

                IEnumerable<V1Ingress> kubernetesIngressItems = settingsManager.KumaLoadAllIngress ? [.. rawKubeItems] : rawKubeItems?.Where(x => x.HasUptimeAnnotation()) ?? new List<V1Ingress>();
                var uptimeKumaList = uptimeKumaService.GetMonitorList().Values;
                var uptimeHttpCRDList = await kubernetesService.GetHttpMonitorEntityItems();
                var uptimeGenericList = await kubernetesService.GetGenericUptimeEntityItems();


                if (!uptimeKumaService.IsInitCompleted())
                {
                    logger.LogInformation("No items uptimeKuma List.. Delaying update");
                    return;
                }
                
                var itemToRemove = uptimeKumaList.Where(x => !kubernetesIngressItems.Any(a => a.GetUptimeName() == x.name) && !uptimeHttpCRDList.Any(a => a.Name() == x.name) && !uptimeGenericList.Any(a => a.Name() == x.name));
                var itemToAdd = kubernetesIngressItems.Where(x => !uptimeKumaList.Any(y => y.name == x.GetUptimeName()));


                foreach (var item in itemToAdd)
                {
                    logger.LogInformation($"Adding monitor. Name={item.GetUptimeName()} URL={item.Detail().URL}");
                    await uptimeKumaService.AddMonitor(item.GetUptimeName(), url: item?.Detail().URL ?? string.Empty, type: "http","ingress");

                }
                 
                foreach (var item in itemToRemove)
                {
                    logger.LogInformation($"Removing monitor. Name={item.name} URL={item.url}");
                    await uptimeKumaService.RemoveMonitor(item.id);
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                }

            }
            catch (Exception ex)
            {
                logger.LogInformation($"Could not run update. ERROR:{ex.Message}");
            }
        }

        internal Task ReconcileHttpUptimeEntity(HttpMonitorEntity entity)
        {
            KumaMonitor? itemToUpdate = uptimeKumaService.GetMonitorList()?.Select(x => x.Value).FirstOrDefault(x => x?.name?.Equals(entity.Name()) ?? false);
            if (itemToUpdate != null)
            {
                itemToUpdate.MapToKumaMonitor(entity);

                return uptimeKumaService.EditMonitor(itemToUpdate, settingsManager.CustomResourceTagName);
            }
            return uptimeKumaService.AddMonitor(entity.Name(), url: entity?.Spec?.Url ?? string.Empty, type: "http", settingsManager.CustomResourceTagName);
        }

        internal Task RemoveHttpUptimeEntity(HttpMonitorEntity entity)
        {
            var uptimeKumaItem = uptimeKumaService.GetMonitorList()?.Values?.FirstOrDefault(x => x.name == entity.Name());
            if (uptimeKumaItem == null)
            {
                return Task.CompletedTask;
            }
            return uptimeKumaService.RemoveMonitor(uptimeKumaItem.id);
        }

        internal Task RemoveIngressEntity(V1Ingress entity)
        {
            var uptimeKumaItem = uptimeKumaService.GetMonitorList()?.Values?.FirstOrDefault(x => x.name == entity.GetUptimeName());
            if (uptimeKumaItem == null)
            {
                return Task.CompletedTask;
            }
            return uptimeKumaService.RemoveMonitor(uptimeKumaItem.id);
        }

        internal Task AddNotification(NotificationUptimeEntity entity)
        {
            entity.Spec.name = entity.Name();
            return uptimeKumaService.AddNotification(entity.Spec);
        }

        internal Task RemoveNotification(NotificationUptimeEntity entity)
        {
            var itemToRemove = uptimeKumaService.GetNotificationList()?.FirstOrDefault(x => x.name == entity.Name());
            if (itemToRemove != null)
            {
                return  uptimeKumaService.RemoveNotification(itemToRemove.id);
            }

            return Task.CompletedTask;
            
        }

        internal Task<bool> AddDashboardUptimeEntity(DashboardUptimeEntity entity)
        {
            return uptimeKumaService.SaveDashboard(entity.Spec);
        }

        internal Task RemoveDashboardUptimeEntity(DashboardUptimeEntity entity)
        {
            return uptimeKumaService.RemoveDashboard(entity.Spec);
        }

        public bool ServicesReady()
        {
            return uptimeKumaService.IsInitCompleted();
        }

        internal Task AddIngressEntity(V1Ingress entity)
        {
            return uptimeKumaService.AddMonitor(entity.Name(), url: entity?.Detail().URL ?? string.Empty, type: "http", "ingress");
        }

        internal async Task ReconcileGenericUptimeEntity(GenericUptimeEntity entity)
        {
            KumaMonitor? itemToUpdate = uptimeKumaService.GetMonitorList()?.Select(x => x.Value).FirstOrDefault(x => x?.name?.Equals(entity.Name()) ?? false);
            if (itemToUpdate != null)
            {
                 entity.Spec.id = itemToUpdate.id;
                 await uptimeKumaService.EditMonitor(entity.Spec, settingsManager.CustomResourceTagName);
                 return;
            }
            await uptimeKumaService.AddMonitor(entity.Spec, settingsManager.CustomResourceTagName);
        }

        internal async Task RemoveGenericUptimeEntity(GenericUptimeEntity entity)
        {
            var uptimeKumaItem = uptimeKumaService.GetMonitorList()?.Values?.FirstOrDefault(x => x.name == entity.Name());
            if (uptimeKumaItem == null)
            {
                return;
            }
            await uptimeKumaService.RemoveMonitor(uptimeKumaItem.id);
        }

        public Dictionary<string, KumaMonitor> GetMonitorList(bool onlyManagedResources = true)
        {
            return uptimeKumaService.GetMonitorList(onlyManagedResources);
        }
    }
}

public static class Extensions
{
    public static string GetUptimeName(this V1Ingress ingress)
    {
        return ingress.HasUptimeAnnotation() ? ingress.Annotations()["app.uptimekuma/name"] : ingress.Name();
    }
    public static bool HasUptimeAnnotation(this V1Ingress ingress)
    {
        return ingress.Annotations()?.ContainsKey("app.uptimekuma/name") ?? false;
    }

    public static (string Name,string? URL,bool isTLS) Detail(this V1Ingress ingress)
    {
        string url = ingress.Spec?.Tls?.Any() ?? false ? $"https://{ingress.Spec.Rules.FirstOrDefault()?.Host ?? "http://URLNotFound:404"}" : $"http://{ingress.Spec.Rules.FirstOrDefault()?.Host ?? "http://URLNotFound:404"}";
        return (Name: GetUptimeName(ingress), URL: url, isTLS: ingress.Spec.Tls?.Any() ?? false);
    }

    
    public static void MapToKumaMonitor(this KumaMonitor kumaMonitor, HttpMonitorEntity entity)
    {
        kumaMonitor.type = entity.Spec.Type;
        kumaMonitor.name = entity.Name();
        kumaMonitor.url = entity.Spec.Url;
        kumaMonitor.method = entity.Spec.Method;
        kumaMonitor.interval = entity.Spec.Interval;
        kumaMonitor.retryInterval = entity.Spec.RetryInterval;
        kumaMonitor.resendInterval = entity.Spec.ResendInterval;
        kumaMonitor.maxretries = entity.Spec.Maxretries;
        kumaMonitor.timeout = entity.Spec.Timeout;
    }
}