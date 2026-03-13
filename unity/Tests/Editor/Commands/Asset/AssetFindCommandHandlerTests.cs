using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Asset;

namespace Scenic.Editor.Tests.Commands.Asset
{
    [TestFixture]
    public class AssetFindCommandHandlerTests
    {
        [Test]
        public void Route_AssetFind_SuccessPath()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-asset-find",
                    Command = "asset.find",
                    ParamsJson = "{}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);

            var result = response.Result as AssetFindCommandResult;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Assets);
            Assert.GreaterOrEqual(result.Total, 0);
            Assert.Greater(result.Limit, 0);
            Assert.GreaterOrEqual(result.Offset, 0);
        }

        [Test]
        public void Route_AssetFind_WithQuery()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-asset-find-query",
                    Command = "asset.find",
                    ParamsJson = "{\"query\": \"t:Scene\", \"limit\": 5}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);

            var result = response.Result as AssetFindCommandResult;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Assets);
            Assert.AreEqual(5, result.Limit);
            foreach (var asset in result.Assets)
            {
                Assert.AreEqual("SceneAsset", asset.Type);
            }
        }

        [Test]
        public void Route_AssetFind_ValidationError()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-asset-find-err",
                    Command = "asset.find",
                    ParamsJson = "{\"limit\": -1}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("limit", response.Error.ToLower());
        }
    }
}
