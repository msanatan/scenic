using Newtonsoft.Json;

namespace UniBridge.Editor.Commands.Test
{
    public sealed class TestQueryParams
    {
        public string Mode;
        public string Filter;
        public PaginationParams Paging;

        private const int DefaultLimit = 50;
        private const int DefaultOffset = 0;

        public static TestQueryParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var mode = NormalizeMode(payload.Value<string>("mode"));
            var filter = payload.Value<string>("filter");
            var paging = PaginationParams.From(request, defaultLimit: DefaultLimit, defaultOffset: DefaultOffset);

            return new TestQueryParams
            {
                Mode = mode,
                Filter = string.IsNullOrWhiteSpace(filter) ? null : filter,
                Paging = paging,
            };
        }

        private static string NormalizeMode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim().ToLowerInvariant();
            if (normalized == "edit" || normalized == "play")
            {
                return normalized;
            }

            throw new CommandHandlingException("params.mode must be one of: edit, play.");
        }
    }

    public sealed class TestListResult
    {
        [JsonProperty("tests")]
        public TestListItem[] Tests;

        [JsonProperty("total")]
        public int Total;

        [JsonProperty("limit")]
        public int Limit;

        [JsonProperty("offset")]
        public int Offset;
    }

    public sealed class TestRunResult
    {
        [JsonProperty("tests")]
        public TestRunItem[] Tests;

        [JsonProperty("passed")]
        public int Passed;

        [JsonProperty("failed")]
        public int Failed;

        [JsonProperty("skipped")]
        public int Skipped;

        [JsonProperty("inconclusive")]
        public int Inconclusive;

        [JsonProperty("durationMs")]
        public int DurationMs;

        [JsonProperty("total")]
        public int Total;

        [JsonProperty("limit")]
        public int Limit;

        [JsonProperty("offset")]
        public int Offset;
    }

    public sealed class TestListItem
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("fullName")]
        public string FullName;

        [JsonProperty("mode")]
        public string Mode;

        [JsonProperty("assembly")]
        public string Assembly;
    }

    public sealed class TestRunItem
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("fullName")]
        public string FullName;

        [JsonProperty("mode")]
        public string Mode;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("durationMs")]
        public int DurationMs;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("stackTrace")]
        public string StackTrace;
    }

    public sealed class TestRunSummary
    {
        public TestRunItem[] Items;
        public int Passed;
        public int Failed;
        public int Skipped;
        public int Inconclusive;
        public int DurationMs;
    }
}
