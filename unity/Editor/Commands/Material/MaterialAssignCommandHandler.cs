using Scenic.Editor.Commands.GameObject;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Scenic.Editor.Commands.Material
{
    [ScenicCommand("material.assign")]
    public sealed class MaterialAssignCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = MaterialAssignCommandParams.From(request);
            var target = GameObjectLookup.ResolveRequired(parameters.Path, parameters.InstanceId, "Target");
            var renderers = target.GetComponents<Renderer>();
            if (renderers == null || renderers.Length == 0)
            {
                throw new CommandHandlingException(
                    $"Target GameObject has no Renderer components: {GameObjectLookup.BuildPath(target.transform)}");
            }

            if (parameters.RendererIndex >= renderers.Length)
            {
                throw new CommandHandlingException(
                    $"params.rendererIndex out of range for target renderers (count={renderers.Length}): {parameters.RendererIndex}");
            }

            var assetPath = MaterialAssetHelpers.NormalizeAssetPath(parameters.AssetPath, requireExists: true);
            var material = MaterialAssetHelpers.LoadRequired(assetPath);
            var renderer = renderers[parameters.RendererIndex];

            var current = renderer.sharedMaterials ?? new UnityEngine.Material[0];
            var nextLength = current.Length;
            if (parameters.Slot >= nextLength)
            {
                nextLength = parameters.Slot + 1;
            }

            var next = new UnityEngine.Material[nextLength];
            for (var i = 0; i < current.Length; i++)
            {
                next[i] = current[i];
            }

            next[parameters.Slot] = material;
            renderer.sharedMaterials = next;
            EditorUtility.SetDirty(renderer);
            if (renderer.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(renderer.gameObject.scene);
            }

            return new MaterialAssignCommandResult
            {
                TargetPath = GameObjectLookup.BuildPath(target.transform),
                TargetInstanceId = target.GetInstanceID(),
                RendererType = renderer.GetType().FullName ?? renderer.GetType().Name,
                RendererIndex = parameters.RendererIndex,
                RendererInstanceId = renderer.GetInstanceID(),
                Slot = parameters.Slot,
                Material = MaterialAssetHelpers.BuildSummary(material, assetPath),
            };
        }
    }
}
