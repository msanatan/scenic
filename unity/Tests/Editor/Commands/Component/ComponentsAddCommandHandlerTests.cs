using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UniBridge.Editor;
using UniBridge.Editor.Commands.Components;

namespace UniBridge.Editor.Tests.Commands.Components
{
    [TestFixture]
    public class ComponentsAddCommandHandlerTests
    {
        [Test]
        public void Route_ComponentsAdd_AddsComponent()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new UnityEngine.GameObject("AddTarget");

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "components-add",
                    Command = "components.add",
                    ParamsJson = "{\"path\":\"/AddTarget\",\"type\":\"UnityEngine.Rigidbody\"}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as ComponentsAddCommandResult;
            Assert.IsNotNull(result);
            StringAssert.Contains("Rigidbody", result.Type);
            Assert.AreNotEqual(0, result.InstanceId);
            Assert.IsNotNull(go.GetComponent<Rigidbody>());

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void Route_ComponentsAdd_AppliesInitialValues()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new UnityEngine.GameObject("AddInitTarget");

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "components-add-initial-values",
                    Command = "components.add",
                    ParamsJson = "{\"path\":\"/AddInitTarget\",\"type\":\"UnityEngine.Rigidbody\",\"initialValues\":{\"mass\":5.5,\"useGravity\":false}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as ComponentsAddCommandResult;
            Assert.IsNotNull(result);
            CollectionAssert.Contains(result.AppliedFields, "mass");
            CollectionAssert.Contains(result.AppliedFields, "useGravity");

            var rb = go.GetComponent<Rigidbody>();
            Assert.IsNotNull(rb);
            Assert.AreEqual(5.5f, rb.mass, 0.0001f);
            Assert.IsFalse(rb.useGravity);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void Route_ComponentsAdd_StrictUnknownField_ReturnsError()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new UnityEngine.GameObject("AddStrictTarget");

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "components-add-strict",
                    Command = "components.add",
                    ParamsJson = "{\"path\":\"/AddStrictTarget\",\"type\":\"UnityEngine.Rigidbody\",\"strict\":true,\"initialValues\":{\"doesNotExist\":1}}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            StringAssert.Contains("unknown initialvalues fields", response.Error.ToLowerInvariant());

            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
