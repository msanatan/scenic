using System.IO;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Material;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Scenic.Editor.Tests.Commands.Material
{
    [TestFixture]
    public class MaterialAssignCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string MaterialPath = "Assets/__TempTests__/MaterialAssignCommandHandlerTests.mat";

        [SetUp]
        public void SetUp()
        {
            if (!AssetDatabase.IsValidFolder(TempDirectoryPath))
            {
                AssetDatabase.CreateFolder("Assets", "__TempTests__");
            }

            AssetDatabase.DeleteAsset(MaterialPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [TearDown]
        public void TearDown()
        {
            var existing = UnityEngine.GameObject.Find("MaterialAssignTarget");
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            AssetDatabase.DeleteAsset(MaterialPath);

            var directoryMetaPath = $"{TempDirectoryPath}.meta";
            var hasAnyTempAssets = Directory.Exists(TempDirectoryPath)
                && Directory.GetFiles(TempDirectoryPath).Length > 0;
            if (!hasAnyTempAssets && AssetDatabase.IsValidFolder(TempDirectoryPath))
            {
                AssetDatabase.DeleteAsset(TempDirectoryPath);
                if (File.Exists(directoryMetaPath))
                {
                    File.Delete(directoryMetaPath);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [Test]
        public void Route_MaterialAssign_SuccessPath()
        {
            var shader = Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("HDRP/Lit");
            Assert.IsNotNull(shader, "Expected at least one default shader to exist for test setup.");
            var source = new UnityEngine.Material(shader)
            {
                name = "MaterialAssignCommandHandlerTests",
            };
            AssetDatabase.CreateAsset(source, MaterialPath);
            AssetDatabase.SaveAssets();

            var target = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = "MaterialAssignTarget";

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-material-assign",
                    Command = "material.assign",
                    ParamsJson = $"{{\"path\":\"/MaterialAssignTarget\",\"assetPath\":\"{MaterialPath}\",\"slot\":0}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            Assert.AreEqual("test-material-assign", response.Id);

            var result = response.Result as MaterialAssignCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("/MaterialAssignTarget", result.TargetPath);
            Assert.AreEqual(target.GetInstanceID(), result.TargetInstanceId);
            Assert.AreEqual(0, result.Slot);
            Assert.AreEqual(0, result.RendererIndex);
            Assert.AreEqual(MaterialPath, result.Material.AssetPath);

            var renderer = target.GetComponent<Renderer>();
            Assert.IsNotNull(renderer);
            Assert.IsNotNull(renderer.sharedMaterials);
            Assert.IsTrue(renderer.sharedMaterials.Length > 0);
            Assert.IsNotNull(renderer.sharedMaterials[0]);
            Assert.AreEqual(MaterialPath, AssetDatabase.GetAssetPath(renderer.sharedMaterials[0]));
        }

        [Test]
        public void Route_MaterialAssign_ValidationError()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-material-assign-err",
                    Command = "material.assign",
                    ParamsJson = "{}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("path", response.Error.ToLower());
        }
    }
}
