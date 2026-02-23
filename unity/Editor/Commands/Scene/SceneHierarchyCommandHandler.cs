using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using Scenic.Editor.Commands;

namespace Scenic.Editor.Commands.Scene
{
    [ScenicCommand("scene.hierarchy")]
    public sealed class SceneHierarchyCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = SceneHierarchyCommandParams.From(request);
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                throw new CommandHandlingException("No active scene is loaded.");
            }

            var nodes = new List<SceneHierarchyNode>();
            var roots = activeScene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                Traverse(roots[i].transform, parentIndex: -1, depth: 0, parentPath: string.Empty, nodes);
            }

            var page = Pagination.Slice(nodes, parameters.Paging, out var total);
            return new SceneHierarchyCommandResult
            {
                Nodes = page,
                Total = total,
                Limit = parameters.Paging.Limit,
                Offset = parameters.Paging.Offset,
            };
        }

        private static void Traverse(
            Transform current,
            int parentIndex,
            int depth,
            string parentPath,
            List<SceneHierarchyNode> output)
        {
            var name = string.IsNullOrWhiteSpace(current.gameObject.name) ? "Unnamed" : current.gameObject.name;
            var path = string.IsNullOrWhiteSpace(parentPath) ? $"/{name}" : $"{parentPath}/{name}";

            var currentIndex = output.Count;
            output.Add(new SceneHierarchyNode
            {
                Name = name,
                Path = path,
                IsActive = current.gameObject.activeSelf,
                Depth = depth,
                ParentIndex = parentIndex,
                SiblingIndex = current.GetSiblingIndex(),
                InstanceId = current.gameObject.GetInstanceID(),
            });

            for (var i = 0; i < current.childCount; i++)
            {
                Traverse(current.GetChild(i), currentIndex, depth + 1, path, output);
            }
        }
    }
}
