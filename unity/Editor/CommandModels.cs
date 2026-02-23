using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scenic.Editor
{
    [Serializable]
    public class CommandRequest
    {
        public string Id = string.Empty;
        public string Command = string.Empty;
        public string ParamsJson = "{}";

        public string ToJson()
        {
            var payload = new JObject
            {
                ["id"] = Id ?? string.Empty,
                ["command"] = Command ?? string.Empty,
                ["params"] = ParseParamsObject(ParamsJson),
            };

            return payload.ToString(Formatting.None);
        }

        public static bool TryParse(string json, out CommandRequest request)
        {
            request = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                var payload = JObject.Parse(json);
                var id = payload.Value<string>("id") ?? string.Empty;
                var command = payload.Value<string>("command") ?? string.Empty;
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(command))
                {
                    return false;
                }

                var parameters = payload["params"] as JObject ?? new JObject();
                request = new CommandRequest
                {
                    Id = id,
                    Command = command,
                    ParamsJson = parameters.ToString(Formatting.None),
                };

                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetStringParam(string key)
        {
            var token = ParseParamsObject(ParamsJson)[key];
            if (token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            return token.Type == JTokenType.String ? token.Value<string>() : token.ToString(Formatting.None);
        }

        public string[] GetStringArrayParam(string key)
        {
            var token = ParseParamsObject(ParamsJson)[key] as JArray;
            if (token == null)
            {
                return Array.Empty<string>();
            }

            var result = new string[token.Count];
            for (var i = 0; i < token.Count; i++)
            {
                var item = token[i];
                result[i] = item == null || item.Type == JTokenType.Null
                    ? string.Empty
                    : item.Type == JTokenType.String
                        ? item.Value<string>()
                        : item.ToString(Formatting.None);
            }

            return result;
        }

        private static JObject ParseParamsObject(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new JObject();
            }

            try
            {
                var token = JToken.Parse(json);
                return token as JObject ?? new JObject();
            }
            catch
            {
                return new JObject();
            }
        }
    }

    [Serializable]
    public class CommandResponse
    {
        [JsonProperty("id")]
        public string Id = string.Empty;

        [JsonProperty("success")]
        public bool Success;

        [JsonProperty("result")]
        public object Result;

        [JsonProperty("error")]
        public string Error;

        public static CommandResponse Ok(string id, object result)
        {
            return new CommandResponse
            {
                Id = id,
                Success = true,
                Result = result,
                Error = null,
            };
        }

        public static CommandResponse Fail(string id, string error)
        {
            return new CommandResponse
            {
                Id = id,
                Success = false,
                Result = null,
                Error = error,
            };
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static bool TryParse(string json, out CommandResponse response)
        {
            response = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                var parsed = JsonConvert.DeserializeObject<CommandResponse>(json);
                if (parsed == null)
                {
                    return false;
                }

                parsed.Id = parsed.Id ?? string.Empty;
                response = parsed;
                return !string.IsNullOrWhiteSpace(response.Id);
            }
            catch
            {
                return false;
            }
        }
    }
}
