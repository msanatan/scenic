using System.IO;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Asset;
using UnityEditor;
using UnityEngine;

namespace Scenic.Editor.Tests.Commands.Asset
{
    [TestFixture]
    public class AssetImportSettingsGetCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string TestAssetPath = "Assets/__TempTests__/ImportSettingsGetTest.mat";

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
        public void Route_AssetImportSettingsGet_SuccessPath()
        {
            var shader = Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("HDRP/Lit");
            Assert.IsNotNull(shader, "Expected at least one default shader to exist for test setup.");

            var material = new UnityEngine.Material(shader) { name = "ImportSettingsGetTest" };
            AssetDatabase.CreateAsset(material, TestAssetPath);
            AssetDatabase.SaveAssets();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-import-settings-get",
                    Command = "asset.importSettings.get",
                    ParamsJson = $"{{\"assetPath\":\"{TestAssetPath}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);

            var result = response.Result as AssetImportSettingsGetCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(TestAssetPath, result.AssetPath);
            Assert.IsNotEmpty(result.ImporterType);
            Assert.IsNotNull(result.Properties);
        }

        [Test]
        public void Route_AssetImportSettingsGet_WithProperties()
        {
            var shader = Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("HDRP/Lit");
            Assert.IsNotNull(shader, "Expected at least one default shader to exist for test setup.");

            var material = new UnityEngine.Material(shader) { name = "ImportSettingsGetTest" };
            AssetDatabase.CreateAsset(material, TestAssetPath);
            AssetDatabase.SaveAssets();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-import-settings-get-props",
                    Command = "asset.importSettings.get",
                    ParamsJson = $"{{\"assetPath\":\"{TestAssetPath}\",\"properties\":[\"m_UserData\"]}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);

            var result = response.Result as AssetImportSettingsGetCommandResult;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Properties.ContainsKey("m_UserData"));
        }

        [Test]
        public void Route_AssetImportSettingsGet_ValidationError()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-import-settings-get-err",
                    Command = "asset.importSettings.get",
                    ParamsJson = "{}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("assetpath", response.Error.ToLower());
        }
    }
}
