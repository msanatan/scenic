using System;
using System.Threading;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Logs;
using UnityEngine;
using UnityEngine.TestTools;

namespace Scenic.Editor.Tests.Commands.Logs
{
    [TestFixture]
    public class LogsCommandHandlerTests
    {
        [Test]
        public void Route_Logs_ReturnsPaginatedResult()
        {
            Debug.Log("scenic-logs-page-" + Guid.NewGuid());

            var response = CommandRouter.Route(CreateRequest("logs-page", "{\"limit\":1,\"offset\":0}"), executeEnabled: true);
            Assert.IsTrue(response.Success);

            var result = response.Result as LogsCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Limit);
            Assert.AreEqual(0, result.Offset);
            Assert.GreaterOrEqual(result.Total, 1);
            Assert.LessOrEqual(result.Logs.Length, 1);
        }

        [Test]
        public void Route_Logs_SeverityFilter_ReturnsMatchingSeverity()
        {
            var warnMessage = "scenic-logs-warn-" + Guid.NewGuid();
            var errorMessage = "scenic-logs-error-" + Guid.NewGuid();
            LogAssert.Expect(LogType.Warning, warnMessage);
            LogAssert.Expect(LogType.Error, errorMessage);
            Debug.LogWarning(warnMessage);
            Debug.LogError(errorMessage);

            Assert.IsTrue(WaitForSeverityMessage("warn", warnMessage));
            Assert.IsTrue(WaitForSeverityMessage("error", errorMessage));

            var warnResponse = CommandRouter.Route(CreateRequest("logs-warn", "{\"severity\":\"warn\",\"limit\":200,\"offset\":0}"), executeEnabled: true);
            Assert.IsTrue(warnResponse.Success);
            var warnResult = warnResponse.Result as LogsCommandResult;
            Assert.IsNotNull(warnResult);
            for (var i = 0; i < warnResult.Logs.Length; i++)
            {
                Assert.AreEqual("warn", warnResult.Logs[i].Severity);
            }

            var errorResponse = CommandRouter.Route(CreateRequest("logs-error", "{\"severity\":\"error\",\"limit\":200,\"offset\":0}"), executeEnabled: true);
            Assert.IsTrue(errorResponse.Success);
            var errorResult = errorResponse.Result as LogsCommandResult;
            Assert.IsNotNull(errorResult);
            for (var i = 0; i < errorResult.Logs.Length; i++)
            {
                Assert.AreEqual("error", errorResult.Logs[i].Severity);
            }
        }

        private static bool WaitForSeverityMessage(string severity, string message, int timeoutMs = 2000)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow <= deadline)
            {
                var response = CommandRouter.Route(
                    CreateRequest("logs-wait", $"{{\"severity\":\"{severity}\",\"limit\":500,\"offset\":0}}"),
                    executeEnabled: true);

                if (response.Success)
                {
                    var result = response.Result as LogsCommandResult;
                    if (result != null)
                    {
                        for (var i = 0; i < result.Logs.Length; i++)
                        {
                            if (result.Logs[i].Message == message)
                            {
                                return true;
                            }
                        }
                    }
                }

                Thread.Sleep(25);
            }

            return false;
        }

        private static CommandRequest CreateRequest(string id, string paramsJson)
        {
            return new CommandRequest
            {
                Id = id,
                Command = "logs",
                ParamsJson = paramsJson,
            };
        }
    }
}
