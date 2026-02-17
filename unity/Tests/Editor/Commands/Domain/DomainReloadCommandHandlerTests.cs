using NUnit.Framework;
using UniBridge.Editor;
using UniBridge.Editor.Commands.Domain;

namespace UniBridge.Editor.Tests.Commands.Domain
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
