using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Scenic.Editor.Commands
{
    public sealed class PaginationParams
    {
        public int Limit;
        public int Offset;

        public static PaginationParams From(CommandRequest request, int defaultLimit = 50, int defaultOffset = 0)
        {
            var limitText = request == null ? null : request.GetStringParam("limit");
            var offsetText = request == null ? null : request.GetStringParam("offset");

            return FromInternal(limitText, offsetText, defaultLimit, defaultOffset);
        }

        public static PaginationParams From(JObject payload, int defaultLimit = 50, int defaultOffset = 0)
        {
            var limitText = CommandModelHelpers.ReadOptionalString(payload, "limit");
            var offsetText = CommandModelHelpers.ReadOptionalString(payload, "offset");

            return FromInternal(limitText, offsetText, defaultLimit, defaultOffset);
        }

        private static PaginationParams FromInternal(
            string limitText,
            string offsetText,
            int defaultLimit,
            int defaultOffset)
        {
            var limit = ParseOrDefault(limitText, defaultLimit, "limit");
            var offset = ParseOrDefault(offsetText, defaultOffset, "offset");

            if (limit <= 0)
            {
                throw new CommandHandlingException("params.limit must be a positive integer.");
            }

            if (offset < 0)
            {
                throw new CommandHandlingException("params.offset must be a non-negative integer.");
            }

            return new PaginationParams
            {
                Limit = limit,
                Offset = offset,
            };
        }

        private static int ParseOrDefault(string value, int defaultValue, string label)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            if (!int.TryParse(value, out var parsed))
            {
                throw new CommandHandlingException($"params.{label} must be an integer.");
            }

            return parsed;
        }
    }

    public static class Pagination
    {
        public static T[] Slice<T>(IReadOnlyList<T> items, PaginationParams paging, out int total)
        {
            return Slice(items, paging.Limit, paging.Offset, out total);
        }

        public static T[] Slice<T>(IReadOnlyList<T> items, int limit, int offset, out int total)
        {
            if (items == null)
            {
                total = 0;
                return Array.Empty<T>();
            }

            total = items.Count;
            if (offset >= total)
            {
                return Array.Empty<T>();
            }

            var pageSize = Math.Min(limit, total - offset);
            var page = new T[pageSize];
            for (var i = 0; i < pageSize; i++)
            {
                page[i] = items[offset + i];
            }

            return page;
        }
    }
}
