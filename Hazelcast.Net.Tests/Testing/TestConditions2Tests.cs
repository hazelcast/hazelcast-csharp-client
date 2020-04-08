using Hazelcast.Testing.Conditions;
using NuGet.Versioning;
using NUnit.Framework;

namespace Hazelcast.Tests.Testing
{
    [TestFixture]
    [ServerVersion("0.2")]
    public class TestConditions2Tests : TestConditionsTestsBase
    {
        [Test]
        public void VersionIsOk()
        {
            Assert.AreEqual(NuGetVersion.Parse("0.2"), ServerVersion);
        }
    }
}
