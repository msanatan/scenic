using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditorInternal;
using UniBridge.Editor;
using UniBridge.Editor.Commands.Tags;

namespace UniBridge.Editor.Tests.Commands.Tags
{
    [TestFixture]
    public class TagsGetCommandHandlerTests
    {
        private readonly List<string> _createdTags = new List<string>();

        [TearDown]
        public void TearDown()
        {
            for (var i = 0; i < _createdTags.Count; i++)
            {
                RemoveTag(_createdTags[i]);
            }
            _createdTags.Clear();
        }

        [Test]
        public void Route_TagsGet_ReturnsTagsWithBuiltInMarkers()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "tags-get",
                    Command = "tags.get",
                    ParamsJson = "{}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            Assert.AreEqual("tags-get", response.Id);

            var result = response.Result as TagsGetCommandResult;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Tags);
            Assert.AreEqual(result.Total, result.Tags.Length);
            Assert.Greater(result.Total, 0);

            var untagged = result.Tags.FirstOrDefault(tag => tag.Name == "Untagged");
            Assert.IsNotNull(untagged);
            Assert.IsTrue(untagged.IsBuiltIn);
        }

        [Test]
        public void Route_TagsAdd_AddsTagAndReturnsAddedTrue()
        {
            var tagName = $"UnibridgeTag_{System.Guid.NewGuid():N}";
            _createdTags.Add(tagName);

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "tags-add",
                    Command = "tags.add",
                    ParamsJson = $"{{\"name\":\"{tagName}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as TagsAddCommandResult;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Added);
            Assert.AreEqual(tagName, result.Tag.Name);
            Assert.IsFalse(result.Tag.IsBuiltIn);
            Assert.Greater(result.Total, 0);
            Assert.IsTrue(InternalEditorUtility.tags.Contains(tagName));
        }

        [Test]
        public void Route_TagsAdd_WhenTagExists_ReturnsAddedFalse()
        {
            var tagName = $"UnibridgeTag_{System.Guid.NewGuid():N}";
            AddTag(tagName);
            _createdTags.Add(tagName);

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "tags-add-existing",
                    Command = "tags.add",
                    ParamsJson = $"{{\"name\":\"{tagName}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as TagsAddCommandResult;
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Added);
            Assert.AreEqual(tagName, result.Tag.Name);
        }

        private static void AddTag(string name)
        {
            if (InternalEditorUtility.tags.Contains(name))
            {
                return;
            }

            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            var serialized = new SerializedObject(assets[0]);
            var tags = serialized.FindProperty("tags");
            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = name;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        private static void RemoveTag(string name)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0 || assets[0] == null)
            {
                return;
            }

            var serialized = new SerializedObject(assets[0]);
            var tags = serialized.FindProperty("tags");
            if (tags == null || !tags.isArray)
            {
                return;
            }

            for (var i = tags.arraySize - 1; i >= 0; i--)
            {
                var value = tags.GetArrayElementAtIndex(i).stringValue;
                if (value == name)
                {
                    tags.DeleteArrayElementAtIndex(i);
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }
    }
}
