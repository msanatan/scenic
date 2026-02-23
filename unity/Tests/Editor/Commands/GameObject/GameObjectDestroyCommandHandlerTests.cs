using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using Scenic.Editor;
using Scenic.Editor.Commands.GameObject;

namespace Scenic.Editor.Tests.Commands.GameObject
{
    [TestFixture]
    public class GameObjectDestroyCommandHandlerTests
    {
        [Test]
        public void Route_GameObjectDestroy_ByPath_RemovesObject()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var target = new UnityEngine.GameObject("DestroyByPath");
            var targetInstanceId = target.GetInstanceID();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "go-destroy-path",
                    Command = "gameobject.destroy",
                    ParamsJson = "{\"path\":\"/DestroyByPath\"}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as GameObjectDestroyCommandResult;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Destroyed);
            Assert.AreEqual("DestroyByPath", result.Name);
            Assert.AreEqual("/DestroyByPath", result.Path);
            Assert.AreEqual(targetInstanceId, result.InstanceId);
            Assert.IsNull(UnityEngine.GameObject.Find("DestroyByPath"));
        }

        [Test]
        public void Route_GameObjectDestroy_ByInstanceId_RemovesObject()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new UnityEngine.GameObject("Root");
            var childA = new UnityEngine.GameObject("Child");
            var childB = new UnityEngine.GameObject("Child");
            childA.transform.SetParent(root.transform, false);
            childB.transform.SetParent(root.transform, false);
            var childAInstanceId = childA.GetInstanceID();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "go-destroy-id",
                    Command = "gameobject.destroy",
                    ParamsJson = $"{{\"instanceId\":{childAInstanceId}}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as GameObjectDestroyCommandResult;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Destroyed);
            Assert.AreEqual(childAInstanceId, result.InstanceId);
            Assert.AreEqual(1, root.transform.childCount);
            Assert.AreEqual("Child", root.transform.GetChild(0).name);

            UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void Route_GameObjectDestroy_AmbiguousPath_ReturnsError()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new UnityEngine.GameObject("Root");
            var childA = new UnityEngine.GameObject("Child");
            var childB = new UnityEngine.GameObject("Child");
            childA.transform.SetParent(root.transform, false);
            childB.transform.SetParent(root.transform, false);

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "go-destroy-ambiguous",
                    Command = "gameobject.destroy",
                    ParamsJson = "{\"path\":\"/Root/Child\"}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            StringAssert.Contains("ambiguous", response.Error.ToLowerInvariant());

            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
