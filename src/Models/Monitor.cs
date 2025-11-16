using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Text.Json.Serialization;
using UptimeKuma.Operator.Entities;

namespace UptimeKuma.Models
{
    [DebuggerDisplay("{id}:{name}")]
    public class KumaMonitor
    {
        [JsonPropertyName("id")]
        public int id { get; set; }

        [JsonPropertyName("type")]
        public string? type { get; set; }

        [JsonPropertyName("name")]
        public string? name { get; set; }

        [JsonPropertyName("active")]
        public bool active { get; set; } = true;

        [JsonPropertyName("tags")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Tag[]? tags { get; set; } = null;

        [JsonPropertyName("weight")]
        public int weight { get; set; }

        [JsonPropertyName("parent")]
        public string? parent { get; set; }

        [JsonPropertyName("url")]
        public string? url { get; set; }

        [JsonPropertyName("method")]
        public string method { get; set; } = "GET";

        [JsonPropertyName("interval")]
        public int interval { get; set; } = 60;

        [JsonPropertyName("retryInterval")]
        public int retryInterval { get; set; } = 60;

        [JsonPropertyName("resendInterval")]
        public int resendInterval { get; set; } = 60;

        [JsonPropertyName("maxretries")]
        public int maxretries { get; set; }

        [JsonPropertyName("timeout")]
        public int timeout { get; set; } = 48;

        [JsonPropertyName("notificationIDList")]
        public Dictionary<string, bool>? notificationIDList { get; set; }

        [JsonPropertyName("ignoreTls")]
        public bool ignoreTls { get; set; } = false;

        [JsonPropertyName("description")]
        public string? description { get; set; }

        [JsonPropertyName("upsideDown")]
        public bool upsideDown { get; set; } = false;

        [JsonPropertyName("packetSize")]
        public int packetSize { get; set; } = 56;

        [JsonPropertyName("expiryNotification")]
        public bool expiryNotification { get; set; }

        [JsonPropertyName("maxredirects")]
        public int maxredirects { get; set; } = 10;

        [JsonPropertyName("accepted_statuscodes")]
        public List<string> accepted_statuscodes { get; set; } = new List<string>() { "200-299" };

        [JsonPropertyName("dns_resolve_type")]
        public string dns_resolve_type { get; set; } = "A";

        [JsonPropertyName("dns_resolve_server")]
        public string dns_resolve_server { get; set; } = "1.1.1.1";

        [JsonPropertyName("docker_container")]
        public string? docker_container { get; set; }

        [JsonPropertyName("docker_host")]
        public string? docker_host { get; set; }

        [JsonPropertyName("proxyId")]
        public string? proxyId { get; set; }

        [JsonPropertyName("mqttUsername")]
        public string? mqttUsername { get; set; }

        [JsonPropertyName("mqttPassword")]
        public string? mqttPassword { get; set; }

        [JsonPropertyName("mqttTopic")]
        public string? mqttTopic { get; set; }

        [JsonPropertyName("mqttSuccessMessage")]
        public string? mqttSuccessMessage { get; set; }

        [JsonPropertyName("authMethod")]
        public string? authMethod { get; set; }

        [JsonPropertyName("oauth_auth_method")]
        public string oauth_auth_method { get; set; } = "client_secret_basic";

        [JsonPropertyName("httpBodyEncoding")]
        public string httpBodyEncoding { get; set; } = "json";

        [JsonPropertyName("headers")]
        public string? headers { get; set; }

        [JsonPropertyName("jsonPath")]
        public string? jsonPath { get; set; }

        [JsonPropertyName("kafkaProducerBrokers")]
        public List<string>? kafkaProducerBrokers { get; set; }

        [JsonPropertyName("kafkaProducerSaslOptions")]
        public KafkaProducerSaslOptions? kafkaProducerSaslOptions { get; set; }

        [JsonPropertyName("kafkaProducerSsl")]
        public bool kafkaProducerSsl { get; set; } = false;

        [JsonPropertyName("gamedigGivenPortOnly")]
        public bool gamedigGivenPortOnly { get; set; } = true;
    }

    public class KafkaProducerSaslOptions
    {
        [JsonPropertyName("mechanism")]
        public string mechanism { get; set; } = "none";
    }

    public class Tag
    {
        [JsonPropertyName("id")]
        public long id { get; set; }

        [JsonPropertyName("monitorId")]
        public long monitorId { get; set; }

        [JsonPropertyName("tagId")]
        public long tagId { get; set; }

        [JsonPropertyName("value")]
        public string? value { get; set; }

        [JsonPropertyName("name")]
        public string? name { get; set; }

        [JsonPropertyName("color")]
        public string? color { get; set; }
    }
}