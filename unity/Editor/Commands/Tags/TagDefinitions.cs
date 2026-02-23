using System;
using System.Collections.Generic;

namespace Scenic.Editor.Commands.Tags
{
    internal static class TagDefinitions
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

        public static bool IsBuiltIn(string name)
        {
            return BuiltInTags.Contains(name ?? string.Empty);
        }

        public static TagItem ToTagItem(string name)
        {
            var normalized = name ?? string.Empty;
            return new TagItem
            {
                Name = normalized,
                IsBuiltIn = IsBuiltIn(normalized),
            };
        }
    }
}
