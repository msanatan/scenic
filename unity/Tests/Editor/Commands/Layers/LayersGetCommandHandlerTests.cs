using NUnit.Framework;
using UnityEditor;
using Scenic.Editor;
using Scenic.Editor.Commands.Layers;

namespace Scenic.Editor.Tests.Commands.Layers
{
    [TestFixture]
    public class LayersGetCommandHandlerTests
    {
        private string _createdLayerName;

        [TearDown]
        public void TearDown()
        {
            if (!string.IsNullOrWhiteSpace(_createdLayerName))
            {
                RemoveLayerByName(_createdLayerName);
                _createdLayerName = null;
            }
        }

        [Test]
        public void Route_LayersGet_ReturnsPaginatedLayerSlots()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "layers-get",
                    Command = "layers.get",
                    ParamsJson = "{\"limit\":10,\"offset\":0}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as LayersGetCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.Limit);
            Assert.AreEqual(0, result.Offset);
            Assert.AreEqual(32, result.Total);
            Assert.AreEqual(10, result.Layers.Length);

            var first = result.Layers[0];
            Assert.AreEqual(0, first.Index);
            Assert.IsTrue(first.IsBuiltIn);
            Assert.IsFalse(first.IsUserEditable);
            Assert.IsTrue(first.IsOccupied);
        }

        [Test]
        public void Route_LayersAdd_AddsLayerAndReturnsAddedTrue()
        {
            var name = $"ScenicLayer_{System.Guid.NewGuid():N}";
            _createdLayerName = name;

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "layers-add",
                    Command = "layers.add",
                    ParamsJson = $"{{\"name\":\"{name}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as LayersAddCommandResult;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Added);
            Assert.AreEqual(name, result.Layer.Name);
            Assert.IsTrue(result.Layer.IsUserEditable);
            Assert.IsTrue(result.Layer.IsOccupied);
            Assert.AreEqual(32, result.Total);
        }

        [Test]
        public void Route_LayersAdd_WhenLayerExists_ReturnsAddedFalse()
        {
            var name = $"ScenicLayer_{System.Guid.NewGuid():N}";
            AddLayer(name);
            _createdLayerName = name;

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "layers-add-existing",
                    Command = "layers.add",
                    ParamsJson = $"{{\"name\":\"{name}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as LayersAddCommandResult;
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Added);
            Assert.AreEqual(name, result.Layer.Name);
        }

        [Test]
        public void Route_LayersRemove_RemovesLayerAndReturnsRemovedTrue()
        {
            var name = $"ScenicLayer_{System.Guid.NewGuid():N}";
            AddLayer(name);

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "layers-remove",
                    Command = "layers.remove",
                    ParamsJson = $"{{\"name\":\"{name}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as LayersRemoveCommandResult;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Removed);
            Assert.AreEqual(name, result.Layer.Name);
            Assert.IsFalse(HasLayer(name));
        }

        [Test]
        public void Route_LayersRemove_WhenMissing_ReturnsRemovedFalse()
        {
            var name = $"ScenicLayer_{System.Guid.NewGuid():N}";
            RemoveLayerByName(name);

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "layers-remove-missing",
                    Command = "layers.remove",
                    ParamsJson = $"{{\"name\":\"{name}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as LayersRemoveCommandResult;
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Removed);
            Assert.AreEqual(name, result.Layer.Name);
        }

        [Test]
        public void Route_LayersRemove_BuiltInLayer_ReturnsError()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "layers-remove-built-in",
                    Command = "layers.remove",
                    ParamsJson = "{\"name\":\"Default\"}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("built-in", response.Error.ToLowerInvariant());
        }

        private static void AddLayer(string name)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            var serialized = new SerializedObject(assets[0]);
            var layers = serialized.FindProperty("layers");
            for (var i = 8; i <= 31; i++)
            {
                var item = layers.GetArrayElementAtIndex(i);
                if (item.stringValue == name)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(item.stringValue))
                {
                    item.stringValue = name;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    AssetDatabase.SaveAssets();
                    return;
                }
            }
        }

        private static void RemoveLayerByName(string name)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0 || assets[0] == null)
            {
                return;
            }

            var serialized = new SerializedObject(assets[0]);
            var layers = serialized.FindProperty("layers");
            if (layers == null || !layers.isArray)
            {
                return;
            }

            for (var i = 31; i >= 8; i--)
            {
                var item = layers.GetArrayElementAtIndex(i);
                if (item.stringValue == name)
                {
                    item.stringValue = string.Empty;
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        private static bool HasLayer(string name)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0 || assets[0] == null)
            {
                return false;
            }

            var serialized = new SerializedObject(assets[0]);
            var layers = serialized.FindProperty("layers");
            if (layers == null || !layers.isArray)
            {
                return false;
            }

            for (var i = 0; i < layers.arraySize; i++)
            {
                var item = layers.GetArrayElementAtIndex(i);
                if (item.stringValue == name)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
