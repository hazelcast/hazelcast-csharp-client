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

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact;

[TestFixture]
public class CSharpRecordTests
{
    // see https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record
    //
    // record classes can have mutable properties and fields
    // record structs can be mutable as well
    // positional properties are immutable in a record class or readonly record struct, mutable in record struct
    //
    // positional properties are defined in the record constructor, and then the class or struct
    // does not have a parameter-less constructor anymore, which means we cannot just use Activator
    // to create instances.
    // the reflection serializer has been extended to use the one parameter-less constructor
    // if any, otherwise to look for the parameterized constructor with the greatest number of
    // parameters with names all matching compact field names, and then use that constructor
    //
    // these tests ensure that we understand the behavior of records and can handle them
    //
    // note: there is *no* reliable way to determine whether a type is a record, records are
    // plain classes or structs + syntactic sugar, and that is not going to change as it is
    // intentional

    [Test]
    public void CannotWriteClassGetOnlyProperty()
    {
        var rec = new ClassWithGetOnlyProperty( "test");
        Assert.That(rec.Value, Is.EqualTo("test"));

        // does not compile: get-only properties cannot be set
        //rec.Value = "changed";

        var type = rec.GetType();
        var properties = type.GetProperties();
        Assert.That(properties.Length, Is.EqualTo(1));
        var valueProperty = properties[0];
        Assert.That(valueProperty.CanRead);

        // the get-only property cannot even be set by reflection

        Assert.That(valueProperty.CanWrite, Is.False);
    }

    [Test]
    public void CanWriteClassInitProperty()
    {
        var rec = new ClassWithInitProperty { Value = "test" };
        Assert.That(rec.Value, Is.EqualTo("test"));

        // does not compile: init properties cannot be set
        //rec.Value = "changed";

        var type = rec.GetType();
        var properties = type.GetProperties();
        Assert.That(properties.Length, Is.EqualTo(1));
        var valueProperty = properties[0];
        Assert.That(valueProperty.CanRead);

        // however the init property *can* be written, according to reflection

        Assert.That(valueProperty.CanWrite);
        valueProperty.SetValue(rec, "changed");
        Assert.That(rec.Value, Is.EqualTo("changed"));
    }

    [Test]
    public void CanWriteRecordClassPositionalProperty()
    {
        var rec = new RecordClassWithPositionalProperty("test");
        Assert.That(rec.Value, Is.EqualTo("test"));

        // does not compile: positional properties are immutable in record classes
        // rec.Value = "changed";

        var type = rec.GetType();
        var properties = type.GetProperties();
        Assert.That(properties.Length, Is.EqualTo(1));
        var valueProperty = properties[0];
        Assert.That(valueProperty.CanRead);

        // however the positional property *can* be written, according to reflection

        Assert.That(valueProperty.CanWrite);
        valueProperty.SetValue(rec, "changed");
        Assert.That(rec.Value, Is.EqualTo("changed"));
    }

    [Test]
    public void CannotWriteRecordClassGetOnlyProperty()
    {
        var rec = new RecordClassWithGetOnlyProperty("test");
        Assert.That(rec.Value, Is.EqualTo("test"));

        // does not compile: get-only properties cannot be set
        //rec.Value = "changed";

        var type = rec.GetType();
        var properties = type.GetProperties();
        Assert.That(properties.Length, Is.EqualTo(1));
        var valueProperty = properties[0];
        Assert.That(valueProperty.CanRead);

        // the get-only property cannot even be set by reflection

        Assert.That(valueProperty.CanWrite, Is.False);
    }


    [Test]
    public void CanWriteRecordClassInitProperty()
    {
        var rec = new RecordClassWithInitProperty { Value = "test" };
        Assert.That(rec.Value, Is.EqualTo("test"));

        // does not compile: init properties cannot be set
        //rec.Value = "changed";

        var type = rec.GetType();
        var properties = type.GetProperties();
        Assert.That(properties.Length, Is.EqualTo(1));
        var valueProperty = properties[0];
        Assert.That(valueProperty.CanRead);

        // however the init property *can* be written, according to reflection

        Assert.That(valueProperty.CanWrite);
        valueProperty.SetValue(rec, "changed");
        Assert.That(rec.Value, Is.EqualTo("changed"));
    }

    [Test]
    public void CannotInstantiateRecordClassWithPositionalProperty()
    {
        var type = typeof(RecordClassWithPositionalProperty);
        var ctors = type.GetConstructors();

        // missing a parameter-less constructor
        Assert.Throws<MissingMethodException>(() => Activator.CreateInstance(type));

        // there is only 1 constructor
        Assert.That(ctors.Length, Is.EqualTo(1));
        var ctor = ctors[0];
        var parameters = ctor.GetParameters();

        // and it wants 1 parameter named 'Value'
        Assert.That(parameters.Length, Is.EqualTo(1));
        Assert.That(parameters[0].Name, Is.EqualTo("Value"));
    }

