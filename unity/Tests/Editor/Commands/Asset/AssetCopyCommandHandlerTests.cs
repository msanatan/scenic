using System.IO;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Asset;
using UnityEditor;
using UnityEngine;

namespace Scenic.Editor.Tests.Commands.Asset
{
    [TestFixture]
    public class AssetCopyCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string SourcePath = "Assets/__TempTests__/AssetCopySource.mat";
        private const string DestPath = "Assets/__TempTests__/AssetCopyDest.mat";

        [SetUp]
        public void SetUp()
        {
            if (!AssetDatabase.IsValidFolder(TempDirectoryPath))
            {
                AssetDatabase.CreateFolder("Assets", "__TempTests__");
            }

            AssetDatabase.DeleteAsset(SourcePath);
            AssetDatabase.DeleteAsset(DestPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(SourcePath);
            AssetDatabase.DeleteAsset(DestPath);

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
        public void Route_AssetCopy_SuccessPath()
        {
            var shader = Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("HDRP/Lit");
            Assert.IsNotNull(shader, "Expected at least one default shader to exist for test setup.");

            var material = new UnityEngine.Material(shader) { name = "AssetCopySource" };
            AssetDatabase.CreateAsset(material, SourcePath);
            AssetDatabase.SaveAssets();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-asset-copy",
                    Command = "asset.copy",
                    ParamsJson = $"{{\"assetPath\":\"{SourcePath}\",\"newPath\":\"{DestPath}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);

            var result = response.Result as AssetCopyCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(SourcePath, result.SourcePath);
            Assert.AreEqual(DestPath, result.NewPath);
            Assert.AreNotEqual(AssetDatabase.AssetPathToGUID(SourcePath), result.Guid);
            Assert.AreEqual(result.Guid, AssetDatabase.AssetPathToGUID(DestPath));
        }

        [Test]
        public void Route_AssetCopy_ValidationError()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-asset-copy-err",
                    Command = "asset.copy",
                    ParamsJson = "{\"assetPath\": \"Assets/some.mat\"}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("newpath", response.Error.ToLower());
        }
    }
}
