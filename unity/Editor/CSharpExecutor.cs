using System;
using System.IO;
using System.Reflection;
using UniBridge.Editor.Commands;

namespace UniBridge.Editor
{
    public static class CSharpExecutor
    {
        private static bool _initialized;
        private static object _evaluator;
        private static MethodInfo _evaluateMethod;
        private static StringWriter _errorWriter;
        private static object _printer;
        private static PropertyInfo _errorsCountProp;

        public static object Evaluate(string code)
        {
            EnsureInitialized();

            _errorWriter.GetStringBuilder().Clear();
            var errorsBefore = (int)(_errorsCountProp?.GetValue(_printer) ?? 0);

            var args = new object[] { (code ?? string.Empty).Trim(), null, null };
            var partial = _evaluateMethod.Invoke(_evaluator, args) as string;

            var errorsAfter = (int)(_errorsCountProp?.GetValue(_printer) ?? 0);
            if (errorsAfter > errorsBefore)
            {
                throw new CommandHandlingException(_errorWriter.ToString().Trim());
            }

            if (!string.IsNullOrEmpty(partial))
            {
                throw new CommandHandlingException(partial);
            }

            var resultSet = args[2] is bool b && b;
            return resultSet ? Serialize(args[1]) : null;
        }

        public static CommandResponse Execute(string id, string code)
        {
            try
            {
                return CommandResponse.Ok(id, Evaluate(code));
            }
            catch (CommandHandlingException ex)
            {
                return CommandResponse.Fail(id, ex.Message);
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
            _printer = Activator.CreateInstance(reportPrinterType, _errorWriter);
            _errorsCountProp = reportPrinterType.GetProperty("ErrorsCount")
                ?? reportPrinterType.BaseType?.GetProperty("ErrorsCount");
            var settings = Activator.CreateInstance(compilerSettingsType);
            var context = Activator.CreateInstance(compilerContextType, settings, _printer);
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
