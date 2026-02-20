using System.Linq;
using NUnit.Framework;
using UniBridge.Editor;
using UniBridge.Editor.Commands.Tags;

namespace UniBridge.Editor.Tests.Commands.Tags
{
    [TestFixture]
    public class TagsGetCommandHandlerTests
    {
        [Test]
        public void Route_TagsGet_ReturnsTagsWithBuiltInMarkers()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "tags-get",
                    Command = "tags.get",
                    ParamsJson = "{}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            Assert.AreEqual("tags-get", response.Id);

            var result = response.Result as TagsGetCommandResult;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Tags);
            Assert.AreEqual(result.Total, result.Tags.Length);
            Assert.Greater(result.Total, 0);

            var untagged = result.Tags.FirstOrDefault(tag => tag.Name == "Untagged");
            Assert.IsNotNull(untagged);
            Assert.IsTrue(untagged.IsBuiltIn);
        }
    }
}
