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
using System.ComponentModel;
using System.Reflection;
using Hazelcast.Configuration.Binding;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using ConfigurationBinder = Hazelcast.Configuration.Binding.ConfigurationBinder;

// not my code
#pragma warning disable all
#pragma warning disable IDE0051

namespace Hazelcast.Tests.Configuration
{
    // this is the complete ConfigurationBinder test class from MS runtime source code
    // just to make sure our own binder does not break anything

    // we need this here so our extension methods take over the default MS ones
    [TestFixture]
    public class ConfigurationBinderTests
    {
        public class ComplexOptions
        {
            public ComplexOptions()
            {
                Nested = new NestedOptions();
                Virtual = "complex";
            }

            public NestedOptions Nested { get; set; }
            public int Integer { get; set; }
            public bool Boolean { get; set; }
            public virtual string Virtual { get; set; }
            public object Object { get; set; }

            public string PrivateSetter { get; private set; }
            public string ProtectedSetter { get; protected set; }
            public string InternalSetter { get; internal set; }
            public static string StaticProperty { get; set; }

            private string PrivateProperty { get; set; }
            internal string InternalProperty { get; set; }
            protected string ProtectedProperty { get; set; }

            protected string ProtectedPrivateSet { get; private set; }

            private string PrivateReadOnly { get; }
            internal string InternalReadOnly { get; }
            protected string ProtectedReadOnly { get; }

            public string ReadOnly
            {
                get { return null; }
            }
        }

        public class NestedOptions
        {
            public int Integer { get; set; }
        }

        public class DerivedOptions : ComplexOptions
        {
            public override string Virtual
            {
                get
                {
                    return base.Virtual;
                }
                set
                {
                    base.Virtual = "Derived:" + value;
                }
            }
        }

        public class NullableOptions
        {
            public bool? MyNullableBool { get; set; }
            public int? MyNullableInt { get; set; }
            public DateTime? MyNullableDateTime { get; set; }
        }

        public class EnumOptions
        {
            public UriKind UriKind { get; set; }
        }

        public class GenericOptions<T>
        {
            public T Value { get; set; }
        }

        public class OptionsWithNesting
        {
            public NestedOptions Nested { get; set; }

            public class NestedOptions
            {
                public int Value { get; set; }
            }
        }

        public class ConfigurationInterfaceOptions
        {
            public IConfigurationSection Section { get; set; }
        }

        public class DerivedOptionsWithIConfigurationSection : DerivedOptions
        {
            public IConfigurationSection DerivedSection { get; set; }
        }

