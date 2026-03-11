using System.IO;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Asset;
using UnityEditor;
using UnityEngine;

namespace Scenic.Editor.Tests.Commands.Asset
{
    [TestFixture]
    public class AssetGetCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string TestAssetPath = "Assets/__TempTests__/AssetGetTest.mat";

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
        public void Route_AssetGet_SuccessPath()
        {
            var shader = Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("HDRP/Lit");
            Assert.IsNotNull(shader, "Expected at least one default shader to exist for test setup.");

            var material = new UnityEngine.Material(shader) { name = "AssetGetTest" };
            AssetDatabase.CreateAsset(material, TestAssetPath);
            AssetDatabase.SaveAssets();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-asset-get",
                    Command = "asset.get",
                    ParamsJson = $"{{\"assetPath\":\"{TestAssetPath}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);

            var result = response.Result as AssetGetCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(TestAssetPath, result.AssetPath);
            Assert.AreEqual(AssetDatabase.AssetPathToGUID(TestAssetPath), result.Guid);
            Assert.AreEqual("AssetGetTest", result.Name);
            Assert.AreEqual("Material", result.Type);
            Assert.IsNotNull(result.Labels);
            Assert.IsNotNull(result.Dependencies);
        }

        [Test]
        public void Route_AssetGet_ValidationError()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-asset-get-err",
                    Command = "asset.get",
                    ParamsJson = "{}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("assetpath", response.Error.ToLower());
        }
    }
}
