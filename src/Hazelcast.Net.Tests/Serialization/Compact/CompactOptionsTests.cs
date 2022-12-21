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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Moq;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    // a collection of tests that validate the different ways to configure compact
    // serialization with a schema that is not known by the client, and therefore
    // must be automatically fetched, during the first operation which involves
    // ToObject (i.e. we ToObject before ToData and have to fetch the schema from
    // the cluster).

    [TestFixture]
    public class CompactOptionsTests
    {
        [Test]
        public void SetTypeName_CannotThenSetToAnotherValue()
        {
            var options = new CompactOptions();

            // can set it once
            options.SetTypeName<Thing>("thing");

            // can set it again as long as it's the same value
            options.SetTypeName(typeof(Thing), "thing");
            options.SetTypeName<Thing>("thing");

            // setting to a different value throws
            Assert.Throws<ConfigurationException>(() => options.SetTypeName<Thing>("different"));
            Assert.Throws<ConfigurationException>(() => options.SetTypeName(typeof(Thing), "different"));
        }

        [Test]
        public void SetTypeName_CanThenAddSerializerWithSameTypeAndName()
        {
            var options = new CompactOptions();

            options.SetTypeName<Thing>("thing");
            options.AddSerializer(new ThingCompactSerializer<Thing>("thing"));
        }

        [Test]
        public void SetTypeName_CannotThenAddSerializerWithDifferentName()
        {
            var options = new CompactOptions();

            options.SetTypeName<Thing>("thing");
            Assert.Throws<ConfigurationException>(() => options.AddSerializer(new ThingCompactSerializer<Thing>("different")));
        }

        [Test]
        public void SetTypeName_CannotThenAddSerializerWithDifferentType()
        {
            var options = new CompactOptions();

            options.SetTypeName<Thing>("thing");

            // one type Thing has been linked to type name "thing",
            // it is not possible to add a serializer for "thing" that does not handle type Thing

            Assert.Throws<ConfigurationException>(() => options.AddSerializer(new ThingCompactSerializer<DifferentThing>("thing")));
            options.AddSerializer(new ThingCompactSerializer<Thing>("thing"));
        }

        [Test]
        public void SetTypeName_MultipleTypesRequireCustomSerializer()
        {
            var options = new CompactOptions();

            // it is OK to have two types share the same type name
            options.SetTypeName<Thing>("thing");
            options.SetTypeName<DifferentThing>("thing");

            // but then an explicit serializer is required
            options.ReflectionSerializer = Mock.Of<ICompactSerializer<object>>();
            Assert.Throws<ConfigurationException>(() => options.GetRegistrations().ToList());

            // indeed,
            //
            // serialization might work:
            //
            //   serialize Thing1 = uses reflection serializer, creates a schema for Thing1 with "thing" name
            //   serialize Thing2 = uses reflection serializer, already has schema with "thing" name
            //     and the reflection serializer is going to try its best using the schema
            //
            // but de-serialization cannot work:
            //
            //   deserialize "thing" = we have the schema, and use the reflection serializer
            //     which cannot determine which type to create since "thing" maps to both Thing1 and Thing2
            //     but... we cannot determine which type to create since "thing" -> type-1 and type-2
            //
            // a custom serializer is required, which should have a way to determine which type to create

            // with an explicit serializer, it works

            var serializer = new ThingInterfaceCompactSerializer();

            // ok, this fails because the serializer has a different type name for Thing
            Assert.Throws<ConfigurationException>(() => options.AddSerializer<IThing, Thing>(serializer));

            // try again with the correct name
            options = new CompactOptions();
            options.SetTypeName<Thing>(serializer.TypeName);
            options.SetTypeName<DifferentThing>(serializer.TypeName);
            options.AddSerializer<IThing, Thing>(serializer);
            
            var registrations = options.GetRegistrations().ToList();
            Assert.That(registrations.Count, Is.EqualTo(2));
            Assert.That(registrations.Any(x => x.SerializedType == typeof(Thing)));
            Assert.That(registrations.Any(x => x.SerializedType == typeof(DifferentThing)));
            Assert.That(registrations[0].Serializer.Serializer == serializer);
            Assert.That(registrations[1].Serializer.Serializer == serializer);
        }

        [Test]
        public void SetTypeName_CanThenSetSchemaWithSameTypeAndName()
        {
            var options = new CompactOptions();

            options.SetTypeName<Thing>("thing");
            options.SetSchema<Thing>(SchemaBuilder.For("thing").Build(), false);
        }

        [Test]
        public void SetTypeName_CanThenSetSchemaWithSameName()
        {
            var options = new CompactOptions();

            options.SetTypeName<Thing>("thing");
            options.SetSchema(SchemaBuilder.For("thing").Build(), false);
        }

        [Test]
        public void SetTypeName_SetSchemaWithSameName_RequiresExplicitSerializer()
        {
            var options = new CompactOptions();

            // it is OK to have two types share the same type name
            options.SetTypeName<Thing>("thing");
            options.SetSchema<DifferentThing>(SchemaBuilder.For("thing").Build(), false);

            // but then an explicit serializer is required
            options.ReflectionSerializer = Mock.Of<ICompactSerializer<object>>();
            Assert.Throws<ConfigurationException>(() => options.GetRegistrations().ToList());

            // with an explicit serializer, it works
            var serializer = new ThingInterfaceCompactSerializer();

            options = new CompactOptions();
            options.SetTypeName<Thing>(serializer.TypeName);
            options.SetTypeName<DifferentThing>(serializer.TypeName);
            options.AddSerializer<IThing, Thing>(serializer);
            options.ReflectionSerializer = Mock.Of<ICompactSerializer<object>>();

            var registrations = options.GetRegistrations().ToList();
            Assert.That(registrations.Count, Is.EqualTo(2));
            Assert.That(registrations.Any(x => x.SerializedType == typeof(Thing)));
            Assert.That(registrations.Any(x => x.SerializedType == typeof(DifferentThing)));
            Assert.That(registrations[0].Serializer.Serializer == serializer);
            Assert.That(registrations[1].Serializer.Serializer == serializer);
        }

        [Test]
        public void SetTypeName_CannotThenSetSchemaWithDifferentName()
        {
            var options = new CompactOptions();

            options.SetTypeName<Thing>("thing");
            Assert.Throws<ConfigurationException>(() => options.SetSchema<Thing>(SchemaBuilder.For("different").Build(), false));
        }

        [Test]
        public void SetTypeName_Exceptions()
        {
            var options = new CompactOptions();

            Assert.Throws<ArgumentNullException>(() => options.SetTypeName(null, "thing"));
            Assert.Throws<ArgumentException>(() => options.SetTypeName(typeof(Thing), null));
            Assert.Throws<ArgumentException>(() => options.SetTypeName(typeof(Thing), "   "));
        }

        [Test]
        public void SetSchemaOfT_CannotThenSetToAnotherValue()
        {
            var options = new CompactOptions();

            // can set it once
            options.SetSchema<Thing>(SchemaBuilder.For("thing").Build(), false);

            // can set it again as long as it's the same value
            options.SetSchema(typeof(Thing), SchemaBuilder.For("thing").Build(), false);
            options.SetSchema<Thing>(SchemaBuilder.For("thing").Build(), false);

            // setting different schema to same type throws
            Assert.Throws<ConfigurationException>(() => options.SetSchema<Thing>(SchemaBuilder.For("different").Build(), false));
            Assert.Throws<ConfigurationException>(() => options.SetSchema(typeof(Thing), SchemaBuilder.For("different").Build(), false));

            // setting different schema to same type name throws
            Assert.Throws<ConfigurationException>(() => options.SetSchema<Thing>(SchemaBuilder.For("thing").WithField("name", FieldKind.Boolean).Build(), false));

            // it is ok to set same schema for a different type
            // but, we end up with 2 types pointing to same schema, and that is going to require an explicit serializer
            // the exception is thrown when getting registrations (see relevant tests)
            options.SetSchema<DifferentThing>(SchemaBuilder.For("thing").Build(), false);

            // also for type-less
            options.SetSchema(SchemaBuilder.For("thing").Build(), false);
            Assert.Throws<ConfigurationException>(() => options.SetSchema(SchemaBuilder.For("thing").WithField("name", FieldKind.Boolean).Build(), false));
        }

        [Test]
        public void SetSchema_CannotThenSetToAnotherValue()
        {
            var options = new CompactOptions();

            // can set it once
            options.SetSchema(SchemaBuilder.For("thing").Build(), false);

            // can set it again as long as it's the same schema
            options.SetSchema(SchemaBuilder.For("thing").Build(), false);

            // setting a different schema for the same typename throws
            Assert.Throws<ConfigurationException>(() => options.SetSchema(SchemaBuilder.For("thing").WithField("name", FieldKind.Boolean).Build(), false));

            // also for typed
            Assert.Throws<ConfigurationException>(() => options.SetSchema<Thing>(SchemaBuilder.For("thing").WithField("name", FieldKind.Boolean).Build(), false));
            options.SetSchema<Thing>(SchemaBuilder.For("thing").Build(), false);
        }

        [Test]
        public void SetSchema_Exceptions()
        {
            var options = new CompactOptions();

            var schema = SchemaBuilder.For("thing").Build();
            Assert.Throws<ArgumentNullException>(() => options.SetSchema(null, schema, true));
            Assert.Throws<ArgumentNullException>(() => options.SetSchema(typeof(Thing), null, true));
            Assert.Throws<ArgumentNullException>(() => options.SetSchema((Schema) null, true));
            Assert.Throws<ArgumentNullException>(() => options.SetSchema((Type) null, true));
        }

        [Test]
        public void AddSerializer_Exceptions()
        {
            var options = new CompactOptions();

            Assert.Throws<ArgumentNullException>(() => options.AddSerializer((ICompactSerializer)null));
            Assert.Throws<ArgumentNullException>(() => options.AddSerializer<Thing>((ICompactSerializer<Thing>)null));
            Assert.Throws<ArgumentNullException>(() => options.AddSerializer<Thing, Thing>((ICompactSerializer<Thing>)null));

            Assert.Throws<ArgumentException>(() => options.AddSerializer(new BogusCompactSerializer()));

            options.AddSerializer((ICompactSerializer)new ThingCompactSerializer<Thing>());
        }

        private class BogusCompactSerializer : ICompactSerializer
        {
            public string TypeName => throw new NotSupportedException();
        }

        [Test]
        public void AddType()
        {
            var options = new CompactOptions();

            options.AddType(typeof (Thing));
            options.AddType<Thing>();

            Assert.Throws<ArgumentNullException>(() => options.AddType(null));
        }

        public class BogusCtorCompactSerializer : ICompactSerializer<object>
        {
            // prevents Activator.CreateInstance from working
            public BogusCtorCompactSerializer(int ignored) { }

            public string TypeName => throw new NotSupportedException();

            public object Read(ICompactReader reader) => throw new NotSupportedException();

            public void Write(ICompactWriter writer, object value) => throw new NotSupportedException();
        }

        [Test]
        public void Exceptions()
        {
            var options = new CompactOptions();

            options.AddSerializer(new Serializer1("thing"));

            // unique serializer per type name!
            Assert.Throws<ConfigurationException>(() => options.AddSerializer(new Serializer1("thing")));

            // serializer for that name cannot serialize that type!
            Assert.Throws<ConfigurationException>(() => options.SetTypeName<DifferentThing>("thing"));

            // must be >0!
            Assert.Throws<ArgumentException>(() => options.SchemaReplicationRetries = 0);
        }

        private class Serializer1 : ICompactSerializer<Thing>
        {
            public Serializer1(string typeName)
            {
                TypeName = typeName;
            }

            public string TypeName { get; }

            public Thing Read(ICompactReader reader) => throw new NotSupportedException();

            public void Write(ICompactWriter writer, Thing value) => throw new NotSupportedException();
        }

        [Test]
        public void GetRegistrations()
        {
            var options = new CompactOptions();
            options.ReflectionSerializer = Mock.Of<ICompactSerializer<object>>();

            Assert.That(options.GetRegistrations(), Is.Empty);

            options.SetSchema(SchemaBuilder.For("duh").Build(), true);
            Assert.Throws<ConfigurationException>(() => options.GetRegistrations().ToList());

            options = new CompactOptions
            {
                ReflectionSerializer = Mock.Of<ICompactSerializer<object>>()
            };
            options.SetSchema(SchemaBuilder.For("System.Object").Build(), true);
            Assert.That(options.GetRegistrations().Count(), Is.EqualTo(1));

            options = new CompactOptions
            {
                ReflectionSerializer = Mock.Of<ICompactSerializer<object>>()
            };
            options.AddType<object>();
            Assert.That(options.GetRegistrations().Count(), Is.EqualTo(1));
        }
    }
}
