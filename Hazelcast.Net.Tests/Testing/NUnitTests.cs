using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Hazelcast.Tests.Testing
{
    [TestFixture]
    [Property("FIXTURE", "fixture")]
    public class NUnitTests
    {
        private TestContext _fixtureContext;

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            _fixtureContext = TestContext.CurrentContext;
        }

        [Test]
        [Property("METHOD", "method")]
        public void CanSetAndGetTestProperties()
        {
            // test fixture properties are available on the fixture context only
            Assert.AreEqual("fixture", _fixtureContext.Test.Properties["FIXTURE"]?.FirstOrDefault());
            Assert.IsNull(TestContext.CurrentContext.Test.Properties["FIXTURE"]?.FirstOrDefault());

            // test method properties are immediately available
            Assert.AreEqual("method", TestContext.CurrentContext.Test.Properties["METHOD"]?.FirstOrDefault());
        }

        [Test]
        [Explicit("throws")]
        public async Task Timeout()
        {
            // NUnit *does* show the timeout exception here

            await Task.Delay(100).ContinueWith(x =>
            {
                try
                {
                    Throw();
                }
                catch (Exception e)
                {
                    throw new TimeoutException("timeout", e);
                }
            });
        }

        private void Throw() => throw new Exception("bang");
    }
}
