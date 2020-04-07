using System;
using Hazelcast.Testing.Conditions;
using NuGet.Versioning;
using NUnit.Framework;

// this allows overriding whatever server version we're using
[assembly:ServerVersion("4.2")]

namespace Hazelcast.Tests.Testing
{
    [TestFixture]
    public class ServerVersionAttributeTests
    {
        [Test]
        public void VersionIsAttributeValue()
        {
            Assert.AreEqual(NuGetVersion.Parse("4.2"), ServerVersion.Version);
        }

        [Test]
        public void VersionReadsEnv()
        {
            Environment.SetEnvironmentVariable(ServerVersion.EnvironmentVariableName, "4.6");
            ServerVersion.Reset();
            Assert.AreEqual(NuGetVersion.Parse("4.6"), ServerVersion.Version);
            Environment.SetEnvironmentVariable(ServerVersion.EnvironmentVariableName, "");
            ServerVersion.Reset();
            Assert.AreEqual(NuGetVersion.Parse("4.2"), ServerVersion.Version);
        }
    }

    [TestFixture]
    public class TestConditions40Tests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ServerVersion.Version = NuGetVersion.Parse("4.0");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ServerVersion.Reset();
        }
    }

    [TestFixture]
    public class TestConditions41Tests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ServerVersion.Version = NuGetVersion.Parse("4.1");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ServerVersion.Reset();
        }
    }

    [TestFixture]
    [ServerCondition("[4.3]")]
    public class TestConditions42Tests
    {
        // this test should never execute
        //
        [Test]
        public void Fail()
        {
            Assert.Fail();
        }
    }

    public abstract class TestConditionsTestsBase
    {
        // this test executes only if the server version is 4.1
        // and then the call to Require41 is expected to succeed
        //
        [Test]
        [ServerCondition("[4.1]")]
        public void CanDoSomethingOn41()
        {
            Require41();
        }

        // this test executes only if the server version is 4.0
        // and then the call to Require41 is expected to fail
        //
        [Test]
        [ServerCondition("[4.0]")]
        public void CannotDoSomethingOn40()
        {
            Assert.Throws<NotImplementedException>(Require41);
        }

        // this test always executes...
        //
        [Test]
        public void Mixed()
        {
            // the code executes only if the server version is 4.1
            // and then the call to Require41 is expected to succeed
            //
            ServerCondition.InRange("[4.1]", Require41);

            ServerCondition.InRange("[4.0]", () =>
            {
                // this code executes only if the server version is 4.0
                // and then the call to Require41 is expected to fail
                //
                Assert.Throws<NotImplementedException>(Require41);
            });

            ServerCondition.InRange("[4.1]",
                // this code executes only if the server version is 4.1
                // and then the call to Require41 is expected to succeed
                //
                Require41,

                // this code executes only if the server version is not 4.1
                // and then the call to Require41 is expected to fail
                //
                () =>
                {
                    Assert.Throws<NotImplementedException>(Require41);
                });
        }

        // ok on 4.1, throws on 4.0
        private static void Require41()
        {
            if (ServerVersion.Version < NuGetVersion.Parse("4.1"))
                throw new NotImplementedException();
        }
    }
}
