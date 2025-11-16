
namespace UptimeKuma
{
    public class SettingsManager
    {
        private const string KumaHostSetting = "kuma_host";
        private const string KumaPasswordSetting = "kuma_password";
        private const string KumaUsernameSetting = "kuma_username";
        private const string KumaLoadAllIngressSetting = "kuma_load_all_ingress";
        private const string SyncDelaySetting = "sync_delay";
        private readonly ILogger<SettingsManager> logger;

        public SettingsManager(ILogger<SettingsManager> logger)
        {
            this.logger = logger;
        }


        public string KumaPassword { get; set; } = Environment.GetEnvironmentVariable(KumaPasswordSetting) ?? string.Empty;

        public string KumaHost { get; set; } = Environment.GetEnvironmentVariable(KumaHostSetting) ?? string.Empty;

        public string KumaUsername { get; set; } = Environment.GetEnvironmentVariable(KumaUsernameSetting) ?? string.Empty;

        public bool KumaLoadAllIngress { get; set; } =
            bool.TryParse(Environment.GetEnvironmentVariable(KumaLoadAllIngressSetting), out var loadAll) && loadAll;

        public long SyncDelay { get; set; } = Convert.ToInt64(Environment.GetEnvironmentVariable(SyncDelaySetting) ?? "300"); //defaults to 5min
        public string KumaTag { get; internal set; } = "⎈";
        public string CustomResourceTagName { get; set; } = "crd";

        public bool ValidateSettings()
        {
            if (string.IsNullOrEmpty(KumaUsername))
            {
                logger.LogInformation($"{KumaUsernameSetting} is not populated.");
                return false;
            }

            if (string.IsNullOrEmpty(KumaPassword))
            {
                logger.LogInformation($"{KumaPasswordSetting} is not populated.");
                return false;
            }
            if (string.IsNullOrEmpty(KumaHost))
            {
                logger.LogInformation($"{KumaHostSetting} is not populated.");
                return false;
            }

            return true;
        }

    }
}
