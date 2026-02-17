using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UniBridge.Editor;
using UniBridge.Editor.Commands.Scene;

namespace UniBridge.Editor.Tests.Commands.Scene
{
    [TestFixture]
    public class SceneActiveCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string TempScenePath = "Assets/__TempTests__/SceneActiveCommandHandlerTests.unity";

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
        public void Route_SceneActive_ReturnsLoadedSceneAndDirtyState()
        {
            var loaded = EditorSceneManager.OpenScene(TempScenePath, OpenSceneMode.Single);

            var cleanResponse = CommandRouter.Route(CreateRequest("scene-active-clean"), executeEnabled: true);
            Assert.IsTrue(cleanResponse.Success);
            Assert.AreEqual("scene-active-clean", cleanResponse.Id);
            var cleanResult = cleanResponse.Result as SceneActiveCommandResult;
            Assert.IsNotNull(cleanResult);
            Assert.IsNotNull(cleanResult.Scene);
            Assert.AreEqual("SceneActiveCommandHandlerTests", cleanResult.Scene.Name);
            Assert.AreEqual(TempScenePath, cleanResult.Scene.Path);
            Assert.IsFalse(cleanResult.Scene.IsDirty);

            var go = new UnityEngine.GameObject("DirtyMarker");
            EditorSceneManager.MarkSceneDirty(loaded);

            var dirtyResponse = CommandRouter.Route(CreateRequest("scene-active-dirty"), executeEnabled: true);
            Assert.IsTrue(dirtyResponse.Success);
            Assert.AreEqual("scene-active-dirty", dirtyResponse.Id);
            var dirtyResult = dirtyResponse.Result as SceneActiveCommandResult;
            Assert.IsNotNull(dirtyResult);
            Assert.IsNotNull(dirtyResult.Scene);
            Assert.AreEqual(TempScenePath, dirtyResult.Scene.Path);
            Assert.IsTrue(dirtyResult.Scene.IsDirty);

            UnityEngine.Object.DestroyImmediate(go);
        }

        private static CommandRequest CreateRequest(string id)
        {
            return new CommandRequest
            {
                Id = id,
                Command = "scene.active",
                ParamsJson = "{}",
            };
        }
    }
}
