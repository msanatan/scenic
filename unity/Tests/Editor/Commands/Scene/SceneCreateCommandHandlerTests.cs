using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using Scenic.Editor;
using Scenic.Editor.Commands.Scene;

namespace Scenic.Editor.Tests.Commands.Scene
{
    [TestFixture]
    public class SceneCreateCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string TempScenePath = "Assets/__TempTests__/SceneCreateCommandHandlerTests.unity";

        [SetUp]
        public void SetUp()
        {
            if (!AssetDatabase.IsValidFolder(TempDirectoryPath))
            {
                AssetDatabase.CreateFolder("Assets", "__TempTests__");
            }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [TearDown]
        public void TearDown()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (File.Exists(TempScenePath))
            {
                AssetDatabase.DeleteAsset(TempScenePath);
            }

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

            AssetDatabase.Refresh();
        }

        [Test]
        public void Route_SceneCreate_ReturnsCreatedSceneInfo()
        {
            var response = CommandRouter.Route(CreateRequest("scene-create-valid", TempScenePath), executeEnabled: true);
            Assert.IsTrue(response.Success);
            Assert.AreEqual("scene-create-valid", response.Id);
            var result = response.Result as SceneCreateCommandResult;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Scene);
            Assert.AreEqual("SceneCreateCommandHandlerTests", result.Scene.Name);
            Assert.AreEqual(TempScenePath, result.Scene.Path);
            Assert.IsFalse(result.Scene.IsDirty);
            Assert.IsTrue(File.Exists(TempScenePath));
        }

        [Test]
        public void Route_SceneCreate_MissingPath_ReturnsError()
        {
            var response = CommandRouter.Route(CreateRequest("scene-create-missing", ""), executeEnabled: true);
            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("path", response.Error.ToLower());
        }

        [Test]
        public void Route_SceneCreate_AlreadyExists_ReturnsError()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, TempScenePath);

            var response = CommandRouter.Route(CreateRequest("scene-create-exists", TempScenePath), executeEnabled: true);
            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("already exists", response.Error.ToLower());
        }

        private static CommandRequest CreateRequest(string id, string path)
        {
            return new CommandRequest
            {
                Id = id,
                Command = "scene.create",
                ParamsJson = $"{{\"path\":\"{path.Replace("\\", "\\\\")}\"}}",
            };
        }
    }
}
