using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UniBridge.Editor;
using UniBridge.Editor.Commands.GameObject;

namespace UniBridge.Editor.Tests.Commands.GameObject
{
    [TestFixture]
    public class GameObjectGetCommandHandlerTests
    {
        [Test]
        public void Route_GameObjectGet_ByPath_ReturnsSnapshot()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var parent = new UnityEngine.GameObject("Parent");
            var child = new UnityEngine.GameObject("Child");
            child.transform.SetParent(parent.transform, false);
            child.transform.localPosition = new Vector3(1f, 2f, 3f);

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "go-get-path",
                    Command = "gameobject.get",
                    ParamsJson = "{\"path\":\"/Parent/Child\"}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as GameObjectGetCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Child", result.Name);
            Assert.AreEqual("/Parent/Child", result.Path);
            Assert.AreEqual("/Parent", result.ParentPath);
            Assert.AreEqual(child.GetInstanceID(), result.InstanceId);
            Assert.AreEqual(1f, result.Transform.Position.X, 0.0001f);
            Assert.AreEqual(2f, result.Transform.Position.Y, 0.0001f);
            Assert.AreEqual(3f, result.Transform.Position.Z, 0.0001f);

            UnityEngine.Object.DestroyImmediate(parent);
        }

        [Test]
        public void Route_GameObjectGet_ByInstanceId_ReturnsSnapshot()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var target = new UnityEngine.GameObject("Target");
            var instanceId = target.GetInstanceID();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "go-get-id",
                    Command = "gameobject.get",
                    ParamsJson = $"{{\"instanceId\":{instanceId}}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as GameObjectGetCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Target", result.Name);
            Assert.AreEqual("/Target", result.Path);
            Assert.AreEqual(instanceId, result.InstanceId);
            Assert.IsNull(result.ParentPath);

            UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
