using System.IO;
using NUnit.Framework;
using UniBridge.Editor;

namespace UniBridge.Editor.Tests
{
    [TestFixture]
    public class StateManagerTests
    {
        private string _testDir;

        [SetUp]
        public void SetUp()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "unibridge-test-" + Path.GetRandomFileName());
            Directory.CreateDirectory(_testDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        [Test]
        public void ProjectHash_IsDeterministic()
        {
            var a = StateManager.ProjectHash("/Users/me/MyGame");
            var b = StateManager.ProjectHash("/Users/me/MyGame");
            Assert.AreEqual(a, b);
        }

        [Test]
        public void ProjectHash_DiffersForDifferentPaths()
        {
            var a = StateManager.ProjectHash("/Users/me/GameA");
            var b = StateManager.ProjectHash("/Users/me/GameB");
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void EnsureStateDirectory_CreatesDirectory()
        {
            var stateDir = Path.Combine(_testDir, "state");
            StateManager.EnsureStateDirectory(stateDir);
            Assert.IsTrue(Directory.Exists(stateDir));
        }

        [Test]
        public void WriteServerJson_CreatesValidJson()
        {
            StateManager.WriteServerJson(_testDir, "/Users/me/MyGame");
            var path = Path.Combine(_testDir, "server.json");
            Assert.IsTrue(File.Exists(path));

            var content = File.ReadAllText(path);
            Assert.IsTrue(content.Contains("MyGame"));
        }
    }
}
