using System;
using Hazelcast.Core;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class SingletonServiceFactoryTests
    {
        [Test]
        public void ServiceIsNotCreated()
        {
            var factory = new SingletonServiceFactory<IService>
            {
                Creator = () => new Service()
            };

            Service.InstanceCount = 0;
            Service.DisposedCount = 0;

            factory.Dispose();

            Assert.That(Service.InstanceCount, Is.EqualTo(0));
            Assert.That(Service.DisposedCount, Is.EqualTo(0));
        }

        [Test]
        public void ServiceIsCreatedAndDisposed()
        {
            var factory = new SingletonServiceFactory<IService>
            {
                Creator = () => new Service()
            };

            Service.InstanceCount = 0;
            Service.DisposedCount = 0;

            _ = factory.Service;

            factory.Dispose();

            Assert.That(Service.InstanceCount, Is.EqualTo(1));
            Assert.That(Service.DisposedCount, Is.EqualTo(1));
        }

        [Test]
        public void ServiceIsCreatedAndNotDisposed()
        {
            var factory = new SingletonServiceFactory<IService>
            {
                Creator = () => new Service(),
                OwnsService = false
            };

            Service.InstanceCount = 0;
            Service.DisposedCount = 0;

            _ = factory.Service;

            factory.Dispose();

            Assert.That(Service.InstanceCount, Is.EqualTo(1));
            Assert.That(Service.DisposedCount, Is.EqualTo(0));
        }

        [Test]
        public void ServiceIsProvidedAndNotDisposed()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IService, Service>();
            var serviceProvider = services.BuildServiceProvider();

            var factory = new SingletonServiceFactory<IService>
            {
                ServiceProvider = serviceProvider
            };

            Service.InstanceCount = 0;
            Service.DisposedCount = 0;

            _ = factory.Service;

            factory.Dispose();

            Assert.That(Service.InstanceCount, Is.EqualTo(1));
            Assert.That(Service.DisposedCount, Is.EqualTo(0));

            serviceProvider.Dispose();

            Assert.That(Service.DisposedCount, Is.EqualTo(1));
        }

        [Test]
        public void ServiceIsProvidedAndDisposed()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IService, Service>();
            var serviceProvider = services.BuildServiceProvider();

            var factory = new SingletonServiceFactory<IService>
            {
                ServiceProvider = serviceProvider,
                OwnsService = true
            };

            Service.InstanceCount = 0;
            Service.DisposedCount = 0;

            _ = factory.Service;

            factory.Dispose();

            Assert.That(Service.InstanceCount, Is.EqualTo(1));
            Assert.That(Service.DisposedCount, Is.EqualTo(1));

            serviceProvider.Dispose();

            Assert.That(Service.DisposedCount, Is.EqualTo(2));
        }

        [Test]
        public void OwnsCreatedServiceByDefault()
        {
            var factory = new SingletonServiceFactory<IService>
            {
                Creator = () => new Service()
            };

            Assert.That(factory.OwnsService, Is.True);
        }

        [Test]
        public void DoesNotOwnsCreatedServiceByDefault()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var factory = new SingletonServiceFactory<IService>
            {
                ServiceProvider = serviceProvider
            };

            Assert.That(factory.OwnsService, Is.False);
        }

        [Test]
        public void ServiceIsSingleton()
        {
            var factory = new SingletonServiceFactory<IService>
            {
                Creator = () => new Service()
            };

            Assert.That(factory.Service, Is.SameAs(factory.Service));
        }

        [Test]
        public void ShallowCloneReturnsSameCreatedService()
        {
            var factory = new SingletonServiceFactory<IService>
            {
                Creator = () => new Service()
            };

            var clone = factory.Clone();

            Assert.That(factory.Service, Is.SameAs(clone.Service));
            Assert.That(clone.OwnsService, Is.False); // but does not own it
        }

        [Test]
        public void ShallowCloneReturnsSameProvidedService()
        {
            var services = new ServiceCollection();
            services.AddTransient<IService, Service>(); // even if provider is transient
            var serviceProvider = services.BuildServiceProvider();

            var factory = new SingletonServiceFactory<IService>
            {
                ServiceProvider = serviceProvider
            };

            var clone = factory.Clone();

            Assert.That(factory.Service, Is.SameAs(clone.Service));
            Assert.That(clone.OwnsService, Is.False); // still does not own it
        }

        [Test]
        public void DeepCloneReturnsDifferentCreatedService()
        {
            var factory = new SingletonServiceFactory<IService>
            {
                Creator = () => new Service()
            };

            var clone = factory.Clone(false);

            Assert.That(factory.Service, Is.Not.SameAs(clone.Service));
            Assert.That(clone.OwnsService, Is.True); // deep clone owns its service
        }

        [Test]
        public void DeepCloneReturnsDifferentProvidedTransientService()
        {
            var services = new ServiceCollection();
            services.AddTransient<IService, Service>();
            var serviceProvider = services.BuildServiceProvider();

            var factory = new SingletonServiceFactory<IService>
            {
                ServiceProvider = serviceProvider
            };

            var clone = factory.Clone(false);

            Assert.That(factory.Service, Is.Not.SameAs(clone.Service));
            Assert.That(clone.OwnsService, Is.False); // still does not own it
        }

        [Test]
        public void DeepCloneReturnsSameProvidedSingletonService()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IService, Service>();
            var serviceProvider = services.BuildServiceProvider();

            var factory = new SingletonServiceFactory<IService>
            {
                ServiceProvider = serviceProvider
            };

            var clone = factory.Clone(false);

            Assert.That(factory.Service, Is.SameAs(clone.Service));
            Assert.That(clone.OwnsService, Is.False); // still does not own it
        }

        [Test]
        public void CanSetAndGetCreator()
        {
            // ReSharper disable once ConvertToLocalFunction
            Func<IService> creator = () => new Service();

            var factory = new SingletonServiceFactory<IService>
            {
                Creator = creator
            };

            Assert.That(factory.Creator, Is.SameAs(creator));
        }

        [Test]
        public void CanSetAndGetServiceProvider()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var factory = new SingletonServiceFactory<IService>
            {
                ServiceProvider = serviceProvider
            };

            Assert.That(factory.ServiceProvider, Is.SameAs(serviceProvider));
        }

        [Test]
        public void NoLazyServiceToDispose()
        {
            var factory = new SingletonServiceFactory<IService>();
            factory.Dispose();
        }

        [Test]
        public void BrokenFactory()
        {
            Assert.Throws<ArgumentNullException>(() => new BrokenServiceFactory(666));

            new BrokenServiceFactory().DisposeNotDisposing();
        }

        public class BrokenServiceFactory : SingletonServiceFactory<IService>
        {
            public BrokenServiceFactory()
            {}

            public BrokenServiceFactory(int i)
                : base(null, true)
            {}

            public void DisposeNotDisposing()
            {
                Dispose(false);
            }
        }

        public interface IService
        { }

        public class Service : IService, IDisposable
        {
            public Service()
            {
                InstanceCount += 1;
            }

            public static int InstanceCount { get; set; }

            public static int DisposedCount { get; set; }

            public void Dispose()
            {
                DisposedCount += 1;
            }
        }
    }
}
