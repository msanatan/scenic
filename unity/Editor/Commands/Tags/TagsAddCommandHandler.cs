using System;
using UnityEditor;
using UnityEditorInternal;

namespace UniBridge.Editor.Commands.Tags
{
    [UniBridgeCommand("tags.add")]
    public sealed class TagsAddCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = TagsAddCommandParams.From(request);
            ValidateName(parameters.Name);

            var existingTags = InternalEditorUtility.tags;
            if (ContainsTag(existingTags, parameters.Name))
            {
                return new TagsAddCommandResult
                {
                    Tag = TagDefinitions.ToTagItem(parameters.Name),
                    Added = false,
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

            tagsProperty.InsertArrayElementAtIndex(tagsProperty.arraySize);
            var addedProperty = tagsProperty.GetArrayElementAtIndex(tagsProperty.arraySize - 1);
            addedProperty.stringValue = parameters.Name;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();

            var updatedTags = InternalEditorUtility.tags;

            return new TagsAddCommandResult
            {
                Tag = TagDefinitions.ToTagItem(parameters.Name),
                Added = true,
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

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new CommandHandlingException("params.name is required.");
            }

            if (name.IndexOf(',') >= 0)
            {
                throw new CommandHandlingException("params.name cannot contain ','.");
            }
        }
    }
}
