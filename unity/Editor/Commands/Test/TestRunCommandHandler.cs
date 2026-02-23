namespace Scenic.Editor.Commands.Test
{
    [ScenicCommand("test.run")]
    public sealed class TestRunCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var query = TestQueryParams.From(request);
            var summary = TestService.RunTests(query);
            var page = Pagination.Slice(summary.Items, query.Paging, out var total);

            return new TestRunResult
            {
                Tests = page,
                Passed = summary.Passed,
                Failed = summary.Failed,
                Skipped = summary.Skipped,
                Inconclusive = summary.Inconclusive,
                DurationMs = summary.DurationMs,
                Total = total,
                Limit = query.Paging.Limit,
                Offset = query.Paging.Offset,
            };
        }
    }
}
