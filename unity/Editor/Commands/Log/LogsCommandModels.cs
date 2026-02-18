using Newtonsoft.Json;

namespace UniBridge.Editor.Commands.Logs
{
    public sealed class LogsCommandParams
    {
        public string Severity;
        public int Limit;
        public int Offset;

        private const int DefaultLimit = 50;
        private const int DefaultOffset = 0;

        public static LogsCommandParams From(CommandRequest request)
        {
            var severity = request == null ? null : request.GetStringParam("severity");
            var normalizedSeverity = NormalizeSeverity(severity);

            var limitText = request == null ? null : request.GetStringParam("limit");
            var offsetText = request == null ? null : request.GetStringParam("offset");

            var limit = ParseOrDefault(limitText, DefaultLimit, "limit");
            var offset = ParseOrDefault(offsetText, DefaultOffset, "offset");

            if (limit <= 0)
            {
                throw new CommandHandlingException("params.limit must be a positive integer.");
            }

            if (offset < 0)
            {
                throw new CommandHandlingException("params.offset must be a non-negative integer.");
            }

            return new LogsCommandParams
            {
                Severity = normalizedSeverity,
                Limit = limit,
                Offset = offset,
            };
        }

        private static int ParseOrDefault(string value, int defaultValue, string label)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            if (!int.TryParse(value, out var parsed))
            {
                throw new CommandHandlingException($"params.{label} must be an integer.");
            }

            return parsed;
        }

        private static string NormalizeSeverity(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim().ToLowerInvariant();
            if (normalized == "info" || normalized == "warn" || normalized == "error")
            {
                return normalized;
            }

            throw new CommandHandlingException("params.severity must be one of: info, warn, error.");
        }
    }

    public sealed class LogsCommandResult
    {
        [JsonProperty("logs")]
        public LogEntry[] Logs;

        [JsonProperty("total")]
        public int Total;

        [JsonProperty("limit")]
        public int Limit;

        [JsonProperty("offset")]
        public int Offset;
    }

    public sealed class LogEntry
    {
        [JsonProperty("timestamp")]
        public string Timestamp;

        [JsonProperty("severity")]
        public string Severity;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("stackTrace")]
        public string StackTrace;
    }
}
