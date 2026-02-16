using System;
using System.IO;
using System.Reflection;

namespace UniBridge.Editor
{
    public static class CSharpExecutor
    {
        private static bool _initialized;
        private static object _evaluator;
        private static MethodInfo _evaluateMethod;
        private static StringWriter _errorWriter;

        public static CommandResponse Execute(string id, string code)
        {
            try
            {
                EnsureInitialized();

                _errorWriter.GetStringBuilder().Clear();

                var args = new object[] { (code ?? string.Empty).Trim(), null, null };
                var partial = _evaluateMethod.Invoke(_evaluator, args) as string;

                var compilationErrors = _errorWriter.ToString().Trim();
                if (!string.IsNullOrEmpty(compilationErrors))
                {
                    return CommandResponse.Fail(id, compilationErrors);
                }

                if (!string.IsNullOrEmpty(partial))
                {
                    return CommandResponse.Fail(id, partial);
                }

                var resultSet = args[2] is bool b && b;
                var value = resultSet ? Serialize(args[1]) : null;
                return CommandResponse.Ok(id, value);
            }
            catch (Exception ex)
            {
                return CommandResponse.Fail(id, ex.Message);
            }
        }

        public static CommandResponse Execute(string code)
        {
            return Execute(Guid.NewGuid().ToString("n"), code);
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            var monoAssembly = Assembly.Load("Mono.CSharp");

            var compilerSettingsType = monoAssembly.GetType("Mono.CSharp.CompilerSettings");
            var reportPrinterType = monoAssembly.GetType("Mono.CSharp.StreamReportPrinter");
            var compilerContextType = monoAssembly.GetType("Mono.CSharp.CompilerContext");
            var evaluatorType = monoAssembly.GetType("Mono.CSharp.Evaluator");

            _errorWriter = new StringWriter();
            var printer = Activator.CreateInstance(reportPrinterType, _errorWriter);
            var settings = Activator.CreateInstance(compilerSettingsType);
            var context = Activator.CreateInstance(compilerContextType, settings, printer);
            _evaluator = Activator.CreateInstance(evaluatorType, context);

            var referenceAssemblyMethod = evaluatorType.GetMethod("ReferenceAssembly", new[] { typeof(Assembly) });
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    referenceAssemblyMethod?.Invoke(_evaluator, new object[] { assembly });
                }
                catch
                {
                    // Ignore assemblies that cannot be referenced.
                }
            }

            _evaluateMethod = evaluatorType.GetMethod("Evaluate", new[]
            {
                typeof(string),
                typeof(object).MakeByRefType(),
                typeof(bool).MakeByRefType(),
            });

            if (_evaluateMethod == null)
            {
                throw new InvalidOperationException("Mono.CSharp.Evaluator.Evaluate method not found.");
            }

            _initialized = true;
        }

        private static object Serialize(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is string || value is bool || value is int || value is long || value is float || value is double || value is decimal)
            {
                return value;
            }

            return value.ToString();
        }
    }
}
