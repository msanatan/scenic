using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UniBridge.Editor;
using UniBridge.Editor.Commands.Components;

namespace UniBridge.Editor.Tests.Commands.Components
{
    [TestFixture]
    public class ComponentsListCommandHandlerTests
    {
        [Test]
        public void Route_ComponentsList_ReturnsComponentsWithPagination()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new UnityEngine.GameObject("ListTarget");
            go.AddComponent<BoxCollider>();
            go.AddComponent<Rigidbody>();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "components-list",
                    Command = "components.list",
                    ParamsJson = "{\"path\":\"/ListTarget\",\"limit\":2,\"offset\":0}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as ComponentsListCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Limit);
            Assert.AreEqual(0, result.Offset);
            Assert.GreaterOrEqual(result.Total, 3);
            Assert.AreEqual(2, result.Components.Length);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void Route_ComponentsList_TypeFilter_ReturnsMatchingComponents()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new UnityEngine.GameObject("FilterTarget");
            go.AddComponent<BoxCollider>();
            go.AddComponent<Rigidbody>();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "components-list-filter",
                    Command = "components.list",
                    ParamsJson = "{\"path\":\"/FilterTarget\",\"type\":\"Rigidbody\",\"limit\":20,\"offset\":0}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as ComponentsListCommandResult;
            Assert.IsNotNull(result);
            Assert.GreaterOrEqual(result.Components.Length, 1);
            for (var i = 0; i < result.Components.Length; i++)
            {
                StringAssert.Contains("Rigidbody", result.Components[i].Type);
            }

            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
