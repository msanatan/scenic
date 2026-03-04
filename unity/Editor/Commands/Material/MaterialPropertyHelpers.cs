using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Scenic.Editor.Commands;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scenic.Editor.Commands.Material
{
    internal static class MaterialPropertyHelpers
    {
        public static bool TryGetPropertyType(UnityEngine.Material material, string propertyName, out ShaderPropertyType propertyType)
        {
            var shader = material.shader;
            var count = shader.GetPropertyCount();
            for (var i = 0; i < count; i++)
            {
                if (!string.Equals(shader.GetPropertyName(i), propertyName, StringComparison.Ordinal))
                {
                    continue;
                }

                propertyType = shader.GetPropertyType(i);
                return true;
            }

            propertyType = default;
            return false;
        }

        public static IEnumerable<string> GetSupportedPropertyNames(UnityEngine.Material material)
        {
            var shader = material.shader;
            var count = shader.GetPropertyCount();
            for (var i = 0; i < count; i++)
            {
                var propertyType = shader.GetPropertyType(i);
                if (!IsSupported(propertyType))
                {
                    continue;
                }

                yield return shader.GetPropertyName(i);
            }
        }

        public static bool IsSupported(ShaderPropertyType propertyType)
        {
            return propertyType == ShaderPropertyType.Float
                || propertyType == ShaderPropertyType.Range
                || propertyType == ShaderPropertyType.Int
                || propertyType == ShaderPropertyType.Color
                || propertyType == ShaderPropertyType.Vector
                || propertyType == ShaderPropertyType.Texture;
        }

        public static string ToWireType(ShaderPropertyType propertyType)
        {
            switch (propertyType)
            {
                case ShaderPropertyType.Float:
                    return "float";
                case ShaderPropertyType.Range:
                    return "range";
                case ShaderPropertyType.Int:
                    return "int";
                case ShaderPropertyType.Color:
                    return "color";
                case ShaderPropertyType.Vector:
                    return "vector";
                case ShaderPropertyType.Texture:
                    return "texture";
                default:
                    throw new CommandHandlingException($"Unsupported material property type: {propertyType}");
            }
        }

        public static JObject ReadValue(UnityEngine.Material material, string propertyName, ShaderPropertyType propertyType)
        {
            switch (propertyType)
            {
                case ShaderPropertyType.Float:
                    return new JObject
                    {
                        ["type"] = "float",
                        ["value"] = material.GetFloat(propertyName),
                    };
                case ShaderPropertyType.Range:
                    return new JObject
                    {
                        ["type"] = "range",
                        ["value"] = material.GetFloat(propertyName),
                    };
                case ShaderPropertyType.Int:
                    return new JObject
                    {
                        ["type"] = "int",
                        ["value"] = material.GetInt(propertyName),
                    };
                case ShaderPropertyType.Color:
                    var color = material.GetColor(propertyName);
                    return new JObject
                    {
                        ["type"] = "color",
                        ["value"] = new JObject
                        {
                            ["r"] = color.r,
                            ["g"] = color.g,
                            ["b"] = color.b,
                            ["a"] = color.a,
                        },
                    };
                case ShaderPropertyType.Vector:
                    var vector = material.GetVector(propertyName);
                    return new JObject
                    {
                        ["type"] = "vector",
                        ["value"] = new JObject
                        {
                            ["x"] = vector.x,
                            ["y"] = vector.y,
                            ["z"] = vector.z,
                            ["w"] = vector.w,
                        },
                    };
                case ShaderPropertyType.Texture:
                    var texture = material.GetTexture(propertyName);
                    return new JObject
                    {
                        ["type"] = "texture",
                        ["assetPath"] = texture == null ? JValue.CreateNull() : AssetDatabase.GetAssetPath(texture),
                        ["textureType"] = texture == null ? JValue.CreateNull() : texture.GetType().FullName,
                    };
                default:
                    throw new CommandHandlingException($"Unsupported material property type: {propertyType}");
            }
        }

        public static void ApplyValue(UnityEngine.Material material, string propertyName, JObject valueSpec, ShaderPropertyType propertyType)
        {
            var requestedType = valueSpec.Value<string>("type");
            if (string.IsNullOrWhiteSpace(requestedType))
            {
                throw new CommandHandlingException($"values.{propertyName}.type is required.");
            }

            var expectedType = ToWireType(propertyType);
            if (!string.Equals(requestedType.Trim(), expectedType, StringComparison.Ordinal))
            {
                throw new CommandHandlingException(
                    $"values.{propertyName}.type must be '{expectedType}' for this shader property.");
            }

            switch (expectedType)
            {
                case "float":
                case "range":
                    var floatToken = valueSpec["value"];
                    if (floatToken == null || (floatToken.Type != JTokenType.Float && floatToken.Type != JTokenType.Integer))
                    {
                        throw new CommandHandlingException($"values.{propertyName}.value must be a number.");
                    }

                    material.SetFloat(propertyName, floatToken.Value<float>());
                    return;
                case "int":
                    var intToken = valueSpec["value"];
                    if (intToken == null || intToken.Type != JTokenType.Integer)
                    {
                        throw new CommandHandlingException($"values.{propertyName}.value must be an integer.");
                    }

                    material.SetInt(propertyName, intToken.Value<int>());
                    return;
                case "color":
                    var colorObj = valueSpec["value"] as JObject;
                    if (colorObj == null)
                    {
                        throw new CommandHandlingException($"values.{propertyName}.value must be an object {{r,g,b,a}}.");
                    }

                    material.SetColor(
                        propertyName,
                        new Color(
                            ReadRequiredFloat(colorObj, "r", propertyName),
                            ReadRequiredFloat(colorObj, "g", propertyName),
                            ReadRequiredFloat(colorObj, "b", propertyName),
                            ReadRequiredFloat(colorObj, "a", propertyName)));
                    return;
                case "vector":
                    var vectorObj = valueSpec["value"] as JObject;
                    if (vectorObj == null)
                    {
                        throw new CommandHandlingException($"values.{propertyName}.value must be an object {{x,y,z,w}}.");
                    }

                    material.SetVector(
                        propertyName,
                        new Vector4(
                            ReadRequiredFloat(vectorObj, "x", propertyName),
                            ReadRequiredFloat(vectorObj, "y", propertyName),
                            ReadRequiredFloat(vectorObj, "z", propertyName),
                            ReadRequiredFloat(vectorObj, "w", propertyName)));
                    return;
                case "texture":
                    var texturePathToken = valueSpec["assetPath"];
                    if (texturePathToken == null || texturePathToken.Type == JTokenType.Null)
                    {
                        material.SetTexture(propertyName, null);
                        return;
                    }

                    if (texturePathToken.Type != JTokenType.String)
                    {
                        throw new CommandHandlingException($"values.{propertyName}.assetPath must be a string or null.");
                    }

                    var texturePath = texturePathToken.Value<string>();
                    if (string.IsNullOrWhiteSpace(texturePath))
                    {
                        throw new CommandHandlingException($"values.{propertyName}.assetPath cannot be empty.");
                    }

                    var texture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath.Trim());
                    if (texture == null)
                    {
                        throw new CommandHandlingException($"Texture asset not found: {texturePath}");
                    }

                    material.SetTexture(propertyName, texture);
                    return;
                default:
                    throw new CommandHandlingException($"Unsupported values.{propertyName}.type: {requestedType}");
            }
        }

        private static float ReadRequiredFloat(JObject source, string key, string propertyName)
        {
            var token = source[key];
            if (token == null || (token.Type != JTokenType.Float && token.Type != JTokenType.Integer))
            {
                throw new CommandHandlingException($"values.{propertyName}.value.{key} must be a number.");
            }

            return token.Value<float>();
        }
    }
}
