using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using Scenic.Editor;
using Scenic.Editor.Commands.GameObject;

namespace Scenic.Editor.Tests.Commands.GameObject
{
    [TestFixture]
    public class GameObjectCreateCommandHandlerTests
    {
        [Test]
        public void Route_GameObjectCreate_2d_AddsSpriteRenderer()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "go-create-2d",
                    Command = "gameobject.create",
                    ParamsJson = "{\"name\":\"Player2D\",\"dimension\":\"2d\"}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as GameObjectCreateCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("/Player2D", result.Path);
            Assert.AreNotEqual(0, result.InstanceId);

            var created = UnityEngine.GameObject.Find("Player2D");
            Assert.IsNotNull(created);
            Assert.IsNotNull(created.GetComponent<SpriteRenderer>());
            UnityEngine.Object.DestroyImmediate(created);
        }

        [Test]
        public void Route_GameObjectCreate_3dPrimitive_WithParentAndLocalTransform()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var parent = new UnityEngine.GameObject("World");
            var parentInstanceId = parent.GetInstanceID();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "go-create-3d",
                    Command = "gameobject.create",
                    ParamsJson = $"{{\"name\":\"Floor\",\"parentInstanceId\":{parentInstanceId},\"primitive\":\"cube\",\"transform\":{{\"space\":\"local\",\"position\":{{\"x\":1,\"y\":2,\"z\":3}},\"rotation\":{{\"x\":0,\"y\":45,\"z\":0}},\"scale\":{{\"x\":2,\"y\":1,\"z\":2}}}}}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as GameObjectCreateCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("/World/Floor", result.Path);
            Assert.AreNotEqual(0, result.InstanceId);

            var created = UnityEngine.GameObject.Find("Floor");
            Assert.IsNotNull(created);
            Assert.AreEqual(parent.transform, created.transform.parent);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), created.transform.localPosition);
            Assert.AreEqual(new Vector3(2f, 1f, 2f), created.transform.localScale);

            UnityEngine.Object.DestroyImmediate(parent);
        }
    }
}
