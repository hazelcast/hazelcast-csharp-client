using System;
using System.Collections.Generic;
using System.Reflection;
using Hazelcast.Core;
using Hazelcast.Predicates;
using Hazelcast.Serialization;
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
            AssertPredicate(Predicate.True(), PredicateDataSerializerHook.TruePredicate);
            AssertPredicate(Predicate.False(), PredicateDataSerializerHook.FalsePredicate);

            AssertPredicate(Predicate.Not(Predicate.True()), PredicateDataSerializerHook.NotPredicate);
            AssertPredicate(Predicate.True().Not(), PredicateDataSerializerHook.NotPredicate);

            AssertPredicate(Predicate.And(), PredicateDataSerializerHook.AndPredicate);
            AssertPredicate(Predicate.True().And(Predicate.False()), PredicateDataSerializerHook.AndPredicate);

            AssertPredicate(Predicate.Or(), PredicateDataSerializerHook.OrPredicate);
            AssertPredicate(Predicate.True().Or(Predicate.False()), PredicateDataSerializerHook.OrPredicate);

            AssertPredicate(Predicate.IsEqual("name", 33), PredicateDataSerializerHook.EqualPredicate);
            AssertPredicate(Predicate.Property("name").Equal(33), PredicateDataSerializerHook.EqualPredicate);
            AssertPredicate(Predicate.IsNotEqual("name", 33), PredicateDataSerializerHook.NotEqualPredicate);
            AssertPredicate(Predicate.Property("name").NotEqual(33), PredicateDataSerializerHook.NotEqualPredicate);

            AssertPredicate(Predicate.IsGreaterThan("name", 33), PredicateDataSerializerHook.GreaterLessPredicate);
            AssertPredicate(Predicate.Property("name").GreaterThan(33), PredicateDataSerializerHook.GreaterLessPredicate);
            AssertPredicate(Predicate.IsGreaterThanOrEqual("name", 33), PredicateDataSerializerHook.GreaterLessPredicate);
            AssertPredicate(Predicate.Property("name").GreaterThanOrEqual(33), PredicateDataSerializerHook.GreaterLessPredicate);
            AssertPredicate(Predicate.IsLessThan("name", 33), PredicateDataSerializerHook.GreaterLessPredicate);
            AssertPredicate(Predicate.Property("name").LessThan(33), PredicateDataSerializerHook.GreaterLessPredicate);
            AssertPredicate(Predicate.IsLessThanOrEqual("name", 33), PredicateDataSerializerHook.GreaterLessPredicate);
            AssertPredicate(Predicate.Property("name").LessThanOrEqual(33), PredicateDataSerializerHook.GreaterLessPredicate);

            AssertPredicate(Predicate.IsBetween("name", 33, 44), PredicateDataSerializerHook.BetweenPredicate);
            AssertPredicate(Predicate.Property("name").Between(33, 44), PredicateDataSerializerHook.BetweenPredicate);

            AssertPredicate(Predicate.IsIn("name", 33, 44), PredicateDataSerializerHook.InPredicate);
            AssertPredicate(Predicate.Property("name").In(33, 44), PredicateDataSerializerHook.InPredicate);

            AssertPredicate(Predicate.IsLike("name", "expression"), PredicateDataSerializerHook.LikePredicate);
            AssertPredicate(Predicate.Property("name").Like("expression"), PredicateDataSerializerHook.LikePredicate);
            AssertPredicate(Predicate.IsILike("name", "expression"), PredicateDataSerializerHook.ILikePredicate);
            AssertPredicate(Predicate.Property("name").ILike("expression"), PredicateDataSerializerHook.ILikePredicate);
            AssertPredicate(Predicate.MatchesRegex("name", "regex"), PredicateDataSerializerHook.RegexPredicate);
            AssertPredicate(Predicate.Property("name").MatchesRegex("regex"), PredicateDataSerializerHook.RegexPredicate);

            AssertPredicate(Predicate.InstanceOf("className"), PredicateDataSerializerHook.InstanceofPredicate);

            AssertPredicate(Predicate.IsIn("name", 1, 2, 3), PredicateDataSerializerHook.InPredicate);

            AssertPredicate(Predicate.Sql("sql"), PredicateDataSerializerHook.SqlPredicate);

            AssertPredicate(Predicate.Key("property").ILike("expression"), PredicateDataSerializerHook.ILikePredicate);
            AssertPredicate(Predicate.This().ILike("expression"), PredicateDataSerializerHook.ILikePredicate);

            Assert.Throws<ArgumentNullException>(() => ((PredicateProperty) null).In(1, 2));
            Assert.Throws<ArgumentNullException>(() => ((PredicateProperty) null).Between(1, 2));
            Assert.Throws<ArgumentNullException>(() => ((PredicateProperty) null).Equal(3));
            Assert.Throws<ArgumentNullException>(() => ((PredicateProperty) null).NotEqual(3));
            Assert.Throws<ArgumentNullException>(() => ((PredicateProperty) null).GreaterThan(3));
            Assert.Throws<ArgumentNullException>(() => ((PredicateProperty) null).GreaterThanOrEqual(3));
            Assert.Throws<ArgumentNullException>(() => ((PredicateProperty) null).LessThan(3));
            Assert.Throws<ArgumentNullException>(() => ((PredicateProperty) null).LessThanOrEqual(3));
            Assert.Throws<ArgumentNullException>(() => ((PredicateProperty) null).Like("a"));
            Assert.Throws<ArgumentNullException>(() => ((PredicateProperty) null).ILike("a"));
            Assert.Throws<ArgumentNullException>(() => ((PredicateProperty) null).MatchesRegex("a"));
        }

        [Test]
        public void PagingPredicateTest()
        {
            AssertPredicate(new PagingPredicate(3, Predicate.True()), PredicateDataSerializerHook.PagingPredicate);
            AssertPredicate(new PagingPredicate(3, Predicate.True(), Predicate.Comparer()), PredicateDataSerializerHook.PagingPredicate);

            var paging = new PagingPredicate(3, Predicate.True());
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
            AssertPredicate(new PartitionPredicate("key", Predicate.True()), PredicateDataSerializerHook.PartitionPredicate);

            var partition = new PartitionPredicate("key", Predicate.True());

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

        private T AssertPredicate<T>(T predicate, int classId)
            where T : IPredicate
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
