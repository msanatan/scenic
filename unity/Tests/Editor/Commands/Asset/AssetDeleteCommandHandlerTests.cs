using System.IO;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Asset;
using UnityEditor;
using UnityEngine;

namespace Scenic.Editor.Tests.Commands.Asset
{
    [TestFixture]
    public class AssetDeleteCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string TestAssetPath = "Assets/__TempTests__/AssetDeleteTest.mat";

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
        public void Route_AssetDelete_SuccessPath()
        {
            var shader = Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("HDRP/Lit");
            Assert.IsNotNull(shader, "Expected at least one default shader to exist for test setup.");

            var material = new UnityEngine.Material(shader) { name = "AssetDeleteTest" };
            AssetDatabase.CreateAsset(material, TestAssetPath);
            AssetDatabase.SaveAssets();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-asset-delete",
                    Command = "asset.delete",
                    ParamsJson = $"{{\"assetPath\":\"{TestAssetPath}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);

            var result = response.Result as AssetDeleteCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(TestAssetPath, result.AssetPath);
            Assert.IsTrue(result.Deleted);
            Assert.IsEmpty(AssetDatabase.AssetPathToGUID(TestAssetPath, AssetPathToGUIDOptions.OnlyExistingAssets));
        }

        [Test]
        public void Route_AssetDelete_ValidationError()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-asset-delete-err",
                    Command = "asset.delete",
                    ParamsJson = "{}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("assetpath", response.Error.ToLower());
        }

        [Test]
        public void Route_AssetDelete_ExecuteGuard()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-asset-delete-guard",
                    Command = "asset.delete",
                    ParamsJson = $"{{\"assetPath\":\"{TestAssetPath}\"}}",
                },
                executeEnabled: false);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
        }
    }
}
