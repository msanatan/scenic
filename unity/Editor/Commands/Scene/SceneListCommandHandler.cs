using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UniBridge.Editor.Commands;

namespace UniBridge.Editor.Commands.Scene
{
    [UniBridgeCommand("scene.list")]
    public sealed class SceneListCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = SceneListCommandParams.From(request);
            var allScenes = DiscoverScenes(parameters.Filter);
            var page = Pagination.Slice(allScenes, parameters.Paging, out var total);

            return new SceneListCommandResult
            {
                Scenes = page,
                Total = total,
                Limit = parameters.Paging.Limit,
                Offset = parameters.Paging.Offset,
            };
        }

        private static List<SceneListItem> DiscoverScenes(string filter)
        {
            var guids = AssetDatabase.FindAssets("t:Scene");
            var scenes = new List<SceneListItem>(guids.Length);

            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(filter)
                    && path.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                scenes.Add(new SceneListItem
                {
                    Name = Path.GetFileNameWithoutExtension(path) ?? string.Empty,
                    Path = path,
                });
            }

            scenes.Sort((a, b) => string.Compare(a.Path, b.Path, StringComparison.OrdinalIgnoreCase));
            return scenes;
        }
    }
}
