// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Text;
using Hazelcast.Configuration;
using Hazelcast.Security;
using Hazelcast.Testing.Conditions;
using NUnit.Framework;

namespace Hazelcast.Tests.Security
{
    [TestFixture]
    public class SecurityTests
    {
        [Test]
        public void Default()
        {
            using var factory = new DefaultCredentialsFactory();

            var credentials = factory.NewCredentials();
            Assert.That(credentials, Is.InstanceOf<UsernamePasswordCredentials>());
            Assert.That(credentials.Name, Is.Null);
            Console.WriteLine(credentials);
        }

        [Test]
        public void UserNamePassword()
        {
            using var factory = new UsernamePasswordCredentialsFactory("username", "password");

            var credentials = factory.NewCredentials();
            Assert.That(credentials, Is.InstanceOf<UsernamePasswordCredentials>());
            Assert.That(credentials.Name, Is.EqualTo("username"));
            Console.WriteLine(credentials);

            var typed = (UsernamePasswordCredentials) credentials;
            Assert.That(typed.Password, Is.EqualTo("password"));
        }

        [Test]
        public void Static()
        {
            var credentials = new UsernamePasswordCredentials();

            using var factory = new StaticCredentialsFactory(credentials);

            Assert.That(factory.NewCredentials(), Is.SameAs(credentials));
        }

        [Test]
        public void Token1()
        {
            var token = new byte[16];

            using var factory = new TokenCredentialsFactory(token);

            var credentials = factory.NewCredentials();
            Console.WriteLine(credentials);
            Assert.That(credentials.Name, Is.EqualTo("<token>"));
            Assert.That(credentials, Is.InstanceOf<TokenCredentials>());

            var typed = (TokenCredentials)credentials;
            Assert.That(typed.GetToken(), Is.SameAs(token));
        }

        [Test]
        public void Token2()
        {
            using var factory = new TokenCredentialsFactory("token");

            var credentials = factory.NewCredentials();
            Assert.That(credentials.Name, Is.EqualTo("<token>"));
            Assert.That(credentials, Is.InstanceOf<TokenCredentials>());

            var typed = (TokenCredentials) credentials;
            Assert.That(typed.GetToken(), Is.EqualTo(Encoding.UTF8.GetBytes("token")));
        }

        [Test]
        public void Token3()
        {
            using var factory = new TokenCredentialsFactory("token", null);

            var credentials = factory.NewCredentials();
            Assert.That(credentials.Name, Is.EqualTo("<token>"));
            Assert.That(credentials, Is.InstanceOf<TokenCredentials>());

            var typed = (TokenCredentials)credentials;
            Assert.That(typed.GetToken(), Is.EqualTo(Encoding.UTF8.GetBytes("token")));
        }

        [Test]
        public void Token4()
        {
            using var factory = new TokenCredentialsFactory("token", "none");

            var credentials = factory.NewCredentials();
            Assert.That(credentials.Name, Is.EqualTo("<token>"));
            Assert.That(credentials, Is.InstanceOf<TokenCredentials>());

            var typed = (TokenCredentials)credentials;
            Assert.That(typed.GetToken(), Is.EqualTo(Encoding.UTF8.GetBytes("token")));
        }

        [Test]
        public void Token5()
        {
            using var factory = new TokenCredentialsFactory(Convert.ToBase64String(Encoding.UTF8.GetBytes("token")), "BASE64");

            var credentials = factory.NewCredentials();
            Assert.That(credentials.Name, Is.EqualTo("<token>"));
            Assert.That(credentials, Is.InstanceOf<TokenCredentials>());

            var typed = (TokenCredentials)credentials;
            Assert.That(typed.GetToken(), Is.EqualTo(Encoding.UTF8.GetBytes("token")));
        }

        [Test]
        public void Token6()
        {
            Assert.Throws<ConfigurationException>(() =>
            {
                var factory = new TokenCredentialsFactory("token", "invalid");
            });
        }

        [Test]
        [Category("enterprise")] // Kerberos is an Enterprise feature
        [Explicit("Requires KDC and domain + configuration.")]
        [ServerCondition("[4.1,)")]
        public void Kerberos()
        {
            Assert.Throws<ArgumentException>(() => _ = new KerberosCredentialsFactory((string) null));
            Assert.Throws<ArgumentException>(() => _ = new KerberosCredentialsFactory(null, "u", "p", "d"));
            Assert.Throws<ArgumentException>(() => _ = new KerberosCredentialsFactory("spn", null, "p", "d"));
            Assert.Throws<ArgumentException>(() => _ = new KerberosCredentialsFactory("spn", "u", null, "d"));
            Assert.Throws<ArgumentException>(() => _ = new KerberosCredentialsFactory("spn", "u", "p", null));

            using var factory = new KerberosCredentialsFactory("spn");

            var credentials = factory.NewCredentials();
            Console.WriteLine(credentials);
            Assert.That(credentials, Is.InstanceOf<KerberosCredentials>());

            Assert.That(factory.NewCredentials(), Is.SameAs(credentials));
            factory.Reset();
            Assert.That(factory.NewCredentials(), Is.Not.SameAs(credentials));
        }
    }
}
