using System.IO;
using NUnit.Framework;
using UniBridge.Editor;

namespace UniBridge.Tests.Editor
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
        public void WriteResult_CreatesJsonFile()
        {
            StateManager.EnsureStateDirectory(_testDir);
            var response = new CommandResponse { Id = "cmd-1", Success = true, Result = "hello" };
            StateManager.WriteResult(_testDir, response);

            var path = Path.Combine(_testDir, "results", "cmd-1.json");
            Assert.IsTrue(File.Exists(path));
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
