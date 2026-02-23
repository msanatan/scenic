using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Scenic.Editor;
using Scenic.Editor.Commands.Prefab;

namespace Scenic.Editor.Tests.Commands.Prefab
{
    [TestFixture]
    public class PrefabCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string TempPrefabPath = "Assets/__TempTests__/PrefabCommandHandlerTests.prefab";

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

            if (File.Exists(TempPrefabPath))
            {
                AssetDatabase.DeleteAsset(TempPrefabPath);
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
        public void Route_PrefabInstantiate_InstantiatesWithParentAndTransform()
        {
            var prefabSource = new UnityEngine.GameObject("PrefabSource");
            PrefabUtility.SaveAsPrefabAsset(prefabSource, TempPrefabPath);
            UnityEngine.Object.DestroyImmediate(prefabSource);

            var parent = new UnityEngine.GameObject("Parent");

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "prefab-instantiate",
                    Command = "prefab.instantiate",
                    ParamsJson = $"{{\"prefabPath\":\"{TempPrefabPath}\",\"parentInstanceId\":{parent.GetInstanceID()},\"transform\":{{\"space\":\"local\",\"position\":{{\"x\":1,\"y\":2,\"z\":3}}}}}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as PrefabInstantiateCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(TempPrefabPath, result.PrefabPath);
            Assert.AreEqual("/Parent/PrefabCommandHandlerTests", result.Path);
            Assert.AreEqual(1f, result.Transform.Position.X);
            Assert.AreEqual(2f, result.Transform.Position.Y);
            Assert.AreEqual(3f, result.Transform.Position.Z);

            var instance = UnityEngine.GameObject.Find("PrefabCommandHandlerTests");
            Assert.IsNotNull(instance);
            Assert.AreEqual(parent.transform, instance.transform.parent);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), instance.transform.localPosition);

            UnityEngine.Object.DestroyImmediate(parent);
        }

        [Test]
        public void Route_PrefabSave_SavesHierarchyToPrefabAsset()
        {
            var root = new UnityEngine.GameObject("SaveRoot");
            var child = new UnityEngine.GameObject("SaveChild");
            child.transform.SetParent(root.transform, false);

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "prefab-save",
                    Command = "prefab.save",
                    ParamsJson = $"{{\"instanceId\":{root.GetInstanceID()},\"prefabPath\":\"{TempPrefabPath}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as PrefabSaveCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(TempPrefabPath, result.PrefabPath);
            Assert.AreEqual("SaveRoot", result.SourceName);
            Assert.AreEqual(root.GetInstanceID(), result.SourceInstanceId);
            Assert.IsTrue(File.Exists(TempPrefabPath));

            var loaded = PrefabUtility.LoadPrefabContents(TempPrefabPath);
            Assert.IsNotNull(loaded.transform.Find("SaveChild"));
            PrefabUtility.UnloadPrefabContents(loaded);

            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
