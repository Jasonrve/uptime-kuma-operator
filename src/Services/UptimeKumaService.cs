using k8s.Models;
using OperatorTester.Entities;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using UptimeKuma.Models;
using UptimeKuma.Operator.Entities;
using static OperatorTester.Entities.ConvertUptimeEntity;
using static OperatorTester.Entities.DashboardUptimeEntity;
using static OperatorTester.Entities.NotificationUptimeEntity;

namespace UptimeKuma.Services
{
    [JsonSourceGenerationOptions(
    WriteIndented = true,
    //    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
    [JsonSerializable(typeof(KumaMonitor))]
    [JsonSerializable(typeof(Notification))]
    [JsonSerializable(typeof(NotificationUptimeEntity))]
    [JsonSerializable(typeof(NotificationUptimeEntity.EntityStatus))]
    [JsonSerializable(typeof(NotificationUptimeEntity.NotificationUptimeModel))]
    [JsonSerializable(typeof(GenericUptimeEntity))]
    [JsonSerializable(typeof(GenericUptimeEntity.EntityStatus))]
    [JsonSerializable(typeof(DashboardUptimeEntity))]
    [JsonSerializable(typeof(DashboardUptimeEntity.DashboardModel))]
    [JsonSerializable(typeof(DashboardUptimeEntity.EntityStatus))]
    [JsonSerializable(typeof(DashboardModel))]
    [JsonSerializable(typeof(Group))]
    [JsonSerializable(typeof(HttpMonitorEntity))]
    [JsonSerializable(typeof(HttpMonitorEntity.EntitySpec))]
    [JsonSerializable(typeof(HttpMonitorEntity.EntityStatus))]
    [JsonSerializable(typeof(Dictionary<string, KumaMonitor>))]
    [JsonSerializable(typeof(KafkaProducerSaslOptions))]
    [JsonSerializable(typeof(Tag))]
    [JsonSerializable(typeof(List<Dictionary<string, KumaMonitor>>))]
    [JsonSerializable(typeof(List<List<Notification>>))]
    [JsonSerializable(typeof(ConvertUptimeEntity))]
    [JsonSerializable(typeof(ConvertModel))]
    
    internal partial class SerializationModeOptions : JsonSerializerContext
    {

    }

    public class UptimeKumaService
    {
        private SocketIOClient.SocketIO client;
        private Dictionary<string, KumaMonitor> monitorList = new Dictionary<string, KumaMonitor>();
        private List<Notification> notificationList = new List<Notification> { };
        private KumaDashboard dashboardList = new KumaDashboard();
        public Dictionary<string, bool> isInitCompleted = new Dictionary<string, bool>()
        {
            { "monitorList", false },
            { "notificationList", false}
        };
        private int? monitoringTagId = null;

        private readonly ILogger<UptimeKumaService> logger;
        private readonly SettingsManager settingsManager;

        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(k8s.Watcher<>))]
        public UptimeKumaService(ILogger<UptimeKumaService> logger, SettingsManager settingsManager)
        {
            try
            {
                this.logger = logger;
                this.settingsManager = settingsManager;

                if (string.IsNullOrWhiteSpace(settingsManager.KumaHost))
                {
                    logger.LogError("KumaHost is not configured. UptimeKumaService will not start.");
                    return;
                }

                string url = settingsManager.KumaHost;
                client = new SocketIOClient.SocketIO(url);

                client.OnReconnectError += Client_OnError;
                client.OnReconnectFailed += Client_ReconnectFailed;
                client.OnReconnected += Client_OnReconnected;
                client.OnDisconnected += Client_OnDisconnected;
                client.OnError += Client_OnError;
                client.OnConnected += Client_OnConnected;

                // Listen to any event
                client.OnAny((eventName, data) =>
                {
                    try
                    {
                        switch (eventName)
                        {
                            case "monitorList":
                                {
                                    logger.LogInformation("Received monitorList");
                                    var ret = System.Text.Json.JsonSerializer.Deserialize(data.ToString(), typeof(List<Dictionary<string, KumaMonitor>>), SerializationModeOptions.Default);
                                    monitorList = (ret as List<Dictionary<string, KumaMonitor>>)?.FirstOrDefault() ?? new Dictionary<string, KumaMonitor>();
                                    isInitCompleted["monitorList"] = true;
                                }
                                break;
                            case "notificationList":
                                {
                                    logger.LogInformation("Received notificationList");
                                    var ret = System.Text.Json.JsonSerializer.Deserialize(data.ToString(), typeof(List<List<Notification>>), SerializationModeOptions.Default);
                                    notificationList = (ret as List<List<Notification>>)?.FirstOrDefault() ?? new List<Notification>();
                                    isInitCompleted["notificationList"] = true;
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error while handling socket event {EventName}.", eventName);
                    }
                });

                logger.LogDebug("Starting Uptime Kuma connection...");
                _ = client.ConnectAsync();

            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }

        private void Client_OnConnected(object? sender, EventArgs e)
        {
            logger.LogInformation($"Uptimekuma connected successfully");
            Task.Run(SetupKuma);
        }

        private void Client_OnError(object? sender, string e)
        {
            logger.LogError("Client error received");
            logger.LogError(e);
        }

        private void Client_OnReconnected(object? sender, int e)
        {
            logger.LogInformation("Reconnected");
        }

        private void Client_ReconnectFailed(object? sender, EventArgs e)
        {
            logger.LogError("Reconnect failed");
        }

        private void Client_OnDisconnected(object? sender, string e)
        {
            logger.LogError("Disconnected");
            logger.LogError(e);
        }

        private void Client_OnError(object? sender, Exception e)
        {
            logger.LogError("Client error");
            logger.LogError(e.Message);
        }

        public async Task SetupKuma()
        {
            try
            {
                string username = settingsManager.KumaUsername;
                string password = settingsManager.KumaPassword;

                await client
                    .Exec("setup", username, password)
                    .Success((msg) =>
                        {
                            logger.LogInformation("Uptime Kuma successfully registered new user.");
                        })
                    .Failure((msg) =>
                        {
                            logger.LogError("Uptime Kuma user registration failed.");
                        });

                await client.Exec("login", new { username, password, token = " " })
                    .Success(async (msg) =>
                    {
                        logger.LogInformation("Login successful.");
                        await SetupTags();
                    })
                    .Failure((msg) =>
                    {
                        logger.LogWarning($"Login failed. Detail: {msg}");
                    });


            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }

        private async Task SetupTags()
        {
            await GetTag();
        }

        private async Task GetTag()
        {
            await client.Exec("getTags")
                    .Success((ret) =>
                    {
                        var tagsNode = ret?["tags"];
                        if (tagsNode is not JsonArray tags)
                        {
                            logger.LogWarning("getTags response did not contain a tags array.");
                            return;
                        }

                        foreach (var tag in tags)
                        {
                            if (tag?["name"]?.ToString() == settingsManager.KumaTag)
                            {
                                if (tag?["id"] is JsonValue idValue && idValue.TryGetValue<int>(out var id))
                                {
                                    monitoringTagId = id;
                                    break;
                                }
                            }
                        }
                    });


            await client.EmitAsync("getTags", async (response) =>
            {
                if (monitoringTagId == null)
                {
                    await client.Exec("addTag", new
                    {
                        color = "#2563EB",
                        name = settingsManager.KumaTag,
                        value = "",
                        @new = true
                    }).Success((response) =>
                    {
                        monitoringTagId = (int)response["tag"]["id"];

                        logger.LogInformation("Uptime Kuma tag successfully registered.");
                    });
                }
            });
        }

        public async Task AddMonitor(string name, string url, string type, string monitortype)
        {
            await AddMonitor(new KumaMonitor { type = type, name = name, url = url, description = "" }, monitortype);
        }

        public async Task AddMonitor(KumaMonitor monitor, string monitortype = "")
        {
            await client.Exec("add", monitor)
                .Success(async (response) =>
                {
                    var monitorIdNode = response?["monitorID"];
                    if (monitorIdNode is JsonValue idValue && idValue.TryGetValue<int>(out var monitorID) && monitorID != 0)
                    {
                        await AddMonitorTag(Convert.ToInt32(monitorID), monitoringTagId, monitortype);
                    }
                });
        }


        public async Task EditMonitor(KumaMonitor entity, string monitortype)
        {
            await client.Exec("editMonitor", entity)
                .Success(async (response) =>
                {
                    if (entity.tags != null && entity.tags.Any(x => x.name == settingsManager.KumaTag))
                    {
                        await AddMonitorTag(entity.id, monitoringTagId, monitortype);
                    }
                });
        }

        public async Task RemoveMonitor(int id)
        {
            await client.Exec("deleteMonitor", id)
                .Success((response) =>
                {
                    //TODO: validate removed monitor

                });
        }

        public async Task AddMonitorTag(int monitorid, int? tagid, string value = "")
        {
            await client.Exec("addMonitorTag", tagid, monitorid, value);
        }


        public async Task AddNotification(NotificationUptimeModel model)
        {
            await client.Exec("addNotification", model, notificationList.FirstOrDefault(x => x.name?.Equals(model.name) ?? false)?.id)
                .Success((ret) =>
                {
                    logger.LogDebug($"{model.name} Notification added successfully");
                });
        }

        public Dictionary<string, KumaMonitor> GetMonitorList(bool onlyManagedResources = true)
        {
            if (onlyManagedResources)
            {
                return new Dictionary<string, KumaMonitor>(monitorList.Where(x => x.Value?.tags?.Any(x => x.name == settingsManager.KumaTag) ?? false));
            }

            return monitorList;
        }

        internal async Task RemoveNotification(int id)
        {
            await client.Exec("deleteNotification", id);
        }

        internal async Task<bool> SaveDashboard(DashboardModel dashboard)
        {
            bool successfull = true;
            if (!monitorList.Any())
                return false;

            JsonArray jsonObject = new JsonArray
            {
                dashboard.Name
            };

            JsonObject innerObject = new JsonObject
            {
                ["slug"] = dashboard.Slug,
                ["id"] = 1000,
                ["title"] = dashboard.Name,
                ["description"] = dashboard.Description,
                ["icon"] = "/upload/logo1.png?t=1713282342148",
                ["theme"] = "auto",
                ["showTags"] = true,
                ["domainNameList"] = new JsonArray(),
                ["customCSS"] = "body {\n  \n}\n",
                ["footerText"] = "",
                ["showPoweredBy"] = false,
                ["googleAnalyticsId"] = "",
                ["showCertificateExpiry"] = dashboard.ShowCertificateExpiry
            };

            jsonObject.Add(innerObject);
            jsonObject.Add("/upload/logo1.png?t=1713282342148");
            JsonArray jsonArray = new JsonArray();

            foreach (Group group in dashboard.Group)
            {
                JsonObject innerArrayObject = new JsonObject
                {
                    ["id"] = dashboard.Group.IndexOf(group),
                    ["name"] = group.Name,
                    ["weight"] = group.Weight,
                    ["monitorList"] = new JsonArray() // Fixing the self-referencing loop here
                };


                if (group?.MonitorList != null)
                    foreach (string monitor in group.MonitorList)
                    {
                        var detail = GetMonitorList().FirstOrDefault(x => x.Value.name == monitor).Value;

                        if (detail == null)
                        {
                            logger.LogWarning($"Cannot find monitor named '{monitor}' mising config for this monitor?");
                            successfull = false;
                            continue;
                        }

                        if (innerArrayObject.ContainsKey("monitorList") && innerArrayObject["monitorList"] is JsonArray monitorList)
                        {
                            monitorList.Add(new JsonObject
                            {
                                ["id"] = detail?.id,
                                ["name"] = detail?.name,
                                ["sendUrl"] = 0,
                                ["type"] = detail?.type,
                                ["tags"] = new JsonArray()
                            });
                        }
                    }

                jsonArray.Add(innerArrayObject);
            }

            jsonObject.Add(jsonArray);
            string json = jsonObject.ToString();

            await client.Exec("addStatusPage", dashboard.Name, dashboard.Slug)
                .Success((ret) =>
                    {
                        logger.LogDebug($"addStatusPage status:{ret}");
                    });

            await client.Exec("saveStatusPage", jsonObject[0], jsonObject[1], jsonObject[2], jsonObject[3])
                .Success((response) =>
                    {
                        logger.LogDebug($"saveStatusPage status:{response}");
                    });

            return successfull;
        }

        internal List<Notification> GetNotificationList()
        {
            return notificationList;
        }

        internal async Task RemoveDashboard(DashboardModel spec)
        {
            await client.Exec("deleteStatusPage", spec.Slug)
                .Success((response) =>
                    {
                        logger.LogDebug($"saveStatusPage status:{response}");
                    });
        }

        internal bool IsInitCompleted()
        {
            return isInitCompleted.All(x => x.Value);
        }
    }
}