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
using System.Linq;
using System.Reflection;
using Hazelcast.Core;
using Hazelcast.Predicates;
using Hazelcast.Serialization;
using Hazelcast.Testing.Predicates;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Predicates
{
    [TestFixture]
    public class PredicatesTests
    {
        private ISerializationService _serializationService;

        [SetUp]
        public void SetUp()
        {
            _serializationService = new SerializationServiceBuilder(new NullLoggerFactory())
                .AddHook<PredicateDataSerializerHook>() // not pretty in tests, eh?
                .Build();
        }

        [Test]
        public void MiscPredicatesTests()
        {
            AssertPredicate(Query.True(), PredicateDataSerializerHook.TruePredicate);
            AssertPredicate(Query.False(), PredicateDataSerializerHook.FalsePredicate);

            AssertPredicate(Query.WhereNot(Query.True()), PredicateDataSerializerHook.NotPredicate);
            AssertPredicate(Query.True().Not(), PredicateDataSerializerHook.NotPredicate);

            AssertPredicate(Query.WhereAll(), PredicateDataSerializerHook.AndPredicate);
            AssertPredicate(Query.True().And(Query.False()), PredicateDataSerializerHook.AndPredicate);

            AssertPredicate(Query.WhereAny(), PredicateDataSerializerHook.OrPredicate);
            AssertPredicate(Query.True().Or(Query.False()), PredicateDataSerializerHook.OrPredicate);

            AssertPredicate(Query.EqualTo("name", 33), PredicateDataSerializerHook.EqualPredicate);
            AssertPredicate(Query.Where("name").IsEqualTo(33), PredicateDataSerializerHook.EqualPredicate);
            AssertPredicate(Query.NotEqualTo("name", 33), PredicateDataSerializerHook.NotEqualPredicate);
            AssertPredicate(Query.Where("name").IsNotEqualTo(33), PredicateDataSerializerHook.NotEqualPredicate);

            AssertPredicate(Query.GreaterThan("name", 33), PredicateDataSerializerHook.GreaterLessPredicate);
            AssertPredicate(Query.Where("name").IsGreaterThan(33), PredicateDataSerializerHook.GreaterLessPredicate);
            AssertPredicate(Query.GreaterThanOrEqualTo("name", 33), PredicateDataSerializerHook.GreaterLessPredicate);
            AssertPredicate(Query.Where("name").IsGreaterThanOrEqualTo(33), PredicateDataSerializerHook.GreaterLessPredicate);
            AssertPredicate(Query.LessThan("name", 33), PredicateDataSerializerHook.GreaterLessPredicate);
            AssertPredicate(Query.Where("name").IsLessThan(33), PredicateDataSerializerHook.GreaterLessPredicate);
            AssertPredicate(Query.LessThanOrEqualTo("name", 33), PredicateDataSerializerHook.GreaterLessPredicate);
            AssertPredicate(Query.Where("name").IsLessThanOrEqualTo(33), PredicateDataSerializerHook.GreaterLessPredicate);

            AssertPredicate(Query.Between("name", 33, 44), PredicateDataSerializerHook.BetweenPredicate);
            AssertPredicate(Query.Where("name").IsBetween(33, 44), PredicateDataSerializerHook.BetweenPredicate);

            AssertPredicate(Query.In("name", 33, 44), PredicateDataSerializerHook.InPredicate);
            AssertPredicate(Query.Where("name").IsIn(33, 44), PredicateDataSerializerHook.InPredicate);

            AssertPredicate(Query.Like("name", "expression"), PredicateDataSerializerHook.LikePredicate);
            AssertPredicate(Query.Where("name").IsLike("expression"), PredicateDataSerializerHook.LikePredicate);
            AssertPredicate(Query.ILike("name", "expression"), PredicateDataSerializerHook.ILikePredicate);
            AssertPredicate(Query.Where("name").IsILike("expression"), PredicateDataSerializerHook.ILikePredicate);
            AssertPredicate(Query.Match("name", "regex"), PredicateDataSerializerHook.RegexPredicate);
            AssertPredicate(Query.Where("name").Matches("regex"), PredicateDataSerializerHook.RegexPredicate);

            AssertPredicate(Query.InstanceOf("className"), PredicateDataSerializerHook.InstanceofPredicate);

            AssertPredicate(Query.In("name", 1, 2, 3), PredicateDataSerializerHook.InPredicate);

            AssertPredicate(Query.Sql("sql"), PredicateDataSerializerHook.SqlPredicate);

            AssertPredicate(Query.WhereKey("property").IsILike("expression"), PredicateDataSerializerHook.ILikePredicate);
            AssertPredicate(Query.WhereValue().IsILike("expression"), PredicateDataSerializerHook.ILikePredicate);
        }

        [Test]
        public void QuerySyntaxes()
        {
            var p1 = Query.EqualTo("attribute", "value");
            Console.WriteLine("p1: " + p1);
            Assert.That(p1.ToString(), Is.EqualTo("(attribute == value)"));

            var p2 = Query.Where("attribute").IsEqualTo("value");
            Console.WriteLine("p2: " + p2);
            Assert.That(p2.ToString(), Is.EqualTo("(attribute == value)"));


            var p3 = Query.EqualTo("attribute", "value").Not();
            Console.WriteLine("p3: " + p3);
            Assert.That(p3.ToString(), Is.EqualTo("NOT((attribute == value))"));

            var p4 = Query.WhereNot(Query.EqualTo("attribute", "value"));
            Console.WriteLine("p4: " + p4);
            Assert.That(p4.ToString(), Is.EqualTo("NOT((attribute == value))"));

            var p5 = Query.Where("attribute").IsNot().EqualTo("value");
            Console.WriteLine("p5: " + p5);
            Assert.That(p5.ToString(), Is.EqualTo("NOT((attribute == value))"));

            var p6 = Query.WhereAll(
                Query.EqualTo("attribute1", "value1"),
                Query.EqualTo("attribute2", "value2").Not());
            Console.WriteLine("p6: " + p6);
            Assert.That(p6.ToString(), Is.EqualTo("AND((attribute1 == value1), NOT((attribute2 == value2)))"));

            var p7 = Query
                .Where("attribute1").IsEqualTo("value1")
                .And("attribute2").IsNot().EqualTo("value2");
            Console.WriteLine("p7: " + p7);
            Assert.That(p7.ToString(), Is.EqualTo("AND((attribute1 == value1), NOT((attribute2 == value2)))"));

            var p8 = Query.True().And(Query.False());
            Console.WriteLine("p8: " + p8);
            Assert.That(p8.ToString(), Is.EqualTo("AND(TRUE, FALSE)"));

            var p9 = Query
                .Where("A").IsEqualTo("valueA")
                .Or("B").IsEqualTo("valueB")
                .And("C").IsEqualTo("valueC");
            Console.WriteLine("p9: " + p9);
            Assert.That(p9.ToString(), Is.EqualTo("OR((A == valueA), AND((B == valueB), (C == valueC)))"));

            var pA = Query
                .Where("A").IsEqualTo("valueA")
                .And("B").IsEqualTo("valueB")
                .Or("C").IsEqualTo("valueC");
            Console.WriteLine("pA1: " + pA);
            Assert.That(pA.ToString(), Is.EqualTo("OR(AND((A == valueA), (B == valueB)), (C == valueC))"));
        }

        [Test]
        public void PagingPredicateTest()
        {
            AssertPredicate(new PagingPredicate(3, Query.True()), PredicateDataSerializerHook.PagingPredicate);
            AssertPredicate(new PagingPredicate(3, Query.True(), new PredicateComparer()), PredicateDataSerializerHook.PagingPredicate);

            var paging = new PagingPredicate(3, Query.True());
            paging.AnchorList.Add(new KeyValuePair<int, KeyValuePair<object, object>>(0, new KeyValuePair<object, object>("a", "b")));
            paging.AnchorList.Add(new KeyValuePair<int, KeyValuePair<object, object>>(1, new KeyValuePair<object, object>("c", "d")));
            AssertPredicate(paging, PredicateDataSerializerHook.PagingPredicate);

            Assert.That(paging.Page, Is.EqualTo(0));
            paging.NextPage();
            Assert.That(paging.Page, Is.EqualTo(1));
            paging.PreviousPage();
            Assert.That(paging.Page, Is.EqualTo(0));
            paging.PreviousPage();
            Assert.That(paging.Page, Is.EqualTo(0));

            paging.Reset();
            Assert.That(paging.IterationType, Is.Null);

            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new PagingPredicate(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new PagingPredicate(0));
            Assert.Throws<ArgumentException>(() => _ = new PagingPredicate(1, new PagingPredicate(1)));

            Assert.Throws<ArgumentNullException>(() => paging.ReadData(null));

            // cannot test reading data as the paging predicate is not really meant to read data
            // so we cannot test that writing then reading works, need integration tests for this
            Assert.Throws<NotSupportedException>(() => paging.ReadData(new ByteArrayObjectDataInput(new byte[0], null, Endianness.Unspecified)));
        }

        [Test]
        public void PartitionPredicate()
        {
            AssertPredicate(new PartitionPredicate(), PredicateDataSerializerHook.PartitionPredicate);
            AssertPredicate(new PartitionPredicate("key", Query.True()), PredicateDataSerializerHook.PartitionPredicate);

            var partition = new PartitionPredicate("key", Query.True());

            Assert.That(partition.FactoryId, Is.EqualTo(FactoryIds.PredicateFactoryId));
            Assert.That(partition.ClassId, Is.EqualTo(PredicateDataSerializerHook.PartitionPredicate));

            Assert.Throws<ArgumentNullException>(() => partition.WriteData(null));
            Assert.Throws<ArgumentNullException>(() => partition.ReadData(null));

            using var output = new ByteArrayObjectDataOutput(1024, _serializationService, Endianness.Unspecified);
            partition.WriteData(output);
            using var input = new ByteArrayObjectDataInput(output.Buffer, _serializationService, Endianness.Unspecified);
            var p = new PartitionPredicate();
            p.ReadData(input);

            Assert.That(p.PartitionKey, Is.EqualTo(partition.PartitionKey));
            Assert.That(p.Target, Is.EqualTo(partition.Target));
        }

        [Test]
        public void ComparerTest()
        {
            var comparer = new PredicateComparer(1, IterationType.Value);

            Assert.Throws<ArgumentNullException>(() => comparer.WriteData(null));
            Assert.Throws<ArgumentNullException>(() => comparer.ReadData(null));

            using var output = new ByteArrayObjectDataOutput(1024, _serializationService, Endianness.Unspecified);
            comparer.WriteData(output);
            var c = new PredicateComparer();
            using var input = new ByteArrayObjectDataInput(output.Buffer, _serializationService, Endianness.Unspecified);
            c.ReadData(input);

            Assert.That(c.Type, Is.EqualTo(comparer.Type));
            Assert.That(c.IterationType, Is.EqualTo(comparer.IterationType));

            // entry

            Assert.That(new PredicateComparer(0, IterationType.Key).Compare(("key1", "value1"), ("key2","value2")),
                Is.EqualTo("key1".CompareTo("key2")));

            Assert.That(new PredicateComparer(1, IterationType.Key).Compare(("key1", "value1"), ("key2", "value2")),
                Is.EqualTo("key2".CompareTo("key1")));

            Assert.That(new PredicateComparer(2, IterationType.Key).Compare(("key1", "value1"), ("key2", "value2")),
                Is.EqualTo(0));

            Assert.That(new PredicateComparer(2, IterationType.Key).Compare(("key1", "value1"), ("key2x", "value2")),
                Is.EqualTo("key1".Length.CompareTo("key2x".Length)));

            Assert.That(new PredicateComparer(3, IterationType.Key).Compare(("key1", "value1"), ("key2", "value2")),
                Is.EqualTo(0));

            Assert.That(new PredicateComparer(3, IterationType.Key).Compare(("key1", "value1"), ("key2x", "value2")),
                Is.EqualTo(0));

            // uh?

            Assert.That(new PredicateComparer(0, (IterationType) 666).Compare(("key1", "value1"), ("key2", "value2")),
                Is.EqualTo("key1".CompareTo("key2")));

            Assert.That(new PredicateComparer(1, (IterationType)666).Compare(("key1", "value1"), ("key2", "value2")),
                Is.EqualTo("key2".CompareTo("key1")));

            Assert.That(new PredicateComparer(2, (IterationType)666).Compare(("key1", "value1"), ("key2", "value2")),
                Is.EqualTo(0));

            Assert.That(new PredicateComparer(2, (IterationType)666).Compare(("key1", "value1"), ("key2x", "value2")),
                Is.EqualTo("key1".Length.CompareTo("key2x".Length)));

            Assert.That(new PredicateComparer(3, (IterationType)666).Compare(("key1", "value1"), ("key2", "value2")),
                Is.EqualTo(0));

            Assert.That(new PredicateComparer(3, (IterationType)666).Compare(("key1", "value1"), ("key2x", "value2")),
                Is.EqualTo(0));

            // value

            Assert.That(new PredicateComparer(0, IterationType.Value).Compare(("key1", "value1"), ("key2", "value2")),
                Is.EqualTo("value1".CompareTo("value2")));

            Assert.That(new PredicateComparer(1, IterationType.Value).Compare(("key1", "value1"), ("key2", "value2")),
                Is.EqualTo("value2".CompareTo("value1")));

            Assert.That(new PredicateComparer(2, IterationType.Value).Compare(("key1", "value1"), ("key2", "value2")),
                Is.EqualTo(0));

            Assert.That(new PredicateComparer(2, IterationType.Value).Compare(("key1", "value1"), ("key2", "value2x")),
                Is.EqualTo("value1".Length.CompareTo("value2x".Length)));

            Assert.That(new PredicateComparer(3, IterationType.Value).Compare(("key1", "value1"), ("key2", "value2")),
                Is.EqualTo(0));

            Assert.That(new PredicateComparer(3, IterationType.Value).Compare(("key1", "value1"), ("key2", "value2x")),
                Is.EqualTo(0));

            // entry

            Assert.That(new PredicateComparer(0, IterationType.Entry).Compare(("key1", "value1"), ("key2", "value2")),
                Is.EqualTo("key1:::value1".CompareTo("key2:::value2")));

            Assert.That(new PredicateComparer(1, IterationType.Entry).Compare(("key1", "value1"), ("key2", "value2")),
                Is.EqualTo("key2:::value2".CompareTo("key1:::value1")));

            Assert.That(new PredicateComparer(2, IterationType.Entry).Compare(("key1", "value1"), ("key2", "value2")),
                Is.EqualTo(0));

            Assert.That(new PredicateComparer(2, IterationType.Entry).Compare(("key1", "value1"), ("key2", "value2x")),
                Is.EqualTo("key1:::value1".Length.CompareTo("key2:::value2x".Length)));

            Assert.That(new PredicateComparer(3, IterationType.Entry).Compare(("key1", "value1"), ("key2", "value2")),
                Is.EqualTo(0));

            Assert.That(new PredicateComparer(3, IterationType.Entry).Compare(("key1", "value1"), ("key2", "value2x")),
                Is.EqualTo(0));

            Assert.That(new PredicateComparer(0, IterationType.Entry).Compare(("abc", "defghi"), ("abcdef", "ghi")),
                Is.EqualTo("abc:::defghi".CompareTo("abcdef:::ghi")));

            // NOTE
            // the behavior for non-supported types and iteration types is ... weird
        }

        private void AssertPredicate(IPredicate predicate, int classId)
        {
            var typeOfPredicate = predicate.GetType();
            var assertMethods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            var assertMethod = assertMethods.First(x => x.Name == nameof(AssertPredicate) && x.IsGenericMethod);
            var method = assertMethod.GetGenericMethodDefinition().MakeGenericMethod(typeOfPredicate);
            method.Invoke(this, new object[] { predicate, classId });
        }

        private T AssertPredicate<T>(T predicate, int classId)
            where T : IIdentifiedDataSerializable
        {
            Assert.That(predicate.FactoryId, Is.EqualTo(FactoryIds.PredicateFactoryId));
            Assert.That(predicate.ClassId, Is.EqualTo(classId));

            Assert.Throws<ArgumentNullException>(() => predicate.WriteData(null));
            Assert.Throws<ArgumentNullException>(() => predicate.ReadData(null));

            using var output = new ByteArrayObjectDataOutput(1024, _serializationService, Endianness.Unspecified);
            predicate.WriteData(output);

            T p = default;
            if (typeof (T) != typeof (PagingPredicate) && typeof (T) != typeof (PartitionPredicate))
            {
                using var input = new ByteArrayObjectDataInput(output.Buffer, _serializationService, Endianness.Unspecified);
                p = (T)Activator.CreateInstance(typeof(T));
                p.ReadData(input);

                Assert.That(predicate.Equals(p));
                Assert.That(predicate.Equals(predicate));
                Assert.That(predicate.Equals(null), Is.False);

                Assert.That(Equals(predicate, p));
                Assert.That(Equals(predicate, predicate));
                Assert.That(Equals(predicate, null), Is.False);

                var type = typeof(T);
                MethodInfo staticEquals;
                do
                {
                    staticEquals = type.GetMethod("Equals", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    type = type.BaseType;

                } while (staticEquals == null && type != typeof(object));
                Assert.That(staticEquals, Is.Not.Null);

                Assert.That((bool)staticEquals.Invoke(null, new object[] { predicate, p }));
                Assert.That((bool)staticEquals.Invoke(null, new object[] { predicate, predicate }));
                Assert.That((bool)staticEquals.Invoke(null, new object[] { predicate, null }), Is.False);

                var data = _serializationService.ToData(predicate);
                p = _serializationService.ToObject<T>(data);
                Assert.That(predicate.Equals(p));
            }

            _ = predicate.GetHashCode();

            Console.WriteLine($"{typeof(T)}: {predicate}");

            return p;
        }

        [Test]
        public void SerializerHookTest()
        {
            var hook = new PredicateDataSerializerHook();

            Assert.That(hook.FactoryId, Is.EqualTo(FactoryIds.PredicateFactoryId));

            var factory = hook.CreateFactory();

            var predicate = factory.Create(PredicateDataSerializerHook.AndPredicate);
            Assert.That(predicate, Is.InstanceOf<AndPredicate>());

            predicate = factory.Create(PredicateDataSerializerHook.BetweenPredicate);
            Assert.That(predicate, Is.InstanceOf<BetweenPredicate>());

            predicate = factory.Create(PredicateDataSerializerHook.EqualPredicate);
            Assert.That(predicate, Is.InstanceOf<EqualPredicate>());

            predicate = factory.Create(PredicateDataSerializerHook.FalsePredicate);
            Assert.That(predicate, Is.InstanceOf<FalsePredicate>());

            predicate = factory.Create(PredicateDataSerializerHook.GreaterLessPredicate);
            Assert.That(predicate, Is.InstanceOf<GreaterLessPredicate>());

            predicate = factory.Create(PredicateDataSerializerHook.InPredicate);
            Assert.That(predicate, Is.InstanceOf<InPredicate>());

            predicate = factory.Create(PredicateDataSerializerHook.InstanceofPredicate);
            Assert.That(predicate, Is.InstanceOf<InstanceofPredicate>());

            predicate = factory.Create(PredicateDataSerializerHook.LikePredicate);
            Assert.That(predicate, Is.InstanceOf<LikePredicate>());

            predicate = factory.Create(PredicateDataSerializerHook.NotEqualPredicate);
            Assert.That(predicate, Is.InstanceOf<NotEqualPredicate>());

            predicate = factory.Create(PredicateDataSerializerHook.NotPredicate);
            Assert.That(predicate, Is.InstanceOf<NotPredicate>());

            predicate = factory.Create(PredicateDataSerializerHook.OrPredicate);
            Assert.That(predicate, Is.InstanceOf<OrPredicate>());

            predicate = factory.Create(PredicateDataSerializerHook.PagingPredicate);
            Assert.That(predicate, Is.InstanceOf<PagingPredicate>());

            predicate = factory.Create(PredicateDataSerializerHook.PartitionPredicate);
            Assert.That(predicate, Is.InstanceOf<PartitionPredicate>());

            predicate = factory.Create(PredicateDataSerializerHook.RegexPredicate);
            Assert.That(predicate, Is.InstanceOf<RegexPredicate>());

            predicate = factory.Create(PredicateDataSerializerHook.SqlPredicate);
            Assert.That(predicate, Is.InstanceOf<SqlPredicate>());

            predicate = factory.Create(PredicateDataSerializerHook.TruePredicate);
            Assert.That(predicate, Is.InstanceOf<TruePredicate>());
        }
    }
}
