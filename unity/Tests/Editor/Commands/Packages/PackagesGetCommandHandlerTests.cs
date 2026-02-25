using System;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Packages;

namespace Scenic.Editor.Tests.Commands.Packages
{
    [TestFixture]
    public class PackagesGetCommandHandlerTests
    {
        [Test]
        public void Route_PackagesGet_ReturnsPaginatedPackages()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "packages-get",
                    Command = "packages.get",
                    ParamsJson = "{\"limit\":5,\"offset\":0,\"includeIndirect\":true}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as PackagesGetCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Limit);
            Assert.AreEqual(0, result.Offset);
            Assert.Greater(result.Total, 0);
            Assert.LessOrEqual(result.Packages.Length, 5);

            if (result.Packages.Length > 0)
            {
                var first = result.Packages[0];
                Assert.IsFalse(string.IsNullOrWhiteSpace(first.Name));
                Assert.IsFalse(string.IsNullOrWhiteSpace(first.DisplayName));
                Assert.IsFalse(string.IsNullOrWhiteSpace(first.Version));
                Assert.IsFalse(string.IsNullOrWhiteSpace(first.Source));
            }
        }

        [Test]
        public void Route_PackagesGet_IncludeIndirectExpandsOrMatchesTotal()
        {
            var directResponse = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "packages-direct",
                    Command = "packages.get",
                    ParamsJson = "{\"limit\":500,\"offset\":0,\"includeIndirect\":false}",
                },
                executeEnabled: true);

            var indirectResponse = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "packages-indirect",
                    Command = "packages.get",
                    ParamsJson = "{\"limit\":500,\"offset\":0,\"includeIndirect\":true}",
                },
                executeEnabled: true);

            Assert.IsTrue(directResponse.Success);
            Assert.IsTrue(indirectResponse.Success);

            var direct = directResponse.Result as PackagesGetCommandResult;
            var indirect = indirectResponse.Result as PackagesGetCommandResult;
            Assert.IsNotNull(direct);
            Assert.IsNotNull(indirect);
            Assert.GreaterOrEqual(indirect.Total, direct.Total);
        }

        [Test]
        public void Route_PackagesGet_SearchFiltersByNameOrDisplayName()
        {
            var baselineResponse = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "packages-baseline",
                    Command = "packages.get",
                    ParamsJson = "{\"limit\":200,\"offset\":0,\"includeIndirect\":true}",
                },
                executeEnabled: true);

            Assert.IsTrue(baselineResponse.Success);
            var baseline = baselineResponse.Result as PackagesGetCommandResult;
            Assert.IsNotNull(baseline);
            Assert.Greater(baseline.Packages.Length, 0);

            var seed = baseline.Packages[0];
            var seedText = string.IsNullOrWhiteSpace(seed.Name) ? seed.DisplayName : seed.Name;
            Assert.IsFalse(string.IsNullOrWhiteSpace(seedText));
            var search = seedText.Substring(0, Math.Min(4, seedText.Length)).Replace("\"", "\\\"");

            var filteredResponse = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "packages-search",
                    Command = "packages.get",
                    ParamsJson = $"{{\"search\":\"{search}\",\"limit\":200,\"offset\":0,\"includeIndirect\":true}}",
                },
                executeEnabled: true);

            Assert.IsTrue(filteredResponse.Success);
            var filtered = filteredResponse.Result as PackagesGetCommandResult;
            Assert.IsNotNull(filtered);
            Assert.Greater(filtered.Total, 0);

            for (var i = 0; i < filtered.Packages.Length; i++)
            {
                var package = filtered.Packages[i];
                var matchesName = package.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
                var matchesDisplayName = package.DisplayName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
                Assert.IsTrue(matchesName || matchesDisplayName);
            }
        }

        [Test]
        public void Route_PackagesAdd_WhenPackageAlreadyInstalled_ReturnsAddedFalse()
        {
            var baselineResponse = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "packages-add-baseline",
                    Command = "packages.get",
                    ParamsJson = "{\"limit\":200,\"offset\":0,\"includeIndirect\":false}",
                },
                executeEnabled: true);

            Assert.IsTrue(baselineResponse.Success);
            var baseline = baselineResponse.Result as PackagesGetCommandResult;
            Assert.IsNotNull(baseline);
            Assert.Greater(baseline.Packages.Length, 0);

            var existingName = baseline.Packages[0].Name;
            var addResponse = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "packages-add-existing",
                    Command = "packages.add",
                    ParamsJson = $"{{\"name\":\"{existingName}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(addResponse.Success);
            var result = addResponse.Result as PackagesAddCommandResult;
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Added);
            Assert.AreEqual(existingName, result.Package.Name);
            Assert.Greater(result.Total, 0);
        }

        [Test]
        public void Route_PackagesRemove_WhenPackageMissing_ReturnsRemovedFalse()
        {
            var missingName = $"com.scenic.missing.{Guid.NewGuid():N}";
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "packages-remove-missing",
                    Command = "packages.remove",
                    ParamsJson = $"{{\"name\":\"{missingName}\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as PackagesRemoveCommandResult;
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Removed);
            Assert.AreEqual(missingName, result.Package.Name);
            Assert.AreEqual(missingName, result.Package.DisplayName);
            Assert.GreaterOrEqual(result.Total, 0);
        }
    }
}
