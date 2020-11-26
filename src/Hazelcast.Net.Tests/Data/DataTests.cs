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
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Networking;
using Hazelcast.Serialization;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Data
{
    [TestFixture]
    public class DataTests
    {
        [Test]
        public void BitmapIndexOptionsTest()
        {
            var x = new BitmapIndexOptions
            {
                UniqueKey = "uniqueKey",
                UniqueKeyTransformation = UniqueKeyTransformation.Long
            };

            Assert.That(x.UniqueKey, Is.EqualTo("uniqueKey"));
            Assert.That(x.UniqueKeyTransformation, Is.EqualTo(UniqueKeyTransformation.Long));

            x = new BitmapIndexOptions
            {
                UniqueKey = "anotherKey",
                UniqueKeyTransformation = UniqueKeyTransformation.Raw
            };

            Assert.That(x.UniqueKey, Is.EqualTo("anotherKey"));
            Assert.That(x.UniqueKeyTransformation, Is.EqualTo(UniqueKeyTransformation.Raw));

            x = new BitmapIndexOptions();

            Assert.That(x.UniqueKey, Is.EqualTo(Hazelcast.Predicates.Query.KeyName));
            Assert.That(x.UniqueKeyTransformation, Is.EqualTo(UniqueKeyTransformation.Object));
        }

        [Test]
        public void MemberVersionTest()
        {
            var x = new MemberVersion(1, 2, 3);

            Assert.That(x.Major, Is.EqualTo(1));
            Assert.That(x.Minor, Is.EqualTo(2));
            Assert.That(x.Patch, Is.EqualTo(3));

            Assert.That(x.ToString(), Is.EqualTo("1.2.3"));
        }

        [Test]
        public void AuthenticationResultTest()
        {
            var clusterId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var address = NetworkAddress.Parse("192.168.33.34:5569");

            var x = new AuthenticationResult(clusterId, memberId, address, "4.5.6", true, 12, 4);

            Assert.That(x.ClusterId, Is.EqualTo(clusterId));
            Assert.That(x.MemberId, Is.EqualTo(memberId));
            Assert.That(x.MemberAddress, Is.SameAs(address));
            Assert.That(x.ServerVersion, Is.EqualTo("4.5.6"));
            Assert.That(x.FailoverSupported);
            Assert.That(x.PartitionCount, Is.EqualTo(12));
            Assert.That(x.SerializationVersion, Is.EqualTo(4));
        }

        [Test]
        public void MemberInfoTest()
        {
            var memberId = Guid.NewGuid();
            var address = NetworkAddress.Parse("192.168.33.34:5569");
            var version = new MemberVersion(4, 5, 6);


            var attributes = new Dictionary<string, string>
            {
                { "attribute", "value" }
            };
            var x = new MemberInfo(memberId, address, version, true, attributes);

            Console.WriteLine(x);

            Assert.That(x.Id, Is.EqualTo(memberId));
            Assert.That(x.Uuid, Is.EqualTo(memberId));
            Assert.That(x.Address, Is.SameAs(address));
            Assert.That(x.Version, Is.SameAs(version));
            Assert.That(x.IsLiteMember, Is.True);
            Assert.That(x.IsLiteMember, Is.True);
            var a = x.Attributes;
            Assert.That(a.Count, Is.EqualTo(1));
            Assert.That(a["attribute"], Is.EqualTo("value"));

            Assert.That(x, Ish.Equatable(
                // weird indeed, but only the ID matters
                new MemberInfo(memberId, x.Address, x.Version, x.IsLiteMember, attributes),
                new MemberInfo(Guid.NewGuid(), x.Address, x.Version, x.IsLiteMember, attributes)
            ));

            var y = new MemberInfo(address, memberId, attributes, true, version, false, null);

            var map = new Dictionary<EndpointQualifier, NetworkAddress>();
            var z = new MemberInfo(address, memberId, attributes, true, version, true, map);
        }

        [Test]
        public void IndexConfigTest()
        {
            var x = new IndexOptions();

            Console.WriteLine(x.ToString());

            x.Name = "name";
            Assert.That(x.Name, Is.EqualTo("name"));

            Assert.That(x.Type, Is.EqualTo(IndexOptions.DefaultType));
            x.Type = IndexType.Hashed;
            Assert.That(x.Type, Is.EqualTo(IndexType.Hashed));

            x = new IndexOptions(new []{ "aaa", "bbb" });
            Assert.That(x.Attributes.Count, Is.EqualTo(2));
            x.AddAttribute("ccc");
            Assert.That(x.Attributes.Count, Is.EqualTo(3));
            Assert.That(x.Attributes, Does.Contain("aaa"));
            Assert.That(x.Attributes, Does.Contain("bbb"));
            Assert.That(x.Attributes, Does.Contain("ccc"));

            Assert.That(x.BitmapIndexOptions, Is.Not.Null);
            var y = new BitmapIndexOptions();
            x.BitmapIndexOptions = y;
            Assert.That(x.BitmapIndexOptions, Is.SameAs(y));

            IndexOptions.ValidateAttribute(x, "flub");

            Assert.Throws<ArgumentNullException>(() => IndexOptions.ValidateAttribute(x, null));
            Assert.Throws<ArgumentException>(() => IndexOptions.ValidateAttribute(x, ""));
            Assert.Throws<ArgumentException>(() => IndexOptions.ValidateAttribute(x, "duh."));
        }

        [Test]
        public void DistributedObjectInfoTest()
        {
            var x = new DistributedObjectInfo("serviceName", "name");

            Console.WriteLine(x);

            Assert.That(x.ServiceName, Is.EqualTo("serviceName"));
            Assert.That(x.Name, Is.EqualTo("name"));

            Assert.That(x, Ish.Equatable(
                new DistributedObjectInfo("serviceName", "name"),
                new DistributedObjectInfo("serviceName", "other"),
                new DistributedObjectInfo("other", "name")));

            var hash = x.GetHashCode();
            Assert.That(hash, Is.Not.Zero);
        }

        [Test]
        public void MapEntryTest()
        {
            var x = new MapEntryStats<string, string>();

            x.Cost = 12;
            Assert.That(x.Cost, Is.EqualTo(12));

            x.CreationTime = 12345;
            Assert.That(x.CreationTime, Is.EqualTo(12345));

            x.EvictionCriteriaNumber = 654;
            Assert.That(x.EvictionCriteriaNumber, Is.EqualTo(654));

            x.ExpirationTime = 4747;
            Assert.That(x.ExpirationTime, Is.EqualTo(4747));

            x.Hits = 987;
            Assert.That(x.Hits, Is.EqualTo(987));

            x.LastAccessTime = 6969;
            Assert.That(x.LastAccessTime, Is.EqualTo(6969));

            x.LastStoredTime = 3211;
            Assert.That(x.LastStoredTime, Is.EqualTo(3211));

            x.LastUpdateTime = 159;
            Assert.That(x.LastUpdateTime, Is.EqualTo(159));

            x.Ttl = 951;
            Assert.That(x.Ttl, Is.EqualTo(951));

            x.Version = 5554;
            Assert.That(x.Version, Is.EqualTo(5554));

            x.MaxIdle = 6644;
            Assert.That(x.MaxIdle, Is.EqualTo(6644));

            x.Key = "key";
            Assert.That(x.Key, Is.EqualTo("key"));

            x.Value = "value";
            Assert.That(x.Value, Is.EqualTo("value"));
        }

        [Test]
        public void EndpointQualifierTest()
        {
            var q = new EndpointQualifier(ProtocolType.Client, "identifier");

            Assert.That(q.Type, Is.EqualTo(ProtocolType.Client));
            Assert.That(q.Identifier, Is.EqualTo("identifier"));
            Assert.That(q.FactoryId, Is.EqualTo(0));
            Assert.That(q.ClassId, Is.EqualTo(17));

            Assert.That(q, Ish.Equatable(
                new EndpointQualifier(ProtocolType.Client, "identifier"),
                new EndpointQualifier(ProtocolType.MemCache, "identifier"),
                new EndpointQualifier(ProtocolType.Client, "other")));

            Assert.That(q.GetHashCode(), Is.Not.Zero);

            Assert.Throws<ArgumentNullException>(() => q.WriteData(null));
            Assert.Throws<ArgumentNullException>(() => q.ReadData(null));

            var output = new ByteArrayObjectDataOutput(1024, null, Endianness.Unspecified);
            q.WriteData(output);
            var input = new ByteArrayObjectDataInput(output.ToByteArray(), null, Endianness.Unspecified);
            var r = new EndpointQualifier(ProtocolType.Wan, "meh");
            r.ReadData(input);

            Assert.That(q, Is.EqualTo(r));
        }
    }
}
