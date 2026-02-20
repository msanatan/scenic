using UnityEditorInternal;

namespace UniBridge.Editor.Commands.Tags
{
    [UniBridgeCommand("tags.get")]
    public sealed class TagsGetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var names = InternalEditorUtility.tags;
            var tags = new TagItem[names.Length];

            for (var i = 0; i < names.Length; i++)
            {
                tags[i] = TagDefinitions.ToTagItem(names[i]);
            }

            return new TagsGetCommandResult
            {
                Tags = tags,
                Total = tags.Length,
            };
        }
    }
}
