using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
#if UNITY_TESTS_FRAMEWORK
using UnityEditor.TestTools.TestRunner.Api;
#endif

namespace UniBridge.Editor.Commands.Test
{
    public static class TestService
    {
        public static TestListItem[] ListTests(TestQueryParams query, out int total)
        {
            var all = DiscoverTests(query.Mode, query.Filter);
            return Pagination.Slice(all, query.Paging, out total);
        }

        public static TestRunSummary RunTests(TestQueryParams query)
        {
#if !UNITY_TESTS_FRAMEWORK
            throw new CommandHandlingException("Unity Test Framework package is required for test.run.");
#else
            if (query.Mode == "play")
            {
                throw new CommandHandlingException("test.run currently supports mode=edit only.");
            }

            var settings = new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                groupNames = BuildGroupNameFilter(query.Filter),
            })
            {
                runSynchronously = true,
            };

            var callback = new RunCallbacks();
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(callback);

            try
            {
                api.Execute(settings);
            }
            finally
            {
                api.UnregisterCallbacks(callback);
                ScriptableObject.DestroyImmediate(api);
            }

            if (callback.RootResult == null)
            {
                throw new CommandHandlingException("No test run result returned from Unity Test Framework.");
            }

            var items = new List<TestRunItem>();
            FlattenRunResults(callback.RootResult, items, query.Filter);

            return new TestRunSummary
            {
                Items = items.ToArray(),
                Passed = callback.RootResult.PassCount,
                Failed = callback.RootResult.FailCount,
                Skipped = callback.RootResult.SkipCount,
                Inconclusive = callback.RootResult.InconclusiveCount,
                DurationMs = (int)Math.Round(callback.RootResult.Duration * 1000.0),
            };
#endif
        }

        private static List<TestListItem> DiscoverTests(string mode, string filter)
        {
            var list = new List<TestListItem>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            DiscoverForMode("edit", mode, filter, list, seen);
            DiscoverForMode("play", mode, filter, list, seen);

            return list;
        }

        private static void DiscoverForMode(
            string entryMode,
            string requestedMode,
            string filter,
            List<TestListItem> target,
            HashSet<string> seen)
        {
            if (!string.IsNullOrWhiteSpace(requestedMode) && !string.Equals(requestedMode, entryMode, StringComparison.Ordinal))
            {
                return;
            }

            var assembliesType = entryMode == "edit"
                ? UnityEditor.Compilation.AssembliesType.Editor
                : UnityEditor.Compilation.AssembliesType.Player;

            var unityAssemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies(assembliesType);
            for (var i = 0; i < unityAssemblies.Length; i++)
            {
                var assemblyInfo = unityAssemblies[i];
                Assembly loadedAssembly;
                try
                {
                    loadedAssembly = Assembly.Load(assemblyInfo.name);
                }
                catch
                {
                    continue;
                }

                if (loadedAssembly == null)
                {
                    continue;
                }

                var types = loadedAssembly.GetTypes();
                for (var t = 0; t < types.Length; t++)
                {
                    var type = types[t];
                    if (!HasAnyNUnitTestMethod(type))
                    {
                        continue;
                    }

                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    for (var m = 0; m < methods.Length; m++)
                    {
                        var method = methods[m];
                        if (!IsNUnitTestMethod(method))
                        {
                            continue;
                        }

                        var fullName = $"{type.FullName}.{method.Name}";
                        if (!MatchesFilter(fullName, filter))
                        {
                            continue;
                        }

                        if (!seen.Add(fullName))
                        {
                            continue;
                        }

                        target.Add(new TestListItem
                        {
                            Name = method.Name,
                            FullName = fullName,
                            Mode = entryMode,
                            Assembly = assemblyInfo.name,
                        });
                    }
                }
            }
        }

        private static bool HasAnyNUnitTestMethod(Type type)
        {
            if (type == null || type.IsAbstract)
            {
                return false;
            }

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            for (var i = 0; i < methods.Length; i++)
            {
                if (IsNUnitTestMethod(methods[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsNUnitTestMethod(MethodInfo method)
        {
            if (method == null)
            {
                return false;
            }

            var attributes = method.GetCustomAttributes(inherit: true);
            for (var i = 0; i < attributes.Length; i++)
            {
                var attributeType = attributes[i].GetType();
                var fullName = attributeType.FullName ?? string.Empty;
                if (fullName == "NUnit.Framework.TestAttribute"
                    || fullName == "NUnit.Framework.TestCaseAttribute"
                    || fullName == "NUnit.Framework.TestCaseSourceAttribute"
                    || fullName == "UnityEngine.TestTools.UnityTestAttribute")
                {
                    return true;
                }
            }

            return false;
        }

        private static bool MatchesFilter(string fullName, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return true;
            }

            return fullName != null
                && fullName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string[] BuildGroupNameFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return null;
            }

            return new[]
            {
                $".*{Regex.Escape(filter)}.*",
            };
        }

#if UNITY_TESTS_FRAMEWORK
        private static void FlattenRunResults(ITestResultAdaptor root, List<TestRunItem> target, string filter)
        {
            if (root == null)
            {
                return;
            }

            if (!root.HasChildren)
            {
                var fullName = root.FullName ?? root.Name;
                if (!MatchesFilter(fullName, filter))
                {
                    return;
                }

                target.Add(new TestRunItem
                {
                    Name = root.Name ?? string.Empty,
                    FullName = fullName ?? string.Empty,
                    Mode = root.Test != null && root.Test.TestMode == TestMode.PlayMode ? "play" : "edit",
                    Status = MapStatus(root.TestStatus),
                    DurationMs = (int)Math.Round(root.Duration * 1000.0),
                    Message = string.IsNullOrWhiteSpace(root.Message) ? null : root.Message,
                    StackTrace = string.IsNullOrWhiteSpace(root.StackTrace) ? null : root.StackTrace,
                });
                return;
            }

            foreach (var child in root.Children)
            {
                FlattenRunResults(child, target, filter);
            }
        }

        private static string MapStatus(TestStatus status)
        {
            switch (status)
            {
                case TestStatus.Passed:
                    return "passed";
                case TestStatus.Failed:
                    return "failed";
                case TestStatus.Skipped:
                    return "skipped";
                default:
                    return "inconclusive";
            }
        }

        private sealed class RunCallbacks : ICallbacks
        {
            public ITestResultAdaptor RootResult;

            public void RunStarted(ITestAdaptor testsToRun)
            {
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                RootResult = result;
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
            }
        }
#endif
    }
}
