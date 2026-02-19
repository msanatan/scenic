using UniBridge.Editor.Commands;

namespace UniBridge.Editor.Commands.Components
{
    internal static class ComponentCommandParamsHelpers
    {
        public static void ValidateSelector(int? componentInstanceId, int? index, string type)
        {
            var hasType = !string.IsNullOrWhiteSpace(type);
            var selectors = (componentInstanceId.HasValue ? 1 : 0) + (index.HasValue ? 1 : 0) + (hasType ? 1 : 0);
            if (selectors != 1)
            {
                throw new CommandHandlingException("Provide exactly one selector: params.componentInstanceId, params.index, or params.type.");
            }

            if (index.HasValue && index.Value < 0)
            {
                throw new CommandHandlingException("params.index must be a non-negative integer.");
            }
        }
    }
}
