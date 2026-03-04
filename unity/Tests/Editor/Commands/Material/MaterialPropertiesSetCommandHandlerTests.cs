using System.IO;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Material;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scenic.Editor.Tests.Commands.Material
{
    [TestFixture]
    public sealed class MaterialPropertiesSetCommandHandlerTests
    {
        private const string TempDirectoryPath = "Assets/__TempTests__";
        private const string MaterialPath = "Assets/__TempTests__/MaterialPropertiesSetCommandHandlerTests.mat";
        private const string TexturePath = "Assets/__TempTests__/MaterialPropertiesSetCommandHandlerTests.asset";

        [SetUp]
        public void SetUp()
        {
            if (!AssetDatabase.IsValidFolder(TempDirectoryPath))
            {
                AssetDatabase.CreateFolder("Assets", "__TempTests__");
            }

            AssetDatabase.DeleteAsset(MaterialPath);
            AssetDatabase.DeleteAsset(TexturePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(MaterialPath);
            AssetDatabase.DeleteAsset(TexturePath);

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
        public void Route_MaterialPropertiesSet_AppliesSupportedProperties()
        {
            var shader = Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("HDRP/Lit");
            Assert.IsNotNull(shader, "Expected at least one default shader to exist for test setup.");

            var material = new UnityEngine.Material(shader)
            {
                name = "MaterialPropertiesSetCommandHandlerTests",
            };
            AssetDatabase.CreateAsset(material, MaterialPath);

            var texture = new Texture2D(2, 2);
            texture.SetPixel(0, 0, Color.red);
            texture.Apply();
            AssetDatabase.CreateAsset(texture, TexturePath);
            AssetDatabase.SaveAssets();

            var values = new JObject();
            var floatProperty = FindPropertyByType(material, ShaderPropertyType.Float);
            if (floatProperty != null)
            {
                values[floatProperty] = new JObject
                {
                    ["type"] = "float",
                    ["value"] = 0.5f,
                };
            }

            var rangeProperty = FindPropertyByType(material, ShaderPropertyType.Range);
            if (rangeProperty != null)
            {
                values[rangeProperty] = new JObject
                {
                    ["type"] = "range",
                    ["value"] = 0.25f,
                };
            }

            var intProperty = FindPropertyByType(material, ShaderPropertyType.Int);
            if (intProperty != null)
            {
                values[intProperty] = new JObject
                {
                    ["type"] = "int",
                    ["value"] = 3,
                };
            }

            var colorProperty = FindPropertyByType(material, ShaderPropertyType.Color);
            if (colorProperty != null)
            {
                values[colorProperty] = new JObject
                {
                    ["type"] = "color",
                    ["value"] = new JObject
                    {
                        ["r"] = 0.2f,
                        ["g"] = 0.4f,
                        ["b"] = 0.6f,
                        ["a"] = 1f,
                    },
                };
            }

            var vectorProperty = FindPropertyByType(material, ShaderPropertyType.Vector);
            if (vectorProperty != null)
            {
                values[vectorProperty] = new JObject
                {
                    ["type"] = "vector",
                    ["value"] = new JObject
                    {
                        ["x"] = 1f,
                        ["y"] = 2f,
                        ["z"] = 3f,
                        ["w"] = 4f,
                    },
                };
            }

            var textureProperty = FindPropertyByType(material, ShaderPropertyType.Texture);
            if (textureProperty != null)
            {
                values[textureProperty] = new JObject
                {
                    ["type"] = "texture",
                    ["assetPath"] = TexturePath,
                };
            }

            Assert.Greater(values.Count, 0, "Expected at least one supported material property on test shader.");

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-material-props-set",
                    Command = "material.properties.set",
                    ParamsJson = new JObject
                    {
                        ["assetPath"] = MaterialPath,
                        ["values"] = values,
                        ["strict"] = true,
                    }.ToString(),
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as MaterialPropertiesSetCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(MaterialPath, result.Material.AssetPath);
            Assert.Greater(result.AppliedProperties.Length, 0);
            Assert.AreEqual(0, result.IgnoredProperties.Length);

            var reloaded = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(MaterialPath);
            Assert.IsNotNull(reloaded);
            if (floatProperty != null)
            {
                Assert.AreEqual(0.5f, reloaded.GetFloat(floatProperty), 0.0001f);
            }
            if (rangeProperty != null)
            {
                Assert.AreEqual(0.25f, reloaded.GetFloat(rangeProperty), 0.0001f);
            }
            if (intProperty != null)
            {
                Assert.AreEqual(3, reloaded.GetInt(intProperty));
            }

            if (colorProperty != null)
            {
                var color = reloaded.GetColor(colorProperty);
                Assert.AreEqual(0.2f, color.r, 0.0001f);
                Assert.AreEqual(0.4f, color.g, 0.0001f);
                Assert.AreEqual(0.6f, color.b, 0.0001f);
                Assert.AreEqual(1f, color.a, 0.0001f);
            }

            if (vectorProperty != null)
            {
                var vector = reloaded.GetVector(vectorProperty);
                Assert.AreEqual(1f, vector.x, 0.0001f);
                Assert.AreEqual(2f, vector.y, 0.0001f);
                Assert.AreEqual(3f, vector.z, 0.0001f);
                Assert.AreEqual(4f, vector.w, 0.0001f);
            }

            if (textureProperty != null)
            {
                var assigned = reloaded.GetTexture(textureProperty);
                Assert.IsNotNull(assigned);
                Assert.AreEqual(TexturePath, AssetDatabase.GetAssetPath(assigned));
            }
        }

        [Test]
        public void Route_MaterialPropertiesSet_ValidationError()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-material-props-set-err",
                    Command = "material.properties.set",
                    ParamsJson = "{}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("assetpath", response.Error.ToLowerInvariant());
        }

        private static string FindPropertyByType(UnityEngine.Material material, params ShaderPropertyType[] types)
        {
            var shader = material.shader;
            var count = shader.GetPropertyCount();
            for (var i = 0; i < count; i++)
            {
                var propertyType = shader.GetPropertyType(i);
                for (var j = 0; j < types.Length; j++)
                {
                    if (propertyType == types[j])
                    {
                        return shader.GetPropertyName(i);
                    }
                }
            }

            return null;
        }
    }
}