        [Test]
        public void CanBindIConfigurationSection()
        {
            var dic = new Dictionary<string, string>
            {
                {"Section:Integer", "-2"},
                {"Section:Boolean", "TRUe"},
                {"Section:Nested:Integer", "11"},
                {"Section:Virtual", "Sup"}
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var options = ConfigurationBinder.Get<ConfigurationInterfaceOptions>(config);

            var childOptions = ConfigurationBinder.Get<DerivedOptions>(options.Section);

            Assert.True(childOptions.Boolean);
            Assert.AreEqual(-2, childOptions.Integer);
            Assert.AreEqual(11, childOptions.Nested.Integer);
            Assert.AreEqual("Derived:Sup", childOptions.Virtual);

            Assert.AreEqual("Section", options.Section.Key);
            Assert.AreEqual("Section", options.Section.Path);
            Assert.Null(options.Section.Value);
        }

        [Test]
        public void CanBindWithKeyOverload()
        {
            var dic = new Dictionary<string, string>
            {
                {"Section:Integer", "-2"},
                {"Section:Boolean", "TRUe"},
                {"Section:Nested:Integer", "11"},
                {"Section:Virtual", "Sup"}
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var options = new DerivedOptions();
            config.HzBind("Section", options);

            Assert.True(options.Boolean);
            Assert.AreEqual(-2, options.Integer);
            Assert.AreEqual(11, options.Nested.Integer);
            Assert.AreEqual("Derived:Sup", options.Virtual);
        }

        [Test]
        public void CanBindIConfigurationSectionWithDerivedOptionsSection()
        {
            var dic = new Dictionary<string, string>
            {
                {"Section:Integer", "-2"},
                {"Section:Boolean", "TRUe"},
                {"Section:Nested:Integer", "11"},
                {"Section:Virtual", "Sup"},
                {"Section:DerivedSection:Nested:Integer", "11"},
                {"Section:DerivedSection:Virtual", "Sup"}
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var options = ConfigurationBinder.Get<ConfigurationInterfaceOptions>(config);

            var childOptions = ConfigurationBinder.Get<DerivedOptionsWithIConfigurationSection>(options.Section);

            var childDerivedOptions = ConfigurationBinder.Get<DerivedOptions>(childOptions.DerivedSection);

            Assert.True(childOptions.Boolean);
            Assert.AreEqual(-2, childOptions.Integer);
            Assert.AreEqual(11, childOptions.Nested.Integer);
            Assert.AreEqual("Derived:Sup", childOptions.Virtual);
            Assert.AreEqual(11, childDerivedOptions.Nested.Integer);
            Assert.AreEqual("Derived:Sup", childDerivedOptions.Virtual);

            Assert.AreEqual("Section", options.Section.Key);
            Assert.AreEqual("Section", options.Section.Path);
            Assert.AreEqual("DerivedSection", childOptions.DerivedSection.Key);
            Assert.AreEqual("Section:DerivedSection", childOptions.DerivedSection.Path);
            Assert.Null(options.Section.Value);
        }

        [Test]
        public void EmptyStringIsNullable()
        {
            var dic = new Dictionary<string, string>
            {
                {"empty", ""},
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            Assert.Null(ConfigurationBinder.GetValue<bool?>(config, "empty"));
            Assert.Null(ConfigurationBinder.GetValue<int?>(config, "empty"));
        }

        [Test]
        public void GetScalarNullable()
        {
            var dic = new Dictionary<string, string>
            {
                {"Integer", "-2"},
                {"Boolean", "TRUe"},
                {"Nested:Integer", "11"}
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            Assert.True(ConfigurationBinder.GetValue<bool?>(config, "Boolean"));
            Assert.AreEqual(-2, ConfigurationBinder.GetValue<int?>(config, "Integer"));
            Assert.AreEqual(11, ConfigurationBinder.GetValue<int?>(config, "Nested:Integer"));
        }

        [Test]
        public void CanBindToObjectProperty()
        {
            var dic = new Dictionary<string, string>
            {
                {"Object", "whatever" }
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var options = new ComplexOptions();
            config.HzBind(options);

            Assert.AreEqual("whatever", options.Object);
        }

        [Test]
        public void GetNullValue()
        {
            var dic = new Dictionary<string, string>
            {
                {"Integer", null},
                {"Boolean", null},
                {"Nested:Integer", null},
                {"Object", null }
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            Assert.False(ConfigurationBinder.GetValue<bool>(config, "Boolean"));
            Assert.AreEqual(0, ConfigurationBinder.GetValue<int>(config, "Integer"));
            Assert.AreEqual(0, ConfigurationBinder.GetValue<int>(config, "Nested:Integer"));
            Assert.Null(ConfigurationBinder.GetValue<ComplexOptions>(config, "Object"));
            Assert.False(ConfigurationBinder.Get<bool>(config.GetSection("Boolean")));
            Assert.AreEqual(0, ConfigurationBinder.Get<int>(config.GetSection("Integer")));
            Assert.AreEqual(0, ConfigurationBinder.Get<int>(config.GetSection("Nested:Integer")));
            Assert.Null(ConfigurationBinder.Get<ComplexOptions>(config.GetSection("Object")));
        }

        [Test]
        public void GetDefaultsWhenDataDoesNotExist()
        {
            var dic = new Dictionary<string, string>
            {
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            Assert.False(ConfigurationBinder.GetValue<bool>(config, "Boolean"));
            Assert.AreEqual(0, ConfigurationBinder.GetValue<int>(config, "Integer"));
            Assert.AreEqual(0, ConfigurationBinder.GetValue<int>(config, "Nested:Integer"));
            Assert.Null(ConfigurationBinder.GetValue<ComplexOptions>(config, "Object"));
            Assert.True(ConfigurationBinder.GetValue(config, "Boolean", true));
            Assert.AreEqual(3, ConfigurationBinder.GetValue(config, "Integer", 3));
            Assert.AreEqual(1, ConfigurationBinder.GetValue(config, "Nested:Integer", 1));
            var foo = new ComplexOptions();
            Assert.AreSame(ConfigurationBinder.GetValue(config, "Object", foo), foo);
        }

        [Test]
        public void GetUri()
        {
            var dic = new Dictionary<string, string>
            {
                {"AnUri", "http://www.bing.com"}
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var uri = ConfigurationBinder.GetValue<Uri>(config, "AnUri");

            Assert.AreEqual("http://www.bing.com", uri.OriginalString);
        }

        [Theory]
        [TestCase("2147483647", typeof(int))]
        [TestCase("4294967295", typeof(uint))]
        [TestCase("32767", typeof(short))]
        [TestCase("65535", typeof(ushort))]
        [TestCase("-9223372036854775808", typeof(long))]
        [TestCase("18446744073709551615", typeof(ulong))]
        [TestCase("trUE", typeof(bool))]
        [TestCase("255", typeof(byte))]
        [TestCase("127", typeof(sbyte))]
        [TestCase("\u25fb", typeof(char))]
        [TestCase("79228162514264337593543950335", typeof(decimal))]
        [TestCase("1.79769e+308", typeof(double))]
        [TestCase("3.40282347E+38", typeof(float))]
        [TestCase("2015-12-24T07:34:42-5:00", typeof(DateTime))]
        [TestCase("12/24/2015 13:44:55 +4", typeof(DateTimeOffset))]
        [TestCase("99.22:22:22.1234567", typeof(TimeSpan))]
        [TestCase("http://www.bing.com", typeof(Uri))]
        // enum test
        [TestCase("Constructor", typeof(AttributeTargets))]
        [TestCase("CA761232-ED42-11CE-BACD-00AA0057B223", typeof(Guid))]
        public void CanReadAllSupportedTypes(string value, Type type)
        {
            // arrange
            var dic = new Dictionary<string, string>
            {
                {"Value", value}
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var optionsType = typeof(GenericOptions<>).MakeGenericType(type);
            var options = Activator.CreateInstance(optionsType);
            var expectedValue = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value);

            // act
            config.HzBind(options);
            var optionsValue = options.GetType().GetProperty("Value").GetValue(options);
            var getValueValue = ConfigurationBinder.GetValue(config, type, "Value");
            var getValue = ConfigurationBinder.Get(config.GetSection("Value"), type);

            // assert
            Assert.AreEqual(expectedValue, optionsValue);
            Assert.AreEqual(expectedValue, getValue);
            Assert.AreEqual(expectedValue, getValueValue);
        }

        [Theory]
        [TestCase(typeof(int))]
        [TestCase(typeof(uint))]
        [TestCase(typeof(short))]
        [TestCase(typeof(ushort))]
        [TestCase(typeof(long))]
        [TestCase(typeof(ulong))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(byte))]
        [TestCase(typeof(sbyte))]
        [TestCase(typeof(char))]
        [TestCase(typeof(decimal))]
        [TestCase(typeof(double))]
        [TestCase(typeof(float))]
        [TestCase(typeof(DateTime))]
        [TestCase(typeof(DateTimeOffset))]
        [TestCase(typeof(TimeSpan))]
        [TestCase(typeof(AttributeTargets))]
        [TestCase(typeof(Guid))]
        public void ConsistentExceptionOnFailedBinding(Type type)
        {
            // arrange
            const string IncorrectValue = "Invalid data";
            const string ConfigKey = "Value";
            var dic = new Dictionary<string, string>
            {
                {ConfigKey, IncorrectValue}
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var optionsType = typeof(GenericOptions<>).MakeGenericType(type);
            var options = Activator.CreateInstance(optionsType);

            // act
            var exception = Assert.Throws<InvalidOperationException>(
                () => config.HzBind(options));

            var getValueException = Assert.Throws<InvalidOperationException>(
                () => ConfigurationBinder.GetValue(config, type, "Value"));

            var getException = Assert.Throws<InvalidOperationException>(
                () => ConfigurationBinder.Get(config.GetSection("Value"), type));

            // assert
            Assert.NotNull(exception.InnerException);
            Assert.NotNull(getException.InnerException);
            //Assert.AreEqual(
            //    Resources.FormatError_FailedBinding(ConfigKey, type),
            //    exception.Message);
            //Assert.AreEqual(
            //    Resources.FormatError_FailedBinding(ConfigKey, type),
            //    getException.Message);
            //Assert.AreEqual(
            //    Resources.FormatError_FailedBinding(ConfigKey, type),
            //    getValueException.Message);
        }

        [Test]
        public void ExceptionOnFailedBindingIncludesPath()
        {
            const string IncorrectValue = "Invalid data";
            const string ConfigKey = "Nested:Value";

            var dic = new Dictionary<string, string>
            {
                {ConfigKey, IncorrectValue}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var options = new OptionsWithNesting();

            var exception = Assert.Throws<InvalidOperationException>(
                () => config.HzBind(options));

            //Assert.AreEqual(Resources.FormatError_FailedBinding(ConfigKey, typeof(int)),
            //    exception.Message);
        }

        [Test]
        public void BinderIgnoresIndexerProperties()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var config = configurationBuilder.Build();
            config.HzBind(new List<string>());
        }

        [Test]
        public void BindCanReadComplexProperties()
        {
            var dic = new Dictionary<string, string>
            {
                {"Integer", "-2"},
                {"Boolean", "TRUe"},
                {"Nested:Integer", "11"}
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var instance = new ComplexOptions();
            config.HzBind(instance);

            Assert.True(instance.Boolean);
            Assert.AreEqual(-2, instance.Integer);
            Assert.AreEqual(11, instance.Nested.Integer);
        }

        [Test]
        public void GetCanReadComplexProperties()
        {
            var dic = new Dictionary<string, string>
            {
                {"Integer", "-2"},
                {"Boolean", "TRUe"},
                {"Nested:Integer", "11"}
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var options = new ComplexOptions();
            config.HzBind(options);

            Assert.True(options.Boolean);
            Assert.AreEqual(-2, options.Integer);
            Assert.AreEqual(11, options.Nested.Integer);
        }

        [Test]
        public void BindCanReadInheritedProperties()
        {
            var dic = new Dictionary<string, string>
            {
                {"Integer", "-2"},
                {"Boolean", "TRUe"},
                {"Nested:Integer", "11"},
                {"Virtual", "Sup"}
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var instance = new DerivedOptions();
            config.HzBind(instance);

            Assert.True(instance.Boolean);
            Assert.AreEqual(-2, instance.Integer);
            Assert.AreEqual(11, instance.Nested.Integer);
            Assert.AreEqual("Derived:Sup", instance.Virtual);
        }

        [Test]
        public void GetCanReadInheritedProperties()
        {
            var dic = new Dictionary<string, string>
            {
                {"Integer", "-2"},
                {"Boolean", "TRUe"},
                {"Nested:Integer", "11"},
                {"Virtual", "Sup"}
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var options = new DerivedOptions();
            config.HzBind(options);

            Assert.True(options.Boolean);
            Assert.AreEqual(-2, options.Integer);
            Assert.AreEqual(11, options.Nested.Integer);
            Assert.AreEqual("Derived:Sup", options.Virtual);
        }

        [Test]
        public void GetCanReadStaticProperty()
        {
            var dic = new Dictionary<string, string>
            {
                {"StaticProperty", "stuff"},
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();
            var options = new ComplexOptions();
            config.HzBind(options);

            Assert.AreEqual("stuff", ComplexOptions.StaticProperty);
        }

        [Test]
        public void BindCanReadStaticProperty()
        {
            var dic = new Dictionary<string, string>
            {
                {"StaticProperty", "other stuff"},
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var instance = new ComplexOptions();
            config.HzBind(instance);

            Assert.AreEqual("other stuff", ComplexOptions.StaticProperty);
        }

        [Test]
        public void CanGetComplexOptionsWhichHasAlsoHasValue()
        {
            var dic = new Dictionary<string, string>
            {
                {"obj", "whut" },
                {"obj:Integer", "-2"},
                {"obj:Boolean", "TRUe"},
                {"obj:Nested:Integer", "11"}
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var options = ConfigurationBinder.Get<ComplexOptions>(config.GetSection("obj"));
            Assert.NotNull(options);
            Assert.True(options.Boolean);
            Assert.AreEqual(-2, options.Integer);
            Assert.AreEqual(11, options.Nested.Integer);
        }

        [Theory]
        [TestCase("ReadOnly")]
        [TestCase("PrivateSetter")]
        [TestCase("ProtectedSetter")]
        [TestCase("InternalSetter")]
        [TestCase("InternalProperty")]
        [TestCase("PrivateProperty")]
        [TestCase("ProtectedProperty")]
        [TestCase("ProtectedPrivateSet")]
        public void GetIgnoresTests(string property)
        {
            var dic = new Dictionary<string, string>
            {
                {property, "stuff"},
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var options = ConfigurationBinder.Get<ComplexOptions>(config);
            Assert.Null(options.GetType().GetTypeInfo().GetDeclaredProperty(property).GetValue(options));
        }

        [Theory]
        [TestCase("PrivateSetter")]
        [TestCase("ProtectedSetter")]
        [TestCase("InternalSetter")]
        [TestCase("InternalProperty")]
        [TestCase("PrivateProperty")]
        [TestCase("ProtectedProperty")]
        [TestCase("ProtectedPrivateSet")]
        public void GetCanSetNonPublicWhenSet(string property)
        {
            var dic = new Dictionary<string, string>
            {
                {property, "stuff"},
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var options = ConfigurationBinder.Get<ComplexOptions>(config, o => o.BindNonPublicProperties = true);
            Assert.AreEqual("stuff", options.GetType().GetTypeInfo().GetDeclaredProperty(property).GetValue(options));
        }

        [Theory]
        [TestCase("InternalReadOnly")]
        [TestCase("PrivateReadOnly")]
        [TestCase("ProtectedReadOnly")]
        public void NonPublicModeGetStillIgnoresReadonly(string property)
        {
            var dic = new Dictionary<string, string>
            {
                {property, "stuff"},
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var options = ConfigurationBinder.Get<ComplexOptions>(config, o => o.BindNonPublicProperties = true);
            Assert.Null(options.GetType().GetTypeInfo().GetDeclaredProperty(property).GetValue(options));
        }

        [Theory]
        [TestCase("ReadOnly")]
        [TestCase("PrivateSetter")]
        [TestCase("ProtectedSetter")]
        [TestCase("InternalSetter")]
        [TestCase("InternalProperty")]
        [TestCase("PrivateProperty")]
        [TestCase("ProtectedProperty")]
        [TestCase("ProtectedPrivateSet")]
        public void BindIgnoresTests(string property)
        {
            var dic = new Dictionary<string, string>
            {
                {property, "stuff"},
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var options = new ComplexOptions();
            config.HzBind(options);

            Assert.Null(options.GetType().GetTypeInfo().GetDeclaredProperty(property).GetValue(options));
        }

        [Theory]
        [TestCase("PrivateSetter")]
        [TestCase("ProtectedSetter")]
        [TestCase("InternalSetter")]
        [TestCase("InternalProperty")]
        [TestCase("PrivateProperty")]
        [TestCase("ProtectedProperty")]
        [TestCase("ProtectedPrivateSet")]
        public void BindCanSetNonPublicWhenSet(string property)
        {
            var dic = new Dictionary<string, string>
            {
                {property, "stuff"},
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var options = new ComplexOptions();
            config.HzBind(options, o => o.BindNonPublicProperties = true );
            Assert.AreEqual("stuff", options.GetType().GetTypeInfo().GetDeclaredProperty(property).GetValue(options));
        }

        [Theory]
        [TestCase("InternalReadOnly")]
        [TestCase("PrivateReadOnly")]
        [TestCase("ProtectedReadOnly")]
        public void NonPublicModeBindStillIgnoresReadonly(string property)
        {
            var dic = new Dictionary<string, string>
            {
                {property, "stuff"},
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            var options = new ComplexOptions();
            config.HzBind(options, o => o.BindNonPublicProperties = true);
            Assert.Null(options.GetType().GetTypeInfo().GetDeclaredProperty(property).GetValue(options));
        }

        [Test]
        public void ExceptionWhenTryingToBindToInterface()
        {
            var input = new Dictionary<string, string>
            {
                {"ISomeInterfaceProperty:Subkey", "x"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var exception = Assert.Throws<InvalidOperationException>(
                () => config.HzBind(new TestOptions()));
            //Assert.AreEqual(
            //    Resources.FormatError_CannotActivateAbstractOrInterface(typeof(ISomeInterface)),
            //    exception.Message);
        }

        [Test]
        public void ExceptionWhenTryingToBindClassWithoutParameterlessConstructor()
        {
            var input = new Dictionary<string, string>
            {
                {"ClassWithoutPublicConstructorProperty:Subkey", "x"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var exception = Assert.Throws<InvalidOperationException>(
                () => config.HzBind(new TestOptions()));
            //Assert.AreEqual(
            //    Resources.FormatError_MissingParameterlessConstructor(typeof(ClassWithoutPublicConstructor)),
            //    exception.Message);
        }

        [Test]
        public void ExceptionWhenTryingToBindToTypeThrowsWhenActivated()
        {
            var input = new Dictionary<string, string>
            {
                {"ThrowsWhenActivatedProperty:subkey", "x"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var exception = Assert.Throws<InvalidOperationException>(
                () => config.HzBind(new TestOptions()));
            Assert.NotNull(exception.InnerException);
            //Assert.AreEqual(
            //    Resources.FormatError_FailedToActivate(typeof(ThrowsWhenActivated)),
            //    exception.Message);
        }

        [Test]
        public void ExceptionIncludesKeyOfFailedBinding()
        {
            var input = new Dictionary<string, string>
            {
                {"NestedOptionsProperty:NestedOptions2Property:ISomeInterfaceProperty:subkey", "x"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var exception = Assert.Throws<InvalidOperationException>(
                () => config.HzBind(new TestOptions()));
            //Assert.AreEqual(
            //    Resources.FormatError_CannotActivateAbstractOrInterface(typeof(ISomeInterface)),
            //    exception.Message);
        }

        private interface ISomeInterface
        {
        }

        private class ClassWithoutPublicConstructor
        {
            private ClassWithoutPublicConstructor()
            {
            }
        }

        private class ThrowsWhenActivated
        {
            public ThrowsWhenActivated()
            {
                throw new Exception();
            }
        }

        private class NestedOptions1
        {
            public NestedOptions2 NestedOptions2Property { get; set; }
        }

        private class NestedOptions2
        {
            public ISomeInterface ISomeInterfaceProperty { get; set; }
        }

        private class TestOptions
        {
            public ISomeInterface ISomeInterfaceProperty { get; set; }

            public ClassWithoutPublicConstructor ClassWithoutPublicConstructorProperty { get; set; }

            public int IntProperty { get; set; }

            public ThrowsWhenActivated ThrowsWhenActivatedProperty { get; set; }

            public NestedOptions1 NestedOptionsProperty { get; set; }
        }
    }
}
