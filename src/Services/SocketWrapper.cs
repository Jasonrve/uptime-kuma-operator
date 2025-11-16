using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Nodes;
using SocketIO.Serializer.SystemTextJson;
using SocketIOClient;
using YamlDotNet.Serialization;

namespace UptimeKuma.Services
{
    public class SocketWrapper : ISocketWrapperSF, ISocketWrapperExec
    {
        public static ILogger<SocketWrapper>? logger;

        private SocketIOClient.SocketIO client;
        private JsonObject? responseJsonObject;
        private bool socketResponseSuccess = false;

        public SocketWrapper(SocketIOClient.SocketIO client)
        {
            this.client = client;
            client.Serializer = new SystemTextJsonSerializer(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public SocketWrapper Success(Action<JsonObject> action)
        {
            if (socketResponseSuccess && responseJsonObject != null)
                action(responseJsonObject);
            return this;
        }

        public SocketWrapper Failure(Action<JsonObject> action)
        {
            if (!socketResponseSuccess && responseJsonObject != null)
                action(responseJsonObject);
            return this;
        }

        public T GetValue<T>(string key)
        { 
            if (responseJsonObject == null)
            {
                return default!;
            }

            return responseJsonObject.GetValue<T>(key);
        }

        public string GetValueString(string key)
        {
            return responseJsonObject?.GetValueString(key) ?? string.Empty;
        }

        public bool HandleResponse(SocketIOResponse response)
        {
            // Parse the JSON string into a JsonNode
            JsonNode? jsonNode = JsonNode.Parse(response.ToString());

            // Check if the JsonNode represents an array and contains at least one element
            if (jsonNode != null && jsonNode is JsonArray jsonArray && jsonArray.Count > 0)
            {
                if (jsonArray != null && jsonArray.Count > 0)
                {
                    // Check if the first element has the "ok" property set to true
                    var firstElement = jsonArray[0];
                    if (firstElement is JsonObject jsonObject)
                    {
                        responseJsonObject = jsonObject;
                        if (firstElement != null && jsonObject.ContainsKey("ok"))
                        {
                            if (jsonObject["ok"] is JsonNode okNode && okNode is JsonValue okValue && okValue.TryGetValue<bool>(out var ok) && ok)
                            {
                                logger?.LogDebug("response is ok");
                                return true;
                            }
                            else
                            {
                                logger?.LogError("response is not ok");
                            }
                        }
                        else
                        {
                            logger?.LogError("response is unkown");
                        }
                    }


                }
            }
            else
            {
                logger?.LogError("unkown response");
            }

            return false;
        }

        public static Task WaitForCancellationAsync(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s!).TrySetResult(true), tcs);
            return tcs.Task;
        }

        public async Task<SocketWrapper> Exec(string eventName, params object[] values)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;
            SocketIOResponse? socketIOResponse = null;

            await client.EmitAsync(eventName, (response) =>
            {
                socketIOResponse = response;
                socketResponseSuccess = HandleResponse(response);
                cts.Cancel();

            }, values);

            await Task.WhenAny(Task.Delay(TimeSpan.FromSeconds(5)), WaitForCancellationAsync(cts.Token));
            return this;
        }
    }

    public interface ISocketWrapperSF
    {
        SocketWrapper Success(Action<JsonObject> action);

        SocketWrapper Failure(Action<JsonObject> action);
    }

    public interface ISocketWrapperExec
    {
        Task<SocketWrapper> Exec(string eventName, params object[] values);
    }

    public static class SocketWrapperExtensions
    {
        public static Task<SocketWrapper> Exec(this SocketIOClient.SocketIO client, string eventName, params object[] parms)
        {
            return new SocketWrapper(client).Exec(eventName, parms);
        }

        public static SocketWrapper Handle(this SocketIOClient.SocketIO client)
        {
            return new SocketWrapper(client);
        }

        public async static Task<SocketWrapper> Success(this Task<SocketWrapper> socketWrapper, Action<JsonObject?> action)
        {
            return (await socketWrapper).Success(action);
        }

        public async static Task<SocketWrapper> Failure(this Task<SocketWrapper> socketWrapper, Action<JsonObject?> action)
        {
            return (await socketWrapper).Failure(action);
        }

        public async static Task<T> GetValue<T>(this Task<SocketWrapper> socketWrapper, string key)
        {
            return (await socketWrapper).GetValue<T>(key);
        }

        public async static Task<string> GetValueString(this Task<SocketWrapper?> socketWrapper, string key)
        {
            return (await socketWrapper).GetValueString(key);
        }

        public static T GetValue<T>(this JsonObject? responseJsonObject, string key)
        {
            if (responseJsonObject.ContainsKey(key))
            {
                var result = JsonSerializer.Deserialize<T>(responseJsonObject[key]);

                return result;
            }

            return default;
        }

        public static string GetValueString(this JsonObject? responseJsonObject, string key)
        {
            if (responseJsonObject != null && responseJsonObject.ContainsKey(key))
            {
                return responseJsonObject[key].ToString();
            }

            return string.Empty;
        }

    }
}