using Newtonsoft.Json;
using UniBridge.Editor.Commands;

namespace UniBridge.Editor.Commands.Logs
{
    public sealed class LogsCommandParams
    {
        public string Severity;
        public PaginationParams Paging;

        private const int DefaultLimit = 50;
        private const int DefaultOffset = 0;

        public static LogsCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var severity = CommandModelHelpers.ReadOptionalString(payload, "severity");
            var normalizedSeverity = NormalizeSeverity(severity);
            var paging = PaginationParams.From(payload, defaultLimit: DefaultLimit, defaultOffset: DefaultOffset);

            return new LogsCommandParams
            {
                Severity = normalizedSeverity,
                Paging = paging,
            };
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
