using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UniBridge.Editor;
using UniBridge.Editor.Commands.GameObject;

namespace UniBridge.Editor.Tests.Commands.GameObject
{
    [TestFixture]
    public class GameObjectFindCommandHandlerTests
    {
        [Test]
        public void Route_GameObjectFind_ByName_ReturnsMatchesWithPagination()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new UnityEngine.GameObject("EnemyRoot");
            var childA = new UnityEngine.GameObject("EnemyA");
            childA.transform.SetParent(root.transform, false);
            var childB = new UnityEngine.GameObject("EnemyB");
            childB.transform.SetParent(root.transform, false);
            var unrelated = new UnityEngine.GameObject("Player");

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "go-find-name",
                    Command = "gameobject.find",
                    ParamsJson = "{\"query\":\"Enemy\",\"limit\":2,\"offset\":0}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as GameObjectFindCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Limit);
            Assert.AreEqual(0, result.Offset);
            Assert.GreaterOrEqual(result.Total, 3);
            Assert.AreEqual(2, result.GameObjects.Length);

            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(unrelated);
        }

        [Test]
        public void Route_GameObjectFind_IncludeInactive_IncludesInactiveMatches()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var active = new UnityEngine.GameObject("SearchTargetActive");
            var inactive = new UnityEngine.GameObject("SearchTargetInactive");
            inactive.SetActive(false);

            var excludedResponse = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "go-find-excluded",
                    Command = "gameobject.find",
                    ParamsJson = "{\"query\":\"SearchTarget\",\"limit\":10,\"offset\":0}",
                },
                executeEnabled: true);
            Assert.IsTrue(excludedResponse.Success);
            var excluded = excludedResponse.Result as GameObjectFindCommandResult;
            Assert.IsNotNull(excluded);
            Assert.AreEqual(1, excluded.Total);

            var includedResponse = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "go-find-included",
                    Command = "gameobject.find",
                    ParamsJson = "{\"query\":\"SearchTarget\",\"includeInactive\":true,\"limit\":10,\"offset\":0}",
                },
                executeEnabled: true);
            Assert.IsTrue(includedResponse.Success);
            var included = includedResponse.Result as GameObjectFindCommandResult;
            Assert.IsNotNull(included);
            Assert.GreaterOrEqual(included.Total, 2);

            UnityEngine.Object.DestroyImmediate(active);
            UnityEngine.Object.DestroyImmediate(inactive);
        }
    }
}
