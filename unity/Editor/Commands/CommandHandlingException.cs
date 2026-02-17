using System;

namespace UniBridge.Editor.Commands
{
    public sealed class CommandHandlingException : Exception
    {
        public CommandHandlingException(string message)
            : base(message)
        {
        }
    }
}
