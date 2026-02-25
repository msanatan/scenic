namespace Scenic.Editor.Commands.Packages
{
    [ScenicCommand("packages.get")]
    public sealed class PackagesGetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = PackagesGetCommandParams.From(request);
            var packages = PackagesClient.List(parameters.IncludeIndirect);
            var items = PackagesClient.BuildItems(packages, parameters.Search);
            var page = Pagination.Slice(items, parameters.Paging, out var total);

            return new PackagesGetCommandResult
            {
                Packages = page,
                Total = total,
                Limit = parameters.Paging.Limit,
                Offset = parameters.Paging.Offset,
            };
        }
    }
}
