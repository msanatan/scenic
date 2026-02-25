namespace Scenic.Editor.Commands.Packages
{
    [ScenicCommand("packages.remove")]
    public sealed class PackagesRemoveCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = PackagesRemoveCommandParams.From(request);

            var existingPackages = PackagesClient.List(includeIndirect: false);
            var existing = PackagesClient.FindByName(existingPackages, parameters.Name);
            if (existing == null)
            {
                return new PackagesRemoveCommandResult
                {
                    Package = PackagesClient.MissingItem(parameters.Name),
                    Removed = false,
                    Total = PackagesClient.Count(existingPackages),
                };
            }

            PackagesClient.Remove(parameters.Name);
            var packagesAfterRemove = PackagesClient.List(includeIndirect: false);

            return new PackagesRemoveCommandResult
            {
                Package = existing,
                Removed = true,
                Total = PackagesClient.Count(packagesAfterRemove),
            };
        }
    }
}
