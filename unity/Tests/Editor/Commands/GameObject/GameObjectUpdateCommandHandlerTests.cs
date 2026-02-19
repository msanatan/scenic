using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UniBridge.Editor;
using UniBridge.Editor.Commands.GameObject;

namespace UniBridge.Editor.Tests.Commands.GameObject
{
    [TestFixture]
    public class GameObjectUpdateCommandHandlerTests
    {
        [Test]
        public void Route_GameObjectUpdate_UpdatesPropertiesByInstanceId()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var target = new UnityEngine.GameObject("Before");
            var instanceId = target.GetInstanceID();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "go-update-id",
                    Command = "gameobject.update",
                    ParamsJson = $"{{\"instanceId\":{instanceId},\"name\":\"After\",\"tag\":\"EditorOnly\",\"layer\":\"Default\",\"isStatic\":true,\"transform\":{{\"space\":\"local\",\"position\":{{\"x\":1,\"y\":2,\"z\":3}},\"rotation\":{{\"x\":0,\"y\":90,\"z\":0}},\"scale\":{{\"x\":2,\"y\":2,\"z\":2}}}}}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as GameObjectUpdateCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("After", result.Name);
            Assert.AreEqual("EditorOnly", result.Tag);
            Assert.AreEqual("Default", result.Layer);
            Assert.IsTrue(result.IsStatic);
            Assert.AreEqual("/After", result.Path);
            Assert.AreEqual(instanceId, result.InstanceId);
            Assert.AreEqual(1f, result.Transform.Position.X, 0.0001f);
            Assert.AreEqual(2f, result.Transform.Position.Y, 0.0001f);
            Assert.AreEqual(3f, result.Transform.Position.Z, 0.0001f);

            UnityEngine.Object.DestroyImmediate(target);
        }

        [Test]
        public void Route_GameObjectUpdate_WithoutPatchFields_ReturnsError()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var target = new UnityEngine.GameObject("Target");

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "go-update-empty",
                    Command = "gameobject.update",
                    ParamsJson = "{\"path\":\"/Target\"}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            StringAssert.Contains("update field", response.Error.ToLowerInvariant());

            UnityEngine.Object.DestroyImmediate(target);
        }

        [Test]
        public void Route_GameObjectUpdate_UnknownLayer_ReturnsError()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var target = new UnityEngine.GameObject("Target");

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "go-update-layer",
                    Command = "gameobject.update",
                    ParamsJson = "{\"path\":\"/Target\",\"layer\":\"UnknownLayerName\"}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            StringAssert.Contains("unknown layer", response.Error.ToLowerInvariant());

            UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
