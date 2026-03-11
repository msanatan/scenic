using System.IO;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Asset;
using UnityEditor;
using UnityEngine;

namespace Scenic.Editor.Tests.Commands.Asset
{
    [TestFixture]
    public class AssetMoveCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string SourcePath = "Assets/__TempTests__/AssetMoveSource.mat";
        private const string DestPath = "Assets/__TempTests__/AssetMoveDest.mat";

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
        public void Route_AssetMove_SuccessPath()
        {
            var shader = Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("HDRP/Lit");
            Assert.IsNotNull(shader, "Expected at least one default shader to exist for test setup.");

            var material = new UnityEngine.Material(shader) { name = "AssetMoveSource" };
            AssetDatabase.CreateAsset(material, SourcePath);
            AssetDatabase.SaveAssets();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-asset-move",
                    Command = "asset.move",
                    ParamsJson = $"{{\"assetPath\":\"{SourcePath}\",\"newPath\":\"{DestPath}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);

            var result = response.Result as AssetMoveCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(SourcePath, result.OldPath);
            Assert.AreEqual(DestPath, result.NewPath);
            Assert.IsEmpty(AssetDatabase.AssetPathToGUID(SourcePath));
            Assert.AreEqual(result.Guid, AssetDatabase.AssetPathToGUID(DestPath));
        }

        [Test]
        public void Route_AssetMove_ValidationError()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-asset-move-err",
                    Command = "asset.move",
                    ParamsJson = "{\"assetPath\": \"Assets/some.mat\"}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("newpath", response.Error.ToLower());
        }

        [Test]
        public void Route_AssetMove_ExecuteGuard()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-asset-move-guard",
                    Command = "asset.move",
                    ParamsJson = $"{{\"assetPath\":\"{SourcePath}\",\"newPath\":\"{DestPath}\"}}",
                },
                executeEnabled: false);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
        }
    }
}
