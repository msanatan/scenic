using System.IO;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Material;
using UnityEditor;
using UnityEngine;

namespace Scenic.Editor.Tests.Commands.Material
{
    [TestFixture]
    public class MaterialCreateCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string MaterialPath = "Assets/__TempTests__/MaterialCreateCommandHandlerTests.mat";

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
        public void Route_MaterialCreate_SuccessPath()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-material-create",
                    Command = "material.create",
                    ParamsJson = $"{{\"assetPath\":\"{MaterialPath}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            Assert.AreEqual("test-material-create", response.Id);

            var result = response.Result as MaterialCreateCommandResult;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Material);
            Assert.AreEqual(MaterialPath, result.Material.AssetPath);
            Assert.AreEqual("MaterialCreateCommandHandlerTests", result.Material.Name);
            Assert.IsNotEmpty(result.Material.Shader);
            Assert.AreNotEqual(0, result.Material.InstanceId);

            var created = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(MaterialPath);
            Assert.IsNotNull(created);
        }

        [Test]
        public void Route_MaterialCreate_ValidationError()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-material-create-err",
                    Command = "material.create",
                    ParamsJson = "{}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("assetpath", response.Error.ToLower());
        }
    }
}
