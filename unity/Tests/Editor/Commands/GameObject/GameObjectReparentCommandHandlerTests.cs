using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UniBridge.Editor;
using UniBridge.Editor.Commands.GameObject;

namespace UniBridge.Editor.Tests.Commands.GameObject
{
    [TestFixture]
    public class GameObjectReparentCommandHandlerTests
    {
        [Test]
        public void Route_GameObjectReparent_ToNewParent_UpdatesHierarchy()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var parentA = new UnityEngine.GameObject("ParentA");
            var parentB = new UnityEngine.GameObject("ParentB");
            var child = new UnityEngine.GameObject("Child");
            child.transform.SetParent(parentA.transform, false);
            var childInstanceId = child.GetInstanceID();

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "go-reparent-parent",
                    Command = "gameobject.reparent",
                    ParamsJson = $"{{\"instanceId\":{childInstanceId},\"parentPath\":\"/ParentB\"}}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as GameObjectReparentCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("/ParentB/Child", result.Path);
            Assert.AreEqual("/ParentB", result.ParentPath);
            Assert.AreEqual(childInstanceId, result.InstanceId);
            Assert.AreEqual(parentB.transform, child.transform.parent);

            UnityEngine.Object.DestroyImmediate(parentA);
            UnityEngine.Object.DestroyImmediate(parentB);
        }

        [Test]
        public void Route_GameObjectReparent_ToRoot_ClearsParent()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var parent = new UnityEngine.GameObject("Parent");
            var child = new UnityEngine.GameObject("Child");
            child.transform.SetParent(parent.transform, false);

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "go-reparent-root",
                    Command = "gameobject.reparent",
                    ParamsJson = "{\"path\":\"/Parent/Child\",\"toRoot\":true}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as GameObjectReparentCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("/Child", result.Path);
            Assert.IsNull(result.ParentPath);
            Assert.IsNull(child.transform.parent);

            UnityEngine.Object.DestroyImmediate(parent);
            UnityEngine.Object.DestroyImmediate(child);
        }

        [Test]
        public void Route_GameObjectReparent_UnderDescendant_ReturnsError()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new UnityEngine.GameObject("Root");
            var child = new UnityEngine.GameObject("Child");
            child.transform.SetParent(root.transform, false);

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "go-reparent-cycle",
                    Command = "gameobject.reparent",
                    ParamsJson = "{\"path\":\"/Root\",\"parentPath\":\"/Root/Child\"}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            StringAssert.Contains("descendant", response.Error.ToLowerInvariant());

            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
