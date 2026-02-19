using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UniBridge.Editor;
using UniBridge.Editor.Commands.Components;

namespace UniBridge.Editor.Tests.Commands.Components
{
    [TestFixture]
    public class ComponentsUpdateCommandHandlerTests
    {
        [Test]
        public void Route_ComponentsUpdate_UpdatesFields()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new UnityEngine.GameObject("UpdateTarget");
            var rb = go.AddComponent<Rigidbody>();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "components-update",
                    Command = "components.update",
                    ParamsJson = "{\"path\":\"/UpdateTarget\",\"componentInstanceId\":" + rb.GetInstanceID() + ",\"values\":{\"mass\":9.5,\"useGravity\":false}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as ComponentsUpdateCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(rb.GetInstanceID(), result.InstanceId);
            CollectionAssert.Contains(result.AppliedFields, "mass");
            CollectionAssert.Contains(result.AppliedFields, "useGravity");

            Assert.AreEqual(9.5f, rb.mass, 0.0001f);
            Assert.IsFalse(rb.useGravity);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void Route_ComponentsUpdate_StrictUnknownField_ReturnsError()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new UnityEngine.GameObject("UpdateStrictTarget");
            var rb = go.AddComponent<Rigidbody>();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "components-update-strict",
                    Command = "components.update",
                    ParamsJson = "{\"path\":\"/UpdateStrictTarget\",\"componentInstanceId\":" + rb.GetInstanceID() + ",\"strict\":true,\"values\":{\"doesNotExist\":1}}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            StringAssert.Contains("unknown values fields", response.Error.ToLowerInvariant());

            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
