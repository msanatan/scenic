using System.IO;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Asset;
using UnityEditor;
using UnityEngine;

namespace Scenic.Editor.Tests.Commands.Asset
{
    [TestFixture]
    public class AssetLabelsRemoveCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string TestAssetPath = "Assets/__TempTests__/LabelsRemoveTest.mat";

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
        public void Route_AssetLabelsRemove_SuccessPath()
        {
            var shader = Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("HDRP/Lit");
            Assert.IsNotNull(shader, "Expected at least one default shader to exist for test setup.");

            var material = new UnityEngine.Material(shader) { name = "LabelsRemoveTest" };
            AssetDatabase.CreateAsset(material, TestAssetPath);
            AssetDatabase.SaveAssets();

            var asset = AssetDatabase.LoadMainAssetAtPath(TestAssetPath);
            AssetDatabase.SetLabels(asset, new[] { "LabelToRemove", "LabelToKeep" });
            AssetDatabase.SaveAssets();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-labels-remove",
                    Command = "asset.labels.remove",
                    ParamsJson = $"{{\"assetPath\":\"{TestAssetPath}\",\"labels\":[\"LabelToRemove\"]}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);

            var result = response.Result as AssetLabelsRemoveCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(TestAssetPath, result.AssetPath);
            Assert.IsTrue(System.Array.Exists(result.Removed, l => l == "LabelToRemove"));
            Assert.IsTrue(System.Array.Exists(result.Labels, l => l == "LabelToKeep"));
            Assert.IsFalse(System.Array.Exists(result.Labels, l => l == "LabelToRemove"));

            var refreshedAsset = AssetDatabase.LoadMainAssetAtPath(TestAssetPath);
            var actualLabels = AssetDatabase.GetLabels(refreshedAsset);
            Assert.IsFalse(System.Array.Exists(actualLabels, l => l == "LabelToRemove"));
            Assert.IsTrue(System.Array.Exists(actualLabels, l => l == "LabelToKeep"));
        }

        [Test]
        public void Route_AssetLabelsRemove_ValidationError_EmptyLabels()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-labels-remove-err",
                    Command = "asset.labels.remove",
                    ParamsJson = "{\"assetPath\": \"Assets/some.mat\", \"labels\": []}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("labels", response.Error.ToLower());
        }

        [Test]
        public void Route_AssetLabelsRemove_ValidationError_MissingLabels()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-labels-remove-missing",
                    Command = "asset.labels.remove",
                    ParamsJson = "{\"assetPath\": \"Assets/some.mat\"}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("labels", response.Error.ToLower());
        }

        [Test]
        public void Route_AssetLabelsRemove_ExecuteGuard()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-labels-remove-guard",
                    Command = "asset.labels.remove",
                    ParamsJson = $"{{\"assetPath\":\"{TestAssetPath}\",\"labels\":[\"SomeLabel\"]}}",
                },
                executeEnabled: false);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
        }
    }
}
