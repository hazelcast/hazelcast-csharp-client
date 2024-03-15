// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.IO;
using System.Reflection;
using Hazelcast.Configuration;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Hazelcast.Tests.Configuration
{
    [TestFixture]
    public class SourcesTests
    {
        [Test]
        public void ArgumentExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => ((ConfigurationBuilder) null).AddHazelcastInMemoryCollection(default));
            Assert.Throws<ArgumentNullException>(() => ((ConfigurationBuilder) null).AddHazelcastCommandLine(default));
            Assert.Throws<ArgumentNullException>(() => ((ConfigurationBuilder) null).AddHazelcastEnvironmentVariables());
            Assert.Throws<ArgumentNullException>(() => ((ConfigurationBuilder) null).AddHazelcastFile(null, "hazelcast.json", null));

            Assert.Throws<ArgumentNullException>(() => ((ConfigurationBuilder) null).AddHazelcastAndDefaults(default));
        }

        [Test]
        public void DetermineEnvironment()
        {
            var dotnetEnvironment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            var aspnetEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            try
            {
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "");
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "");

                Assert.That(ConfigurationBuilderExtensions.DetermineEnvironment(null), Is.EqualTo("Production"));

                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "AspNetCore");
                Assert.That(ConfigurationBuilderExtensions.DetermineEnvironment(null), Is.EqualTo("AspNetCore"));

                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "DotNet");
                Assert.That(ConfigurationBuilderExtensions.DetermineEnvironment(null), Is.EqualTo("DotNet"));

                Assert.That(ConfigurationBuilderExtensions.DetermineEnvironment("Code"), Is.EqualTo("Code"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", dotnetEnvironment);
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", aspnetEnvironment);
            }
        }

        private string[] CommandLineArgs { get; } =
        {
            "hazelcast.arg1=value1",
            "hazelcast:arg2=value2",
            "/hazelcast.arg3", "value3",
            "/hazelcast:arg4", "value4",
            "--hazelcast.arg5", "value5",
            "--hazelcast:arg6", "value6",
            "",
            "/hazelcast.arg7=value7",
            "/hazelcast:arg8=value8",
            "--hazelcast.arg9=value9",
            "--hazelcast:arg10=value10",
        };

        [Test]
        public void CommandLine()
        {
            var configuration = new ConfigurationBuilder()
                .AddCommandLine(CommandLineArgs)
                .AddHazelcastCommandLine(CommandLineArgs)
                .Build();

            for (var i = 1; i <= 8; i++)
                Assert.That(configuration["hazelcast:arg" + i], Is.EqualTo("value" + i));
        }

        private KeyValuePair<string, string>[] InMemoryData { get; } =
        {
            new KeyValuePair<string, string>("hazelcast.arg11", "value11"),
            new KeyValuePair<string, string>("hazelcast:arg12", "value12"),
        };

        [Test]
        public void InMemory()
        {
            var configuration = new ConfigurationBuilder()
                .AddHazelcastInMemoryCollection(InMemoryData)
                .Build();

            Assert.That(configuration["hazelcast:arg11"], Is.EqualTo("value11"));
            Assert.That(configuration["hazelcast:arg12"], Is.EqualTo("value12"));
        }

        [Test]
        public void EnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("hazelcast.arg21", "value21");
            Environment.SetEnvironmentVariable("hazelcast__arg22", "value22");

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddHazelcastEnvironmentVariables()
                .Build();

            Assert.That(configuration["hazelcast:arg21"], Is.EqualTo("value21"));
            Assert.That(configuration["hazelcast:arg22"], Is.EqualTo("value22"));
        }

        [Test]
        public void JsonFile()
        {
            var path = Path.GetFullPath(Path.Combine(Assembly.GetExecutingAssembly().Location, "../../../../Resources/Options"));

            var configuration = new ConfigurationBuilder()
                .AddHazelcastFile(path, "Test.json", "Testing")
                .Build();

            Assert.That(configuration["hazelcast:arg31"], Is.EqualTo("value31"));
            Assert.That(configuration["hazelcast:arg32"], Is.EqualTo("value32"));
        }

        [Test]
        public void All1()
        {
            var path = Path.GetFullPath(Path.Combine(Assembly.GetExecutingAssembly().Location, "../../../../Resources/Options"));

            Environment.SetEnvironmentVariable("hazelcast.arg21", "value21");
            Environment.SetEnvironmentVariable("hazelcast__arg22", "value22");

            var configuration = new ConfigurationBuilder()
                .AddHazelcastAndDefaults(CommandLineArgs, null, InMemoryData, null, path, "Test.json", "Testing")
                .Build();

            for (var i = 1; i <= 8; i++)
                Assert.That(configuration["hazelcast:arg" + i], Is.EqualTo("value" + i));

            Assert.That(configuration["hazelcast:arg11"], Is.EqualTo("value11"));
            Assert.That(configuration["hazelcast:arg12"], Is.EqualTo("value12"));

            Assert.That(configuration["hazelcast:arg21"], Is.EqualTo("value21"));
            Assert.That(configuration["hazelcast:arg22"], Is.EqualTo("value22"));

            Assert.That(configuration["hazelcast:arg31"], Is.EqualTo("value31"));
            Assert.That(configuration["hazelcast:arg32"], Is.EqualTo("value32"));
        }

        [Test]
        public void All2()
        {
            var path = Path.GetFullPath(Path.Combine(Assembly.GetExecutingAssembly().Location, "../../../../Resources/Options"));

            Environment.SetEnvironmentVariable("hazelcast.arg21", "value21");
            Environment.SetEnvironmentVariable("hazelcast__arg22", "value22");

            var configuration = new ConfigurationBuilder()
                .AddHazelcastAndDefaults(CommandLineArgs, null, keyValues: InMemoryData, optionsFilePath: path, environmentName: "Testing")
                .Build();

            for (var i = 1; i <= 8; i++)
                Assert.That(configuration["hazelcast:arg" + i], Is.EqualTo("value" + i));

            Assert.That(configuration["hazelcast:arg11"], Is.EqualTo("value11"));
            Assert.That(configuration["hazelcast:arg12"], Is.EqualTo("value12"));

            Assert.That(configuration["hazelcast:arg21"], Is.EqualTo("value21"));
            Assert.That(configuration["hazelcast:arg22"], Is.EqualTo("value22"));

            Assert.That(configuration["hazelcast:arg41"], Is.EqualTo("value41"));
            Assert.That(configuration["hazelcast:arg42"], Is.EqualTo("value42"));
        }

                [Test]
        public void All3()
        {
            var path = Path.GetFullPath(Path.Combine(Assembly.GetExecutingAssembly().Location, "../../../../Resources/Options"));

            Environment.SetEnvironmentVariable("hazelcast.arg21", "value21");
            Environment.SetEnvironmentVariable("hazelcast__arg22", "value22");

            var configuration = new ConfigurationBuilder()
                .AddCommandLine(CommandLineArgs) // In a normal flow, commandline args will be provided by framework.
                .AddEnvironmentVariables()
                .AddHazelcast(CommandLineArgs, null, keyValues: InMemoryData, optionsFilePath: path, environmentName: "Testing")
                .Build();

            for (var i = 1; i <= 8; i++)
                Assert.That(configuration["hazelcast:arg" + i], Is.EqualTo("value" + i));

            Assert.That(configuration["hazelcast:arg11"], Is.EqualTo("value11"));
            Assert.That(configuration["hazelcast:arg12"], Is.EqualTo("value12"));

            Assert.That(configuration["hazelcast:arg21"], Is.EqualTo("value21"));
            Assert.That(configuration["hazelcast:arg22"], Is.EqualTo("value22"));

            Assert.That(configuration["hazelcast:arg41"], Is.EqualTo("value41"));
            Assert.That(configuration["hazelcast:arg42"], Is.EqualTo("value42"));
        }

    }
}
