using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using Scenic.Editor;
using Scenic.Editor.Commands.Scene;

namespace Scenic.Editor.Tests.Commands.Scene
{
    [TestFixture]
    public class SceneHierarchyCommandHandlerTests
    {
        [Test]
        public void Route_SceneHierarchy_ReturnsFlattenedTreeWithParentChildLinks()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new UnityEngine.GameObject("Root");
            var childA = new UnityEngine.GameObject("Child");
            childA.transform.SetParent(root.transform, false);
            var childB = new UnityEngine.GameObject("Child");
            childB.transform.SetParent(root.transform, false);
            childB.SetActive(false);
            var grandchild = new UnityEngine.GameObject("Leaf");
            grandchild.transform.SetParent(childA.transform, false);

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "scene-hierarchy",
                    Command = "scene.hierarchy",
                    ParamsJson = "{\"limit\":100,\"offset\":0}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as SceneHierarchyCommandResult;
            Assert.IsNotNull(result);
            Assert.GreaterOrEqual(result.Total, 4);
            Assert.IsNotNull(result.Nodes);

            var rootIndex = FindIndexByPath(result.Nodes, "/Root");
            Assert.GreaterOrEqual(rootIndex, 0);
            Assert.AreEqual(-1, result.Nodes[rootIndex].ParentIndex);
            Assert.AreEqual(0, result.Nodes[rootIndex].Depth);
            Assert.AreNotEqual(0, result.Nodes[rootIndex].InstanceId);

            var childAIndex = FindIndexByPath(result.Nodes, "/Root/Child");
            Assert.GreaterOrEqual(childAIndex, 0);
            Assert.AreEqual(rootIndex, result.Nodes[childAIndex].ParentIndex);
            Assert.AreEqual(1, result.Nodes[childAIndex].Depth);
            Assert.AreEqual(0, result.Nodes[childAIndex].SiblingIndex);
            Assert.IsTrue(result.Nodes[childAIndex].IsActive);
            Assert.AreNotEqual(0, result.Nodes[childAIndex].InstanceId);

            var childBIndex = FindIndexByPathWithSibling(result.Nodes, "/Root/Child", 1);
            Assert.GreaterOrEqual(childBIndex, 0);
            Assert.AreEqual(rootIndex, result.Nodes[childBIndex].ParentIndex);
            Assert.AreEqual(1, result.Nodes[childBIndex].Depth);
            Assert.IsFalse(result.Nodes[childBIndex].IsActive);
            Assert.AreNotEqual(0, result.Nodes[childBIndex].InstanceId);

            var leafIndex = FindIndexByPath(result.Nodes, "/Root/Child/Leaf");
            Assert.GreaterOrEqual(leafIndex, 0);
            Assert.AreEqual(childAIndex, result.Nodes[leafIndex].ParentIndex);
            Assert.AreEqual(2, result.Nodes[leafIndex].Depth);
            Assert.AreNotEqual(0, result.Nodes[leafIndex].InstanceId);

            UnityEngine.Object.DestroyImmediate(root);
        }

        private static int FindIndexByPath(SceneHierarchyNode[] nodes, string path)
        {
            for (var i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].Path == path)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindIndexByPathWithSibling(SceneHierarchyNode[] nodes, string path, int siblingIndex)
        {
            for (var i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].Path == path && nodes[i].SiblingIndex == siblingIndex)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
