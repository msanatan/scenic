using System;
using System.Collections.Generic;
using UnityEditorInternal;

namespace UniBridge.Editor.Commands.Tags
{
    [UniBridgeCommand("tags.get")]
    public sealed class TagsGetCommandHandler : ICommandHandler
    {
        private static readonly HashSet<string> BuiltInTags = new HashSet<string>(StringComparer.Ordinal)
        {
            "Untagged",
            "Respawn",
            "Finish",
            "EditorOnly",
            "MainCamera",
            "Player",
            "GameController",
        };

        public object Handle(CommandRequest request)
        {
            var names = InternalEditorUtility.tags;
            var tags = new TagItem[names.Length];

            for (var i = 0; i < names.Length; i++)
            {
                var name = names[i] ?? string.Empty;
                tags[i] = new TagItem
                {
                    Name = name,
                    IsBuiltIn = BuiltInTags.Contains(name),
                };
            }

            return new TagsGetCommandResult
            {
                Tags = tags,
                Total = tags.Length,
            };
        }
    }
}
