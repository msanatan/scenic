using System;
using UnityEditor;
using UnityEditorInternal;

namespace Scenic.Editor.Commands.Tags
{
    [ScenicCommand("tags.remove")]
    public sealed class TagsRemoveCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = TagsRemoveCommandParams.From(request);
            var name = parameters.Name;

            if (TagDefinitions.IsBuiltIn(name))
            {
                throw new CommandHandlingException($"Cannot remove built-in tag: {name}");
            }

            var existingTags = InternalEditorUtility.tags;
            if (!ContainsTag(existingTags, name))
            {
                return new TagsRemoveCommandResult
                {
                    Tag = TagDefinitions.ToTagItem(name),
                    Removed = false,
                    Total = existingTags.Length,
                };
            }

            var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (asset == null || asset.Length == 0 || asset[0] == null)
            {
                throw new CommandHandlingException("Unable to load ProjectSettings/TagManager.asset.");
            }

            var serialized = new SerializedObject(asset[0]);
            var tagsProperty = serialized.FindProperty("tags");
            if (tagsProperty == null || !tagsProperty.isArray)
            {
                throw new CommandHandlingException("Unable to resolve tags property in TagManager.");
            }

            for (var i = tagsProperty.arraySize - 1; i >= 0; i--)
            {
                var item = tagsProperty.GetArrayElementAtIndex(i);
                if (string.Equals(item.stringValue, name, StringComparison.Ordinal))
                {
                    tagsProperty.DeleteArrayElementAtIndex(i);
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();

            var updatedTags = InternalEditorUtility.tags;

            return new TagsRemoveCommandResult
            {
                Tag = TagDefinitions.ToTagItem(name),
                Removed = true,
                Total = updatedTags.Length,
            };
        }

        private static bool ContainsTag(string[] tags, string name)
        {
            if (tags == null)
            {
                return false;
            }

            for (var i = 0; i < tags.Length; i++)
            {
                if (string.Equals(tags[i], name, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
