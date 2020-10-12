using System;
using NUnit.Framework;

namespace Hazelcast.Tests
{
    [TestFixture]
    public class HazelcastTests
    {
        [Test]
        public void Succeed()
        {
            // all good!
        }

        [Test]
        public void Environment()
        {
            Console.WriteLine("MachineName:      " + System.Environment.MachineName);
            Console.WriteLine("OSVersion:        " + System.Environment.OSVersion);
            Console.WriteLine("CurrentDirectory: " + System.Environment.CurrentDirectory);
            Console.WriteLine("UserName:         " + System.Environment.UserName);
        }
    }
}
