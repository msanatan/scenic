using System.IO;
using NUnit.Framework;
using UnityEditor;
using Scenic.Editor;
using Scenic.Editor.Commands.ScriptableObjects;

namespace Scenic.Editor.Tests.Commands.ScriptableObject
{
    [TestFixture]
    public sealed class ScriptableObjectCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string AssetPath = "Assets/__TempTests__/ScenicSample.asset";
        private const string TypeName = "Scenic.Editor.Commands.ScriptableObjects.ScenicSampleScriptableObject";

        [SetUp]
        public void SetUp()
        {
            if (!AssetDatabase.IsValidFolder(TempDirectoryPath))
            {
                AssetDatabase.CreateFolder("Assets", "__TempTests__");
            }

            AssetDatabase.DeleteAsset(AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(AssetPath);

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
        public void Route_ScriptableObjectCreate_CreatesAssetAndAppliesValues()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "scriptableobject-create",
                    Command = "scriptableobject.create",
                    ParamsJson = $"{{\"assetPath\":\"{AssetPath}\",\"type\":\"{TypeName}\",\"initialValues\":{{\"number\":8.25,\"label\":\"hello\",\"enabledFlag\":true}},\"strict\":true}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as ScriptableObjectCreateCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(AssetPath, result.Asset.AssetPath);
            Assert.AreEqual(TypeName, result.Asset.Type);
            Assert.Contains("number", result.AppliedFields);
            Assert.Contains("label", result.AppliedFields);
            Assert.Contains("enabledFlag", result.AppliedFields);

            var loaded = AssetDatabase.LoadAssetAtPath<ScenicSampleScriptableObject>(AssetPath);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(8.25f, loaded.number, 0.0001f);
            Assert.AreEqual("hello", loaded.label);
            Assert.IsTrue(loaded.enabledFlag);
        }

        [Test]
        public void Route_ScriptableObjectGet_ReturnsSerializedValues()
        {
            var created = UnityEngine.ScriptableObject.CreateInstance<ScenicSampleScriptableObject>();
            created.number = 2.5f;
            created.label = "get";
            created.enabledFlag = false;
            AssetDatabase.CreateAsset(created, AssetPath);
            AssetDatabase.SaveAssets();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "scriptableobject-get",
                    Command = "scriptableobject.get",
                    ParamsJson = $"{{\"assetPath\":\"{AssetPath}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as ScriptableObjectGetCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(AssetPath, result.Asset.AssetPath);
            Assert.AreEqual(TypeName, result.Asset.Type);
            Assert.IsNotNull(result.Serialized);
        }

        [Test]
        public void Route_ScriptableObjectUpdate_UpdatesExistingAsset()
        {
            var created = UnityEngine.ScriptableObject.CreateInstance<ScenicSampleScriptableObject>();
            created.number = 1f;
            created.label = "before";
            created.enabledFlag = false;
            AssetDatabase.CreateAsset(created, AssetPath);
            AssetDatabase.SaveAssets();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "scriptableobject-update",
                    Command = "scriptableobject.update",
                    ParamsJson = $"{{\"assetPath\":\"{AssetPath}\",\"values\":{{\"number\":12.75,\"label\":\"after\",\"enabledFlag\":true}},\"strict\":true}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as ScriptableObjectUpdateCommandResult;
            Assert.IsNotNull(result);
            Assert.Contains("number", result.AppliedFields);
            Assert.Contains("label", result.AppliedFields);
            Assert.Contains("enabledFlag", result.AppliedFields);

            var loaded = AssetDatabase.LoadAssetAtPath<ScenicSampleScriptableObject>(AssetPath);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(12.75f, loaded.number, 0.0001f);
            Assert.AreEqual("after", loaded.label);
            Assert.IsTrue(loaded.enabledFlag);
        }
    }
}
