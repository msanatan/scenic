using System;
using System.Collections.Generic;
using UnityEditor.PackageManager;

namespace Scenic.Editor.Commands.Packages
{
    [ScenicCommand("packages.get")]
    public sealed class PackagesGetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = PackagesGetCommandParams.From(request);
            var listRequest = Client.List(offlineMode: true, includeIndirectDependencies: parameters.IncludeIndirect);

            while (!listRequest.IsCompleted)
            {
            }

            if (listRequest.Status == StatusCode.Failure)
            {
                var message = listRequest.Error == null ? "Failed to list Unity packages." : listRequest.Error.message;
                throw new CommandHandlingException(message);
            }

            var items = BuildItems(listRequest.Result, parameters.Search);
            var page = Pagination.Slice(items, parameters.Paging, out var total);

            return new PackagesGetCommandResult
            {
                Packages = page,
                Total = total,
                Limit = parameters.Paging.Limit,
                Offset = parameters.Paging.Offset,
            };
        }

        private static PackageItem[] BuildItems(PackageCollection packages, string search)
        {
            if (packages == null)
            {
                return Array.Empty<PackageItem>();
            }

            var hasSearch = !string.IsNullOrWhiteSpace(search);
            var items = new List<PackageItem>();

            foreach (var packageInfo in packages)
            {
                if (packageInfo == null)
                {
                    continue;
                }

                var name = packageInfo.name ?? string.Empty;
                var displayName = string.IsNullOrWhiteSpace(packageInfo.displayName)
                    ? name
                    : packageInfo.displayName;

                if (hasSearch && !Contains(name, search) && !Contains(displayName, search))
                {
                    continue;
                }

                items.Add(new PackageItem
                {
                    Name = name,
                    DisplayName = displayName,
                    Version = packageInfo.version ?? string.Empty,
                    Source = packageInfo.source.ToString(),
                    IsDirectDependency = packageInfo.isDirectDependency,
                });
            }

            return items.ToArray();
        }

        private static bool Contains(string value, string search)
        {
            return !string.IsNullOrEmpty(value)
                && value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
