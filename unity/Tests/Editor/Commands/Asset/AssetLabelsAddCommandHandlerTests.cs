using System.IO;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Asset;
using UnityEditor;
using UnityEngine;

namespace Scenic.Editor.Tests.Commands.Asset
{
    [TestFixture]
    public class AssetLabelsAddCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string TestAssetPath = "Assets/__TempTests__/LabelsAddTest.mat";

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
        public void Route_AssetLabelsAdd_SuccessPath()
        {
            var shader = Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("HDRP/Lit");
            Assert.IsNotNull(shader, "Expected at least one default shader to exist for test setup.");

            var material = new UnityEngine.Material(shader) { name = "LabelsAddTest" };
            AssetDatabase.CreateAsset(material, TestAssetPath);
            AssetDatabase.SaveAssets();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-labels-add",
                    Command = "asset.labels.add",
                    ParamsJson = $"{{\"assetPath\":\"{TestAssetPath}\",\"labels\":[\"TestLabel\",\"AnotherLabel\"]}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);

            var result = response.Result as AssetLabelsAddCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(TestAssetPath, result.AssetPath);
            Assert.IsTrue(System.Array.Exists(result.Labels, l => l == "TestLabel"));
            Assert.IsTrue(System.Array.Exists(result.Labels, l => l == "AnotherLabel"));
            Assert.IsTrue(System.Array.Exists(result.Added, l => l == "TestLabel"));

            var refreshedAsset = AssetDatabase.LoadMainAssetAtPath(TestAssetPath);
            var actualLabels = AssetDatabase.GetLabels(refreshedAsset);
            Assert.IsTrue(System.Array.Exists(actualLabels, l => l == "TestLabel"));
            Assert.IsTrue(System.Array.Exists(actualLabels, l => l == "AnotherLabel"));
        }

        [Test]
        public void Route_AssetLabelsAdd_ValidationError_EmptyLabels()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-labels-add-err",
                    Command = "asset.labels.add",
                    ParamsJson = "{\"assetPath\": \"Assets/some.mat\", \"labels\": []}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("labels", response.Error.ToLower());
        }

        [Test]
        public void Route_AssetLabelsAdd_ValidationError_MissingLabels()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-labels-add-missing",
                    Command = "asset.labels.add",
                    ParamsJson = "{\"assetPath\": \"Assets/some.mat\"}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("labels", response.Error.ToLower());
        }
    }
}
