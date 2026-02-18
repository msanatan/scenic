using System;
using System.Collections.Generic;
using UniBridge.Editor.Commands;
using UnityEditor;
using UnityEngine;

namespace UniBridge.Editor.Commands.Logs
{
    [InitializeOnLoad]
    public static class LogBuffer
    {
        private const int MaxEntries = 10000;
        private static readonly object Gate = new object();
        private static readonly List<LogEntry> Entries = new List<LogEntry>(1024);

        static LogBuffer()
        {
            Application.logMessageReceivedThreaded += OnLogMessage;
        }

        private static void OnLogMessage(string condition, string stackTrace, LogType type)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTimeOffset.UtcNow.ToString("o"),
                Severity = SeverityFrom(type),
                Message = condition ?? string.Empty,
                StackTrace = stackTrace ?? string.Empty,
            };

            lock (Gate)
            {
                Entries.Add(entry);
                var overflow = Entries.Count - MaxEntries;
                if (overflow > 0)
                {
                    Entries.RemoveRange(0, overflow);
                }
            }
        }

        public static LogEntry[] Query(string severity, PaginationParams paging, out int total)
        {
            var filtered = new List<LogEntry>();

            lock (Gate)
            {
                for (var i = Entries.Count - 1; i >= 0; i--)
                {
                    var entry = Entries[i];
                    if (!string.IsNullOrWhiteSpace(severity) && !string.Equals(entry.Severity, severity, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    filtered.Add(entry);
                }
            }

            return Pagination.Slice(filtered, paging, out total);
        }

        private static string SeverityFrom(LogType type)
        {
            switch (type)
            {
                case LogType.Warning:
                    return "warn";
                case LogType.Assert:
                case LogType.Error:
                case LogType.Exception:
                    return "error";
                default:
                    return "info";
            }
        }
    }
}
