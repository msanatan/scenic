using System.IO;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Material;
using UnityEditor;
using UnityEngine;

namespace Scenic.Editor.Tests.Commands.Material
{
    [TestFixture]
    public class MaterialGetCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string MaterialPath = "Assets/__TempTests__/MaterialGetCommandHandlerTests.mat";

        [SetUp]
        public void SetUp()
        {
            if (!AssetDatabase.IsValidFolder(TempDirectoryPath))
            {
                AssetDatabase.CreateFolder("Assets", "__TempTests__");
            }

            AssetDatabase.DeleteAsset(MaterialPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(MaterialPath);

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
        public void Route_MaterialGet_SuccessPath()
        {
            var sourceShader = Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("HDRP/Lit");
            Assert.IsNotNull(sourceShader, "Expected at least one default shader to exist for test setup.");

            var source = new UnityEngine.Material(sourceShader)
            {
                name = "MaterialGetCommandHandlerTests",
            };
            AssetDatabase.CreateAsset(source, MaterialPath);
            AssetDatabase.SaveAssets();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-material-get",
                    Command = "material.get",
                    ParamsJson = $"{{\"assetPath\":\"{MaterialPath}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            Assert.AreEqual("test-material-get", response.Id);

            var result = response.Result as MaterialGetCommandResult;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Material);
            Assert.AreEqual(MaterialPath, result.Material.AssetPath);
            Assert.AreEqual("MaterialGetCommandHandlerTests", result.Material.Name);
            Assert.IsNotEmpty(result.Material.Shader);
            Assert.AreNotEqual(0, result.Material.InstanceId);
        }

        [Test]
        public void Route_MaterialGet_ValidationError()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-material-get-err",
                    Command = "material.get",
                    ParamsJson = "{}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("assetpath", response.Error.ToLower());
        }
    }
}
