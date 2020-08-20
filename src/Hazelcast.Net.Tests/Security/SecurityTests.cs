// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Security;
using NUnit.Framework;

namespace Hazelcast.Tests.Security
{
    [TestFixture]
    public class SecurityTests
    {
        [Test]
        public void Default()
        {
            var factory = new DefaultCredentialsFactory();
            var credentials = factory.NewCredentials();
            Assert.That(credentials, Is.InstanceOf<UsernamePasswordCredentials>());
            Assert.That(credentials.Name, Is.Null);
            Console.WriteLine(credentials);
            factory.Dispose();
        }

        [Test]
        public void UserNamePassword1()
        {
            var factory = new UsernamePasswordCredentialsFactory("username", "password");
            var credentials = factory.NewCredentials();
            Assert.That(credentials, Is.InstanceOf<UsernamePasswordCredentials>());
            Assert.That(credentials.Name, Is.EqualTo("username"));
            Console.WriteLine(credentials);
            var typed = (UsernamePasswordCredentials) credentials;
            Assert.That(typed.Password, Is.EqualTo("password"));
            factory.Dispose();
        }

        [Test]
        public void UserNamePassword2()
        {
            var factory = new UsernamePasswordCredentialsFactory(new Dictionary<string, string>
            {
                { "username", "username" },
                { "password", "password" },
                { "foo", "bar" }
            });
            var credentials = factory.NewCredentials();
            Assert.That(credentials, Is.InstanceOf<UsernamePasswordCredentials>());
            Assert.That(credentials.Name, Is.EqualTo("username"));
            Console.WriteLine(credentials);
            var typed = (UsernamePasswordCredentials)credentials;
            Assert.That(typed.Password, Is.EqualTo("password"));
            factory.Dispose();
        }

        [Test]
        public void Static()
        {
            var credentials = new UsernamePasswordCredentials();
            var factory = new StaticCredentialsFactory(credentials);
            Assert.That(factory.NewCredentials(), Is.SameAs(credentials));
        }

        [Test]
        public void Token1()
        {
            var token = new byte[16];
            var factory = new TokenCredentialsFactory(token);
            var credentials = factory.NewCredentials();
            Console.WriteLine(credentials);
            Assert.That(credentials.Name, Is.EqualTo("<token>"));
            Assert.That(credentials, Is.InstanceOf<TokenCredentials>());
            var typed = (TokenCredentials)credentials;
            Assert.That(typed.GetToken(), Is.SameAs(token));
            factory.Dispose();
        }

        [Test]
        public void Token2()
        {
            var factory = new TokenCredentialsFactory("token");
            var credentials = factory.NewCredentials();
            Assert.That(credentials.Name, Is.EqualTo("<token>"));
            Assert.That(credentials, Is.InstanceOf<TokenCredentials>());
            var typed = (TokenCredentials) credentials;
            Assert.That(typed.GetToken(), Is.EqualTo(Encoding.UTF8.GetBytes("token")));
            factory.Dispose();
        }

        [Test]
        public void Token3()
        {
            var factory = new TokenCredentialsFactory(new Dictionary<string, string>
            {
                { "token", "token" }
            });
            var credentials = factory.NewCredentials();
            Assert.That(credentials.Name, Is.EqualTo("<token>"));
            Assert.That(credentials, Is.InstanceOf<TokenCredentials>());
            var typed = (TokenCredentials)credentials;
            Assert.That(typed.GetToken(), Is.EqualTo(Encoding.UTF8.GetBytes("token")));
            factory.Dispose();
        }

        [Test]
        [Explicit("Fails - no kerberos for now")]
        public void Kerberos()
        {
            Assert.Throws<ArgumentException>(() => _ = new KerberosCredentialsFactory(null));
            Assert.Throws<ArgumentException>(() => _ = new KerberosCredentialsFactory(null, "u", "p", "d"));
            Assert.Throws<ArgumentException>(() => _ = new KerberosCredentialsFactory("spn", null, "p", "d"));
            Assert.Throws<ArgumentException>(() => _ = new KerberosCredentialsFactory("spn", "u", null, "d"));
            Assert.Throws<ArgumentException>(() => _ = new KerberosCredentialsFactory("spn", "u", "p", null));

            var factory = new KerberosCredentialsFactory("spn");
            var credentials = factory.NewCredentials();
            Console.WriteLine(credentials);
            Assert.That(credentials, Is.InstanceOf<KerberosCredentials>());
            var typed = (KerberosCredentials) credentials;

            // testing the actual credentials require Kerberos?

            Assert.That(factory.NewCredentials(), Is.SameAs(credentials));
            factory.Reset();
            Assert.That(factory.NewCredentials(), Is.Not.SameAs(credentials));

            factory.Dispose();
        }
    }
}
