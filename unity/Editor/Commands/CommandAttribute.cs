using System;

namespace Scenic.Editor.Commands
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ScenicCommandAttribute : Attribute
    {
        public ScenicCommandAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public bool RequiresExecuteEnabled { get; set; }
    }
}
