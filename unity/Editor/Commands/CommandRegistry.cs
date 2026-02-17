using System;
using System.Collections.Generic;
using System.Reflection;

namespace UniBridge.Editor.Commands
{
    internal sealed class CommandRegistration
    {
        public string Name;
        public bool RequiresExecuteEnabled;
        public ICommandHandler Handler;
    }

    public static class CommandRegistry
    {
        private static readonly Dictionary<string, CommandRegistration> _commands =
            new Dictionary<string, CommandRegistration>(StringComparer.OrdinalIgnoreCase);

        static CommandRegistry()
        {
            RegisterFromAssembly(typeof(CommandRegistry).Assembly);
        }

        internal static bool TryResolve(string name, out CommandRegistration registration)
        {
            return _commands.TryGetValue(name ?? string.Empty, out registration);
        }

        internal static bool RequiresExecuteEnabled(string name)
        {
            return TryResolve(name, out var registration) && registration.RequiresExecuteEnabled;
        }

        private static void RegisterFromAssembly(Assembly assembly)
        {
            var types = assembly.GetTypes();
            for (var i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (type == null || type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                if (!typeof(ICommandHandler).IsAssignableFrom(type))
                {
                    continue;
                }

                var attribute = type.GetCustomAttribute<UniBridgeCommandAttribute>();
                if (attribute == null || string.IsNullOrWhiteSpace(attribute.Name))
                {
                    continue;
                }

                if (!(Activator.CreateInstance(type) is ICommandHandler handler))
                {
                    continue;
                }

                _commands[attribute.Name] = new CommandRegistration
                {
                    Name = attribute.Name,
                    RequiresExecuteEnabled = attribute.RequiresExecuteEnabled,
                    Handler = handler,
                };
            }
        }
    }
}
