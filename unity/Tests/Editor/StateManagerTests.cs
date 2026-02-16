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
        public void EnsureStateDirectory_CreatesRequestsDirectory()
        {
            StateManager.EnsureStateDirectory(_testDir);

            var requestsPath = Path.Combine(_testDir, "requests");
            Assert.IsTrue(Directory.Exists(requestsPath));
        }

        [Test]
        public void WriteRequest_ReadRequest_RoundTrips()
        {
            StateManager.EnsureStateDirectory(_testDir);
            var request = new CommandRequest
            {
                Id = "cmd-req-1",
                Command = "execute",
                Params = new CommandParams
                {
                    Code = "UnityEngine.Debug.Log(\"hi\")",
                    Ids = new[] { "a", "b" },
                },
            };

            StateManager.WriteRequest(_testDir, request);

            var loaded = StateManager.ReadRequest(_testDir, "cmd-req-1");
            Assert.IsNotNull(loaded);
            Assert.AreEqual("cmd-req-1", loaded.Id);
            Assert.AreEqual("execute", loaded.Command);
            Assert.AreEqual("UnityEngine.Debug.Log(\"hi\")", loaded.Params.Code);
            Assert.AreEqual(2, loaded.Params.Ids.Length);
        }

        [Test]
        public void ListPendingRequests_ExcludesRequestsWithExistingResults()
        {
            StateManager.EnsureStateDirectory(_testDir);

            StateManager.WriteRequest(_testDir, new CommandRequest
            {
                Id = "cmd-pending",
                Command = "execute",
                Params = new CommandParams { Code = "1+1" },
            });

            StateManager.WriteRequest(_testDir, new CommandRequest
            {
                Id = "cmd-complete",
                Command = "execute",
                Params = new CommandParams { Code = "2+2" },
            });
            StateManager.WriteResult(_testDir, new CommandResponse
            {
                Id = "cmd-complete",
                Success = true,
                Result = "4",
            });

            var pending = StateManager.ListPendingRequests(_testDir);
            Assert.AreEqual(1, pending.Length);
            Assert.AreEqual("cmd-pending", pending[0].Id);
        }

        [Test]
        public void DeleteRequest_RemovesRequestFile()
        {
            StateManager.EnsureStateDirectory(_testDir);
            StateManager.WriteRequest(_testDir, new CommandRequest
            {
                Id = "cmd-delete",
                Command = "execute",
                Params = new CommandParams { Code = "3+3" },
            });

            StateManager.DeleteRequest(_testDir, "cmd-delete");

            var requestPath = Path.Combine(_testDir, "requests", "cmd-delete.json");
            Assert.IsFalse(File.Exists(requestPath));
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
