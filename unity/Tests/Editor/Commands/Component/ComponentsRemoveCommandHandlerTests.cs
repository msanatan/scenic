using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using Scenic.Editor;
using Scenic.Editor.Commands.Components;

namespace Scenic.Editor.Tests.Commands.Components
{
    [TestFixture]
    public class ComponentsRemoveCommandHandlerTests
    {
        [Test]
        public void Route_ComponentsRemove_ByInstanceId_RemovesComponent()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new UnityEngine.GameObject("RemoveTarget");
            var rb = go.AddComponent<Rigidbody>();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "components-remove-by-id",
                    Command = "components.remove",
                    ParamsJson = "{\"path\":\"/RemoveTarget\",\"componentInstanceId\":" + rb.GetInstanceID() + "}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as ComponentsRemoveCommandResult;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Removed);
            Assert.AreEqual(rb.GetInstanceID(), result.InstanceId);
            StringAssert.Contains("Rigidbody", result.Type);
            Assert.IsNull(go.GetComponent<Rigidbody>());

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void Route_ComponentsRemove_Transform_ReturnsError()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new UnityEngine.GameObject("RemoveTransformTarget");

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "components-remove-transform",
                    Command = "components.remove",
                    ParamsJson = "{\"path\":\"/RemoveTransformTarget\",\"index\":0}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            StringAssert.Contains("cannot remove transform", response.Error.ToLowerInvariant());

            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
