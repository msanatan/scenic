using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace UniBridge.Editor
{
    [Serializable]
    public class CommandRequest
    {
        public string Id = string.Empty;
        public string Command = string.Empty;
        public string ParamsJson = "{}";

        public string ToJson()
        {
            return "{" +
                   "\"id\":" + JsonCompat.Quote(Id) + "," +
                   "\"command\":" + JsonCompat.Quote(Command) + "," +
                   "\"params\":" + (string.IsNullOrWhiteSpace(ParamsJson) ? "{}" : ParamsJson) +
                   "}";
        }

        public static bool TryParse(string json, out CommandRequest request)
        {
            request = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            var id = JsonCompat.ExtractString(json, "id") ?? string.Empty;
            var command = JsonCompat.ExtractString(json, "command") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            var paramsObject = JsonCompat.ExtractObject(json, "params") ?? "{}";
            request = new CommandRequest
            {
                Id = id,
                Command = command,
                ParamsJson = paramsObject,
            };

            return true;
        }

        public string GetStringParam(string key)
        {
            return JsonCompat.ExtractString(ParamsJson, key);
        }

        public string[] GetStringArrayParam(string key)
        {
            return JsonCompat.ExtractStringArray(ParamsJson, key);
        }
    }

    [Serializable]
    public class CommandResponse
    {
        public string Id = string.Empty;
        public bool Success;
        public string Result;
        public string Error;

        public static CommandResponse Ok(string id, object result)
        {
            return new CommandResponse
            {
                Id = id,
                Success = true,
                Result = result == null ? null : Convert.ToString(result, CultureInfo.InvariantCulture),
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
            return "{" +
                   "\"id\":" + JsonCompat.Quote(Id) + "," +
                   "\"success\":" + (Success ? "true" : "false") + "," +
                   "\"result\":" + JsonCompat.QuoteOrNull(Result) + "," +
                   "\"error\":" + JsonCompat.QuoteOrNull(Error) +
                   "}";
        }
    }

    internal static class JsonCompat
    {
        private static readonly Regex StringPattern = new Regex("\\\"{0}\\\"\\s*:\\s*\\\"(?<v>(?:\\\\.|[^\\\"])*)\\\"", RegexOptions.Compiled);

        public static string ExtractString(string json, string key)
        {
            var pattern = string.Format(CultureInfo.InvariantCulture, StringPattern.ToString(), Regex.Escape(key));
            var match = Regex.Match(json, pattern, RegexOptions.Singleline);
            if (!match.Success)
            {
                return null;
            }

            return Unescape(match.Groups["v"].Value);
        }

        public static string[] ExtractStringArray(string json, string key)
        {
            var arrayPattern = "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*\\[(?<arr>.*?)\\]";
            var arrayMatch = Regex.Match(json, arrayPattern, RegexOptions.Singleline);
            if (!arrayMatch.Success)
            {
                return Array.Empty<string>();
            }

            var values = new List<string>();
            var itemMatches = Regex.Matches(arrayMatch.Groups["arr"].Value, "\\\"(?<v>(?:\\\\.|[^\\\"])*)\\\"");
            foreach (Match item in itemMatches)
            {
                values.Add(Unescape(item.Groups["v"].Value));
            }

            return values.ToArray();
        }

        public static string ExtractObject(string json, string key)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var keyPattern = "\"" + key + "\"";
            var keyIndex = json.IndexOf(keyPattern, StringComparison.Ordinal);
            if (keyIndex < 0)
            {
                return null;
            }

            var colonIndex = json.IndexOf(':', keyIndex + keyPattern.Length);
            if (colonIndex < 0)
            {
                return null;
            }

            var start = -1;
            for (var i = colonIndex + 1; i < json.Length; i++)
            {
                var c = json[i];
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                start = c == '{' ? i : -1;
                break;
            }

            if (start < 0)
            {
                return null;
            }

            var depth = 0;
            var inString = false;
            var escaped = false;
            for (var i = start; i < json.Length; i++)
            {
                var c = json[i];
                if (inString)
                {
                    if (escaped)
                    {
                        escaped = false;
                    }
                    else if (c == '\\')
                    {
                        escaped = true;
                    }
                    else if (c == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (c == '"')
                {
                    inString = true;
                    continue;
                }

                if (c == '{')
                {
                    depth++;
                }
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return json.Substring(start, i - start + 1);
                    }
                }
            }

            return null;
        }

        public static string QuoteOrNull(string value)
        {
            return value == null ? "null" : Quote(value);
        }

        public static string Quote(string value)
        {
            if (value == null)
            {
                return "null";
            }

            return "\"" + Escape(value) + "\"";
        }

        public static string Escape(string value)
        {
            var sb = new StringBuilder(value.Length + 8);
            foreach (var c in value)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default: sb.Append(c); break;
                }
            }

            return sb.ToString();
        }

        public static string Unescape(string value)
        {
            if (value == null)
            {
                return null;
            }

            return value
                .Replace("\\\"", "\"")
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t")
                .Replace("\\\\", "\\");
        }
    }
}
