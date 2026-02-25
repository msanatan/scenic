using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Scenic.Editor.Commands.Packages
{
    internal static class PackagesClient
    {
        private const int RequestTimeoutMs = 10_000;

        public static PackageCollection List(bool includeIndirect)
        {
            var request = Client.List(offlineMode: true, includeIndirectDependencies: includeIndirect);
            WaitForCompletion(request, "listing Unity packages");
            if (request.Status == StatusCode.Failure)
            {
                var message = request.Error == null ? "Failed to list Unity packages." : request.Error.message;
                throw new CommandHandlingException(message);
            }

            return request.Result;
        }

        public static PackageInfo Add(string identifier)
        {
            var request = Client.Add(identifier);
            WaitForCompletion(request, $"adding Unity package '{identifier}'");
            if (request.Status == StatusCode.Failure)
            {
                var message = request.Error == null ? "Failed to add Unity package." : request.Error.message;
                throw new CommandHandlingException(message);
            }

            return request.Result;
        }

        public static void Remove(string name)
        {
            var request = Client.Remove(name);
            WaitForCompletion(request, $"removing Unity package '{name}'");
            if (request.Status == StatusCode.Failure)
            {
                var message = request.Error == null ? "Failed to remove Unity package." : request.Error.message;
                throw new CommandHandlingException(message);
            }
        }

        public static PackageItem[] BuildItems(PackageCollection packages, string search)
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

                var item = ToItem(packageInfo);
                if (hasSearch && !Contains(item.Name, search) && !Contains(item.DisplayName, search))
                {
                    continue;
                }

                items.Add(item);
            }

            return items.ToArray();
        }

        public static PackageItem ToItem(PackageInfo packageInfo)
        {
            var name = packageInfo == null ? string.Empty : packageInfo.name ?? string.Empty;
            var displayName = packageInfo == null || string.IsNullOrWhiteSpace(packageInfo.displayName)
                ? name
                : packageInfo.displayName;

            return new PackageItem
            {
                Name = name,
                DisplayName = displayName,
                Version = packageInfo == null ? string.Empty : packageInfo.version ?? string.Empty,
                Source = packageInfo == null ? PackageSource.Unknown.ToString() : packageInfo.source.ToString(),
                IsDirectDependency = packageInfo != null && packageInfo.isDirectDependency,
            };
        }

        public static PackageItem MissingItem(string name)
        {
            return new PackageItem
            {
                Name = name,
                DisplayName = name,
                Version = string.Empty,
                Source = PackageSource.Unknown.ToString(),
                IsDirectDependency = false,
            };
        }

        public static PackageItem FindByName(PackageCollection packages, string name)
        {
            if (packages == null)
            {
                return null;
            }

            foreach (var package in packages)
            {
                if (package == null)
                {
                    continue;
                }

                if (string.Equals(package.name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return ToItem(package);
                }
            }

            return null;
        }

        public static int Count(PackageCollection packages)
        {
            if (packages == null)
            {
                return 0;
            }

            var count = 0;
            foreach (var package in packages)
            {
                if (package != null)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool Contains(string value, string search)
        {
            return !string.IsNullOrEmpty(value)
                && value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void WaitForCompletion(Request request, string action)
        {
            var timer = Stopwatch.StartNew();
            while (!request.IsCompleted)
            {
                if (timer.ElapsedMilliseconds > RequestTimeoutMs)
                {
                    throw new CommandHandlingException($"Timed out while {action}.");
                }

                Thread.Sleep(10);
            }
        }
    }
}
