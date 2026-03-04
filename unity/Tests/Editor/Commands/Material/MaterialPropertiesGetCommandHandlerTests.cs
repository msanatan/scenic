using System.IO;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Material;
using UnityEditor;
using UnityEngine;

namespace Scenic.Editor.Tests.Commands.Material
{
    [TestFixture]
    public sealed class MaterialPropertiesGetCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string MaterialPath = "Assets/__TempTests__/MaterialPropertiesGetCommandHandlerTests.mat";

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
        public void Route_MaterialPropertiesGet_SuccessPath()
        {
            var shader = Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("HDRP/Lit");
            Assert.IsNotNull(shader, "Expected at least one default shader to exist for test setup.");

            var source = new UnityEngine.Material(shader)
            {
                name = "MaterialPropertiesGetCommandHandlerTests",
            };
            AssetDatabase.CreateAsset(source, MaterialPath);
            AssetDatabase.SaveAssets();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-material-props-get",
                    Command = "material.properties.get",
                    ParamsJson = $"{{\"assetPath\":\"{MaterialPath}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as MaterialPropertiesGetCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(MaterialPath, result.Material.AssetPath);
            var propertiesValue = typeof(MaterialPropertiesGetCommandResult)
                .GetField("Properties")
                ?.GetValue(result);
            Assert.IsNotNull(propertiesValue);

            var countProperty = propertiesValue.GetType().GetProperty("Count");
            Assert.IsNotNull(countProperty);
            var propertyCount = (int)countProperty.GetValue(propertiesValue, null);
            Assert.Greater(propertyCount, 0);
        }

        [Test]
        public void Route_MaterialPropertiesGet_UnknownProperty_ReturnsError()
        {
            var shader = Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("HDRP/Lit");
            Assert.IsNotNull(shader, "Expected at least one default shader to exist for test setup.");

            var source = new UnityEngine.Material(shader)
            {
                name = "MaterialPropertiesGetCommandHandlerTests",
            };
            AssetDatabase.CreateAsset(source, MaterialPath);
            AssetDatabase.SaveAssets();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-material-props-get-err",
                    Command = "material.properties.get",
                    ParamsJson = $"{{\"assetPath\":\"{MaterialPath}\",\"names\":[\"DefinitelyNotARealProperty\"]}}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("not found", response.Error.ToLowerInvariant());
        }
    }
}
