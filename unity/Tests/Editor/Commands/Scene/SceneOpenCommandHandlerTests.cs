using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UniBridge.Editor;
using UniBridge.Editor.Commands.Scene;

namespace UniBridge.Editor.Tests.Commands.Scene
{
    [TestFixture]
    public class SceneOpenCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string TempScenePath = "Assets/__TempTests__/SceneOpenCommandHandlerTests.unity";

        [SetUp]
        public void SetUp()
        {
            if (!AssetDatabase.IsValidFolder(TempDirectoryPath))
            {
                AssetDatabase.CreateFolder("Assets", "__TempTests__");
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, TempScenePath);
            AssetDatabase.Refresh();
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
        public void Route_SceneOpen_ReturnsOpenedSceneInfo()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var response = CommandRouter.Route(CreateRequest("scene-open-valid", TempScenePath), executeEnabled: true);
            Assert.IsTrue(response.Success);
            Assert.AreEqual("scene-open-valid", response.Id);
            var result = response.Result as SceneOpenCommandResult;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Scene);
            Assert.AreEqual("SceneOpenCommandHandlerTests", result.Scene.Name);
            Assert.AreEqual(TempScenePath, result.Scene.Path);
            Assert.IsFalse(result.Scene.IsDirty);
        }

        [Test]
        public void Route_SceneOpen_MissingPath_ReturnsError()
        {
            var response = CommandRouter.Route(CreateRequest("scene-open-missing", ""), executeEnabled: true);
            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("path", response.Error.ToLower());
        }

        [Test]
        public void Route_SceneOpen_NonexistentPath_ReturnsError()
        {
            var response = CommandRouter.Route(CreateRequest("scene-open-notfound", "Assets/DoesNotExist.unity"), executeEnabled: true);
            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("not found", response.Error.ToLower());
        }

        private static CommandRequest CreateRequest(string id, string path)
        {
            return new CommandRequest
            {
                Id = id,
                Command = "scene.open",
                ParamsJson = $"{{\"path\":\"{path.Replace("\\", "\\\\")}\"}}",
            };
        }
    }
}
