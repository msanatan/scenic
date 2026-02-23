using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenic.Editor.Commands.GameObject
{
    [ScenicCommand("gameobject.find")]
    public sealed class GameObjectFindCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = GameObjectFindCommandParams.From(request);
            var scene = ResolveScene(parameters.ScenePath);
            var matches = new List<GameObjectFindItem>();
            var roots = scene.GetRootGameObjects();

            for (var i = 0; i < roots.Length; i++)
            {
                TraverseAndCollect(roots[i].transform, parameters, matches);
            }

            var page = Pagination.Slice(matches, parameters.Paging, out var total);
            return new GameObjectFindCommandResult
            {
                GameObjects = page,
                Total = total,
                Limit = parameters.Paging.Limit,
                Offset = parameters.Paging.Offset,
            };
        }

        private static UnityEngine.SceneManagement.Scene ResolveScene(string scenePath)
        {
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                var active = EditorSceneManager.GetActiveScene();
                if (!active.IsValid())
                {
                    throw new CommandHandlingException("No active scene is loaded.");
                }

                return active;
            }

            var scene = SceneManager.GetSceneByPath(scenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new CommandHandlingException($"Scene is not loaded: {scenePath}");
            }

            return scene;
        }

        private static void TraverseAndCollect(Transform root, GameObjectFindCommandParams parameters, List<GameObjectFindItem> matches)
        {
            var stack = new Stack<Transform>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var currentPath = GameObjectLookup.BuildPath(current);

                if ((parameters.IncludeInactive || current.gameObject.activeSelf) && IsMatch(current, currentPath, parameters.Query))
                {
                    matches.Add(new GameObjectFindItem
                    {
                        Name = current.gameObject.name,
                        Path = currentPath,
                        InstanceId = current.gameObject.GetInstanceID(),
                        IsActive = current.gameObject.activeSelf,
                        ParentPath = current.parent == null ? null : GameObjectLookup.BuildPath(current.parent),
                        SiblingIndex = current.GetSiblingIndex(),
                    });
                }

                for (var i = current.childCount - 1; i >= 0; i--)
                {
                    stack.Push(current.GetChild(i));
                }
            }
        }

        private static bool IsMatch(Transform transform, string path, string query)
        {
            if (query.StartsWith("/", StringComparison.Ordinal))
            {
                return path.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return transform.gameObject.name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                || path.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
