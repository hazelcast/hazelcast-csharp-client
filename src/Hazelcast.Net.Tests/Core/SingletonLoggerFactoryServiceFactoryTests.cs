using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    public class SingletonLoggerFactoryServiceFactoryTests
    {
        [Test]
        public void TestCreateLogger()
        {
            var f = new SingletonLoggerFactoryServiceFactory()
            {
                Creator = () => NullLoggerFactory.Instance
            };

            var s = f.CreateLogger<SingletonLoggerFactoryServiceFactoryTests>();
            Assert.That(s, Is.Not.Null);
            Assert.That(s, Is.InstanceOf<ILogger<SingletonLoggerFactoryServiceFactoryTests>>());
        }
        
        [Test]
        public void TestAddLogger()
        {
            var f = new SingletonLoggerFactoryServiceFactory()
            {
                Creator = () => NullLoggerFactory.Instance
            };

            var mockProvider = Mock.Of<IMyProvider>();
            
            f.AddProvider(mockProvider);

            //var l = f.ServiceProvider.;
            
        }


        public interface IMyProvider : ILoggerProvider
        {
            
        }
    }
}
