using NUnit.Framework;
using UniBridge.Editor;
using UniBridge.Editor.Commands.Layers;

namespace UniBridge.Editor.Tests.Commands.Layers
{
    [TestFixture]
    public class LayersGetCommandHandlerTests
    {
        [Test]
        public void Route_LayersGet_ReturnsPaginatedLayerSlots()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "layers-get",
                    Command = "layers.get",
                    ParamsJson = "{\"limit\":10,\"offset\":0}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as LayersGetCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.Limit);
            Assert.AreEqual(0, result.Offset);
            Assert.AreEqual(32, result.Total);
            Assert.AreEqual(10, result.Layers.Length);

            var first = result.Layers[0];
            Assert.AreEqual(0, first.Index);
            Assert.IsTrue(first.IsBuiltIn);
            Assert.IsFalse(first.IsUserEditable);
            Assert.IsTrue(first.IsOccupied);
        }
    }
}
