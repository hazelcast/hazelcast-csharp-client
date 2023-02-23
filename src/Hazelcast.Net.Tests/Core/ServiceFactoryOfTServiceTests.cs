// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Hazelcast.Core;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class ServiceFactoryOfTServiceTests
    {
        [Test]
        public void Create()
        {
            var f = new ServiceFactory<IThing>
            {
                Creator = () => new Thing()
            };

            var s = f.Create();
            Assert.That(s, Is.Not.Null);
            Assert.That(s, Is.InstanceOf<Thing>());
            Assert.That(f.Create(), Is.Not.SameAs(s));
        }

        [Test]
        public void Clone()
        {
            var f1 = new ServiceFactory<IThing>
            {
                Creator = () => new Thing()
            };

            var f2 = f1.Clone();

            var s = f2.Create();
            Assert.That(s, Is.Not.Null);
            Assert.That(s, Is.InstanceOf<Thing>());
            Assert.That(f2.Create(), Is.Not.SameAs(s));
            Assert.That(f1.Create(), Is.Not.SameAs(s));
        }

        [Test]
        public void CreateSingleton()
        {
            var f = new SingletonServiceFactory<IThing>
            {
                Creator = () => new Thing()
            };

            var s = f.Service;
            Assert.That(s, Is.Not.Null);
            Assert.That(s, Is.InstanceOf<Thing>());
            Assert.That(f.Service, Is.SameAs(s));
        }
        
        [Test]
        public void AddProviderToSingleton()
        {
            var f = new SingletonServiceFactory<IThing>
            {
                Creator = () => new Thing()
            };

            var s = f.Service;
            
            Assert.That(s, Is.Not.Null);
            Assert.That(s, Is.InstanceOf<Thing>());
            Assert.That(f.Service, Is.SameAs(s));
        }

        [Test]
        public void ShallowCloneSingleton()
        {
            var f1 = new SingletonServiceFactory<IThing>
            {
                Creator = () => new Thing()
            };

            var f2 = f1.Clone();

            var s = f2.Service;
            Assert.That(s, Is.Not.Null);
            Assert.That(s, Is.InstanceOf<Thing>());
            Assert.That(f2.Service, Is.SameAs(s));
            Assert.That(f1.Service, Is.SameAs(s));
        }

        [Test]
        public void DeepCloneSingleton()
        {
            var f1 = new SingletonServiceFactory<IThing>
            {
                Creator = () => new Thing()
            };

            var f2 = f1.Clone(false);

            var s2 = f2.Service;
            Assert.That(s2, Is.Not.Null);
            Assert.That(s2, Is.InstanceOf<Thing>());
            Assert.That(f2.Service, Is.SameAs(s2));

            var s1 = f1.Service;
            Assert.That(s1, Is.Not.Null);
            Assert.That(s1, Is.InstanceOf<Thing>());
            Assert.That(f1.Service, Is.SameAs(s1));

            Assert.That(s1, Is.Not.SameAs(s2));
        }

        [Test]
        public void OwnsSingleton()
        {
            var f = new SingletonServiceFactory<IThing>
            {
                Creator = () => new Thing()
            };

            var s = f.Service;

            f.Dispose();
            Assert.That(s.Disposed, Is.True);
        }

        [Test]
        public void DoesNotOwnSingleton()
        {
            var f = new SingletonServiceFactory<IThing>
            {
                Creator = () => new Thing(),
                OwnsService = false
            };

            var s = f.Service;

            f.Dispose();
            Assert.That(s.Disposed, Is.False);
        }

        [Test]
        public void ProvidedService()
        {
            var services = new ServiceCollection();
            services.AddTransient<IThing, Thing>();
            var serviceProvider = services.BuildServiceProvider();

            var f = new SingletonServiceFactory<IThing>
            {
                ServiceProvider = serviceProvider
            };

            Assert.That(f.OwnsService, Is.False);

            var s = f.Service;
            Assert.That(s, Is.Not.Null);
            Assert.That(s, Is.InstanceOf<Thing>());
            Assert.That(f.Service, Is.SameAs(s));

            f.Dispose();
            Assert.That(s.Disposed, Is.False);

            serviceProvider.Dispose();
            Assert.That(s.Disposed, Is.True);
        }
     
        private interface IThing : IDisposable
        {
            bool Disposed { get; }
        }

        private class Thing : IThing
        {
            public bool Disposed { get; private set; }

            public void Dispose()
            {
                Disposed = true;
            }
        }
    }
}
