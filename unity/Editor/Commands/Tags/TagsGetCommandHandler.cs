using UnityEditorInternal;
using UniBridge.Editor.Commands;

namespace UniBridge.Editor.Commands.Tags
{
    [UniBridgeCommand("tags.get")]
    public sealed class TagsGetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = TagsGetCommandParams.From(request);
            var names = InternalEditorUtility.tags;
            var tags = new TagItem[names.Length];

            for (var i = 0; i < names.Length; i++)
            {
                tags[i] = TagDefinitions.ToTagItem(names[i]);
            }

            var page = Pagination.Slice(tags, parameters.Paging, out var total);

            return new TagsGetCommandResult
            {
                Tags = page,
                Total = total,
                Limit = parameters.Paging.Limit,
                Offset = parameters.Paging.Offset,
            };
        }
    }
}
