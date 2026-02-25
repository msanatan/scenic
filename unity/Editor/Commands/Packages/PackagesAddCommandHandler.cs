namespace Scenic.Editor.Commands.Packages
{
    [ScenicCommand("packages.add")]
    public sealed class PackagesAddCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = PackagesAddCommandParams.From(request);

            var existingPackages = PackagesClient.List(includeIndirect: false);
            var existing = PackagesClient.FindByName(existingPackages, parameters.Name);
            if (existing != null)
            {
                return new PackagesAddCommandResult
                {
                    Package = existing,
                    Added = false,
                    Total = PackagesClient.Count(existingPackages),
                };
            }

            var identifier = BuildIdentifier(parameters.Name, parameters.Version);
            var addedInfo = PackagesClient.Add(identifier);
            var packagesAfterAdd = PackagesClient.List(includeIndirect: false);

            return new PackagesAddCommandResult
            {
                Package = PackagesClient.ToItem(addedInfo),
                Added = true,
                Total = PackagesClient.Count(packagesAfterAdd),
            };
        }

        private static string BuildIdentifier(string name, string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return name;
            }

            return string.Concat(name, "@", version);
        }
    }
}
