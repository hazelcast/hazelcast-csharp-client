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
using Hazelcast.Configuration;
using Hazelcast.Configuration.Binding;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using ConfigurationBinder = Hazelcast.Configuration.Binding.ConfigurationBinder;

namespace Hazelcast.Tests.Configuration
{
    // NOTE: this is only testing our own custom code,
    // see ConfigurationBinderTests and ConfigurationCollectionBindingTests which have been
    // imported from MS runtime source code and test that our binder works just like the
    // default binder.

    // we need this here so our extension methods take over the default MS ones
    [TestFixture]
    public class BindingTests
    {
        [Test]
        public void HzBind()
        {
            Assert.Throws<ArgumentNullException>(() => ((IConfiguration)null).HzBind(new Options()));
            Assert.Throws<ArgumentNullException>(() => ((IConfiguration)null).HzBind("section", new Options()));
            Assert.Throws<ArgumentNullException>(() => ((IConfiguration)null).HzBind(new Options(), binderOptions => binderOptions.BindNonPublicProperties = true));

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("value1", "value1"),
                    new KeyValuePair<string, string>("value2", "value2"),
                    new KeyValuePair<string, string>("value3", "value3"),
                    new KeyValuePair<string, string>("value4", "value4"),
                    new KeyValuePair<string, string>("value5", "value5"),
                })
                .Build();

            configuration.HzBind(null);

            var options = new Options();
            configuration.HzBind(options);

            Assert.That(options.Value1, Is.EqualTo("value1"));
            Assert.That(options.Value2, Is.Null);
            Assert.That(options.Value3X, Is.EqualTo("value3"));
            Assert.That(options.GetValue4(), Is.EqualTo("value4"));
            Assert.That(options.GetValue5(), Is.Null);

            options = new Options();
            configuration.HzBind(options, binderOptions => binderOptions.BindNonPublicProperties = true);

            Assert.That(options.Value1, Is.EqualTo("value1"));
            Assert.That(options.Value2, Is.Null);
            Assert.That(options.Value3X, Is.EqualTo("value3"));
            Assert.That(options.GetValue4(), Is.EqualTo("value4"));
            Assert.That(options.GetValue5(), Is.EqualTo("value5"));

            configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("options:value1", "value1"),
                    new KeyValuePair<string, string>("options:value2", "value2"),
                    new KeyValuePair<string, string>("options:value3", "value3"),
                    new KeyValuePair<string, string>("options:value4", "value4"),
                    new KeyValuePair<string, string>("options:value5", "value5"),
                })
                .Build();

            options = new Options();
            configuration.HzBind("options", options);

            Assert.That(options.Value1, Is.EqualTo("value1"));
            Assert.That(options.Value2, Is.Null);
            Assert.That(options.Value3X, Is.EqualTo("value3"));
            Assert.That(options.GetValue4(), Is.EqualTo("value4"));
            Assert.That(options.GetValue5(), Is.Null);

            options = new Options();
            configuration.HzBind("options", options, binderOptions => binderOptions.BindNonPublicProperties = true);

            Assert.That(options.Value1, Is.EqualTo("value1"));
            Assert.That(options.Value2, Is.Null);
            Assert.That(options.Value3X, Is.EqualTo("value3"));
            Assert.That(options.GetValue4(), Is.EqualTo("value4"));
            Assert.That(options.GetValue5(), Is.EqualTo("value5"));

            configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("value6", "value6"),
                })
                .Build();

            Assert.Throws<ConfigurationException>(() => configuration.HzBind(new ThrowOptions()));
        }

        [Test]
        public void ArgumentExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => ConfigurationBinder.Get(((IConfiguration) null), typeof (int), null));
            Assert.Throws<ArgumentNullException>(() => ConfigurationBinder.Get<int>(((IConfiguration)null), null));
            Assert.Throws<ArgumentNullException>(() => ConfigurationBinder.GetValue(((IConfiguration)null), typeof(int), "key", 42));
        }

        [Test]
        public void GetListInvalidValues()
        {
            // copy of an original test that our binder is changing

            var input = new Dictionary<string, string>
            {
                {"InvalidList:0", "true"},
                {"InvalidList:1", "invalid"},
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();
            var list = new List<bool>();

            Assert.Throws<InvalidOperationException>(() => config.GetSection("InvalidList").HzBind(list));
        }

        [Test]
        public void GetArrayInvalidValues()
        {
            // this is *not* tested by MS tests, but still, we changed the behavior

            var input = new Dictionary<string, string>
            {
                {"InvalidArray:0", "true"},
                {"InvalidArray:1", "invalid"},
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();
            var array = new bool[2];

            Assert.Throws<InvalidOperationException>(() => config.GetSection("InvalidArray").HzBind(array));

        }

        public class Options
        {
            public string Value1 { get; set; }

            [BinderIgnore]
            public string Value2 { get; set; }

            [BinderName("Value3")]
            public string Value3X { get; set; }

            [BinderIgnore(false)]
            private string Value4 { get; set; }

            private string Value5 { get; set; }

            public string GetValue4() => Value4;
            public string GetValue5() => Value5;
        }

        public class ThrowOptions
        {
            public string ValueThrow
            {
                get => "";
                set => throw new Exception("bang");
            }
        }
    }
}
