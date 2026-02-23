using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Domain;

namespace Scenic.Editor.Tests.Commands.Domain
{
    [TestFixture]
    public class DomainReloadCommandHandlerTests
    {
        [Test]
        public void Route_DomainReload_ReturnsTriggeredTrue()
        {
            var response = CommandRouter.Route(CreateRequest("domain-reload-ok"), executeEnabled: true);
            Assert.IsTrue(response.Success);
            Assert.AreEqual("domain-reload-ok", response.Id);
            var result = response.Result as DomainReloadCommandResult;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Triggered);
        }

        private static CommandRequest CreateRequest(string id)
        {
            return new CommandRequest
            {
                Id = id,
                Command = "domain.reload",
                ParamsJson = "{}",
            };
        }
    }
}
