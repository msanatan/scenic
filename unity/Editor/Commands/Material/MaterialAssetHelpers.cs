using System;
using System.IO;
using Scenic.Editor.Commands;
using UnityEditor;
using UnityEngine;

namespace Scenic.Editor.Commands.Material
{
    internal static class MaterialAssetHelpers
    {
        private static readonly string[] DefaultShaderCandidates =
        {
            "Standard",
            "Universal Render Pipeline/Lit",
            "HDRP/Lit",
        };

        public static string NormalizeAssetPath(string assetPath, bool requireExists)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                throw new CommandHandlingException("params.assetPath is required.");
            }

            var normalized = assetPath.Trim().Replace("\\", "/");
            if (!normalized.StartsWith("Assets/", StringComparison.Ordinal))
            {
                throw new CommandHandlingException("params.assetPath must start with \"Assets/\".");
            }

            if (!normalized.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
            {
                throw new CommandHandlingException("params.assetPath must end with \".mat\".");
            }

            if (requireExists && !File.Exists(normalized))
            {
                throw new CommandHandlingException($"Material asset not found: {normalized}");
            }

            return normalized;
        }

        public static Shader ResolveShader(string shaderName)
        {
            if (!string.IsNullOrWhiteSpace(shaderName))
            {
                var provided = Shader.Find(shaderName.Trim());
                if (provided == null)
                {
                    throw new CommandHandlingException($"Shader not found: {shaderName}");
                }

                return provided;
            }

            foreach (var candidate in DefaultShaderCandidates)
            {
                var shader = Shader.Find(candidate);
                if (shader != null)
                {
                    return shader;
                }
            }

            throw new CommandHandlingException("Unable to resolve a default shader. Provide params.shader.");
        }

        public static UnityEngine.Material LoadRequired(string assetPath)
        {
            var material = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(assetPath);
            if (material == null)
            {
                throw new CommandHandlingException($"Material asset not found: {assetPath}");
            }

            return material;
        }

        public static MaterialSummary BuildSummary(UnityEngine.Material material, string assetPath)
        {
            return new MaterialSummary
            {
                AssetPath = assetPath,
                Name = material.name,
                Shader = material.shader != null ? material.shader.name : string.Empty,
                InstanceId = material.GetInstanceID(),
            };
        }
    }
}
