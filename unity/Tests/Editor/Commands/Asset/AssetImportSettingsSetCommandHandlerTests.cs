using System.IO;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Asset;
using UnityEditor;
using UnityEngine;

namespace Scenic.Editor.Tests.Commands.Asset
{
    [TestFixture]
    public class AssetImportSettingsSetCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string TestAssetPath = "Assets/__TempTests__/ImportSettingsSetTest.mat";

        [SetUp]
        public void SetUp()
        {
            if (!AssetDatabase.IsValidFolder(TempDirectoryPath))
            {
                AssetDatabase.CreateFolder("Assets", "__TempTests__");
            }

            AssetDatabase.DeleteAsset(TestAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TestAssetPath);

            var hasAnyTempAssets = Directory.Exists(TempDirectoryPath)
                && Directory.GetFiles(TempDirectoryPath).Length > 0;
            if (!hasAnyTempAssets && AssetDatabase.IsValidFolder(TempDirectoryPath))
            {
                AssetDatabase.DeleteAsset(TempDirectoryPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [Test]
        public void Route_AssetImportSettingsSet_SuccessPath()
        {
            var shader = Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("HDRP/Lit");
            Assert.IsNotNull(shader, "Expected at least one default shader to exist for test setup.");

            var material = new UnityEngine.Material(shader) { name = "ImportSettingsSetTest" };
            AssetDatabase.CreateAsset(material, TestAssetPath);
            AssetDatabase.SaveAssets();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-import-settings-set",
                    Command = "asset.importSettings.set",
                    ParamsJson = $"{{\"assetPath\":\"{TestAssetPath}\",\"properties\":{{\"m_UserData\":\"test-value\"}}}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);

            var result = response.Result as AssetImportSettingsSetCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(TestAssetPath, result.AssetPath);
            Assert.IsNotEmpty(result.ImporterType);
            Assert.IsTrue(System.Array.Exists(result.AppliedProperties, p => p == "m_UserData"));

            var importer = AssetImporter.GetAtPath(TestAssetPath);
            var so = new SerializedObject(importer);
            so.Update();
            Assert.AreEqual("test-value", so.FindProperty("m_UserData")?.stringValue);
        }

        [Test]
        public void Route_AssetImportSettingsSet_ValidationError_MissingProperties()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-import-settings-set-err",
                    Command = "asset.importSettings.set",
                    ParamsJson = "{\"assetPath\": \"Assets/some.mat\"}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("properties", response.Error.ToLower());
        }

        [Test]
        public void Route_AssetImportSettingsSet_ValidationError_EmptyProperties()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-import-settings-set-empty",
                    Command = "asset.importSettings.set",
                    ParamsJson = "{\"assetPath\": \"Assets/some.mat\", \"properties\": {}}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("properties", response.Error.ToLower());
        }
    }
}
