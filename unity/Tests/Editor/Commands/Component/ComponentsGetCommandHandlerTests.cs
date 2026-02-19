using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UniBridge.Editor;
using UniBridge.Editor.Commands.Components;

namespace UniBridge.Editor.Tests.Commands.Components
{
    [TestFixture]
    public class ComponentsGetCommandHandlerTests
    {
        [Test]
        public void Route_ComponentsGet_ByInstanceId_ReturnsSerializedComponent()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new UnityEngine.GameObject("GetTarget");
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 7.25f;
            rb.useGravity = false;

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "components-get-by-id",
                    Command = "components.get",
                    ParamsJson = "{\"path\":\"/GetTarget\",\"componentInstanceId\":" + rb.GetInstanceID() + "}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as ComponentsGetCommandResult;
            Assert.IsNotNull(result);
            StringAssert.Contains("Rigidbody", result.Component.Type);
            Assert.AreEqual(rb.GetInstanceID(), result.Component.InstanceId);
            Assert.IsNotNull(result.Component.Serialized);
            StringAssert.Contains("\"serialized\":", response.ToJson());

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void Route_ComponentsGet_ByType_WithMultipleMatches_ReturnsError()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new UnityEngine.GameObject("GetTypeTarget");
            go.AddComponent<BoxCollider>();
            go.AddComponent<SphereCollider>();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "components-get-ambiguous-type",
                    Command = "components.get",
                    ParamsJson = "{\"path\":\"/GetTypeTarget\",\"type\":\"Collider\"}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            StringAssert.Contains("multiple components matched type", response.Error.ToLowerInvariant());

            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
