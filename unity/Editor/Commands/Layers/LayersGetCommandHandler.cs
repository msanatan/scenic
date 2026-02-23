namespace Scenic.Editor.Commands.Layers
{
    [ScenicCommand("layers.get")]
    public sealed class LayersGetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = LayersGetCommandParams.From(request);
            var all = LayerDefinitions.BuildLayerItems();
            var page = Pagination.Slice(all, parameters.Paging, out var total);

            return new LayersGetCommandResult
            {
                Layers = page,
                Total = total,
                Limit = parameters.Paging.Limit,
                Offset = parameters.Paging.Offset,
            };
        }
    }
}
