using System;

namespace UniBridge.Editor.Commands
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class UniBridgeCommandAttribute : Attribute
    {
        public UniBridgeCommandAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public bool RequiresExecuteEnabled { get; set; }
    }
}
