using System;

namespace Scenic.Editor.Commands
{
    public sealed class CommandHandlingException : Exception
    {
        public CommandHandlingException(string message)
            : base(message)
        {
        }
    }
}