    [Test]
    public void CanInstantiateRecordClassWithPositionalProperty()
    {
        var type = typeof(RecordClassWithPositionalProperty);
        var rec = (RecordClassWithPositionalProperty) FormatterServices.GetUninitializedObject(type);

        var properties = type.GetProperties();
        Assert.That(properties.Length, Is.EqualTo(1));
        var valueProperty = properties[0];
        Assert.That(valueProperty.CanRead);

        Assert.That(valueProperty.CanWrite);
        valueProperty.SetValue(rec, "changed");
        Assert.That(rec.Value, Is.EqualTo("changed"));

        // it still is possible to implement other constructors but they all
        // will have to invoke the automatic constructor (with positional
        // properties) *and* that one *cannot* be modified - so we are safe

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields) Console.WriteLine($">>> {field.Name}");
        var backingFieldRegexp = new Regex("\\<(.+)\\>k__BackingField", RegexOptions.Compiled);
        var x = fields.Select(x =>
        {
            var m = backingFieldRegexp.Match(x.Name);
            return m.Success ? (x, m.Groups[1].Value) : (x, null);
        }).ToArray();

        Assert.That(x.Length, Is.EqualTo(1));
        Assert.That(x[0].Item2, Is.EqualTo("Value"));

        // BUT!

        // a typical record (if it has not been altered) *does* have a init setter
        // for all positional properties - so we can say that compact does *not*
        // support properties that are get-only, full stop.
    }

    [Test]
    public void GetUninitializedObjectDoesNotInitialize()
    {
        var type = typeof(ClassWithNoParameterLessConstructor);
        var rec = (ClassWithNoParameterLessConstructor) FormatterServices.GetUninitializedObject(type);

        // problem is, *nothing* is initialized
        // it's not only the constructor that does not run

        Assert.That(rec.Value, Is.Null);
        Assert.That(rec.Value2, Is.Null);
        Assert.That(rec._value3, Is.Null);
    }

    [Test]
    public void GetProperConstructor()
    {
        var type = typeof (RecordClassWithPositionalProperty);

        var ctors = type.GetConstructors();
        var properties = type.GetProperties();

        var serializableProperties = properties
            .Where(x => x.CanRead && x.CanWrite)
            .ToArray();

        var serializablePropertyNames = new HashSet<string>(serializableProperties.Select(x => x.Name));

        bool IsMatch(ConstructorInfo ctor)
        {
            foreach (var parameter in ctor.GetParameters())
            {
                if (!serializablePropertyNames.Contains(parameter.Name))
                    return false;
            }

            return true;
        }

        var ctor = ctors.Where(IsMatch).OrderBy(x => x.GetParameters().Length).FirstOrDefault();
        Assert.That(ctor, Is.Not.Null);
    }

    public class ClassWithNoParameterLessConstructor
    {
        public string _value3 = "value3";

        public ClassWithNoParameterLessConstructor(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public string Value2 { get; set; } = "value2";
    }

    public class ClassWithGetOnlyProperty
    {
        public ClassWithGetOnlyProperty(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    public class ClassWithInitProperty
    {
        public string Value { get; init; } = "";
    }

    public record RecordClassWithPositionalProperty(string Value);

    /*
    public record RecordClassWithPositionalPropertyAndCtor(string Value)
    {
        public RecordClassWithPositionalPropertyAndCtor(string s) : this(s)
        { }
    }
    */

    public record RecordClassWithInitProperty
    {
        public string Value { get; init; } = default!;
    }

    public record RecordClassWithGetOnlyProperty
    {
        public RecordClassWithGetOnlyProperty(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    public readonly record struct ReadonlyRecordStructWithPositionalProperty(string Value);

    public record struct RecordStructWithPositionalProperty(string Value);

    // does not compile: 'set' is illegal for readonly structs
    /*
    public readonly record struct ReadonlyRecordStructWithSettableProperty
    {
        public string Value { get; set; }
    }
    */

    public readonly record struct ReadonlyRecordStructWithInitProperty
    {
        public string Value { get; init; }
    }

    public record struct RecordStructWithInitProperty
    {
        public string Value { get; init; }
    }

    public readonly record struct ReadonlyRecordStructWithGetOnlyProperty
    {
        public ReadonlyRecordStructWithGetOnlyProperty(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    public record struct RecordStructWithGetOnlyProperty
    {
        public RecordStructWithGetOnlyProperty(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}