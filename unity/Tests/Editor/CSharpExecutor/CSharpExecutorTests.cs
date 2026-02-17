using NUnit.Framework;
using UniBridge.Editor;

namespace UniBridge.Editor.Tests
{
    [TestFixture]
    public class CSharpExecutorTests
    {
        [Test]
        public void Evaluate_SimpleExpression_ReturnsResult()
        {
            var response = CSharpExecutor.Execute("2 + 2");
            Assert.IsTrue(response.Success);
            Assert.AreEqual("4", response.Result?.ToString());
        }

        [Test]
        public void Evaluate_InvalidCode_ReturnsError()
        {
            var response = CSharpExecutor.Execute("nonExistent.Foo()");
            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
        }

        [Test]
        public void Evaluate_UnityApi_Works()
        {
            var response = CSharpExecutor.Execute("UnityEngine.Application.unityVersion");
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.Result);
        }
    }
}
