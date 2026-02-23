using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Test;

namespace Scenic.Editor.Tests.Commands.Test
{
    [TestFixture]
    public class TestListCommandHandlerTests
    {
        [Test]
        public void Route_TestList_ReturnsPaginatedTests()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-list",
                    Command = "test.list",
                    ParamsJson = "{\"mode\":\"edit\",\"limit\":10,\"offset\":0}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as TestListResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.Limit);
            Assert.AreEqual(0, result.Offset);
            Assert.GreaterOrEqual(result.Total, 0);
            Assert.IsNotNull(result.Tests);
        }
    }
}
