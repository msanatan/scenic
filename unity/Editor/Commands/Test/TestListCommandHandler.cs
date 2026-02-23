namespace Scenic.Editor.Commands.Test
{
    [ScenicCommand("test.list")]
    public sealed class TestListCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var query = TestQueryParams.From(request);
            var page = TestService.ListTests(query, out var total);

            return new TestListResult
            {
                Tests = page,
                Total = total,
                Limit = query.Paging.Limit,
                Offset = query.Paging.Offset,
            };
        }
    }
}
