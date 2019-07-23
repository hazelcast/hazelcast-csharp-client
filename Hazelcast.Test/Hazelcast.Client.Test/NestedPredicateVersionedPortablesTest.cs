// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Numerics;
using Hazelcast.Client.Model;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    [Category("3.8")]
    public class NestedPredicateVersionedPortablesTest : SingleMemberBaseTest
    {
        private static IMap<int, Body> map;

        [SetUp]
        public void Init()
        {
            map = Client.GetMap<int, Body>("map");
        }

        [TearDown]
        public static void Destroy()
        {
            map.Clear();
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            config.GetSerializationConfig().AddPortableFactory(1, new SimplePortableFactory());
        }

        private class SimplePortableFactory : IPortableFactory
        {
            public IPortable Create(int classId)
            {
                switch (classId)
                {
                    case 1:
                        return new Body();
                    case 2:
                        return new Limb();
                    default:
                        throw new InvalidOperationException("Wrong class ID");
                }
            }
        }


        [Test]
        public void addingIndexes()
        {
            // single-attribute index
            map.AddIndex("name", true);
            // nested-attribute index
            map.AddIndex("limb.name", true);
        }
        
        
        [Test]
        public void singleAttributeQuery_versionedProtables_predicates()
        {
            // GIVEN
            map.Put(1, new Body("body1", new Limb("hand")));
            map.Put(2, new Body("body2", new Limb("leg")));

            // WHEN
            IPredicate predicate = Predicates.Property("limb.name").Equal("hand");
            ICollection<Body> values = map.Values(predicate);

            //THEN
            Assert.AreEqual(1, values.Count);
            Body[] bt = new Body[1];
            values.CopyTo(bt, 0);
            Assert.AreEqual("body1", bt[0].getName());
        }

        [Test]
        public void nestedAttributeQuery_distributedSql()
        {
            // GIVEN
            map.Put(1, new Body("body1", new Limb("hand")));
            map.Put(2, new Body("body2", new Limb("leg")));

            // WHEN
            ICollection<Body> values = map.Values(new SqlPredicate("limb.name == 'leg'"));

            // THEN
            Assert.AreEqual(1, values.Count);
            Body[] bt = new Body[1];
            values.CopyTo(bt, 0); 
            Assert.AreEqual("body2", bt[0].getName());
        }

        private class Body : IVersionedPortable
        {

            private String name;
            private Limb limb;

            public Body(String name, Limb limb)
            {
                this.name = name;
                this.limb = limb;
            }

            public Body()
            {

            }

            public String getName()
            {
                return name;
            }

            public Limb getLimb()
            {
                return limb;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }
                if (obj == null || !GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                Body body = (Body)obj;
                return !(limb != null ? !limb.Equals(body.limb) : body.limb != null);
            }

            public override int GetHashCode()
            {
                int result = name != null ? name.GetHashCode() : 0;
                result = 31 * result + (limb != null ? limb.GetHashCode() : 0);
                return result;
            }

            public override string ToString()
            {
                return "Body{"
                    + "name='" + name + '\''
                    + ", limb=" + limb
                    + '}';
            }

            public int GetClassId()
            {
                return 1;
            }

            public int GetClassVersion()
            {
                return 15;
            }

            public int GetFactoryId()
            {
                return 1;
            }

            public void ReadPortable(IPortableReader reader)
            {
                name = reader.ReadUTF("name");
                limb = reader.ReadPortable<Limb>("limb");
            }

            public void WritePortable(IPortableWriter writer)
            {
                writer.WriteUTF("name", name);
                writer.WritePortable("limb", limb);
            }
        }

        private class Limb : IVersionedPortable
        {
            private String name;

            public Limb(String name)
            {
                this.name = name;
            }

            public Limb()
            {
            }

            public override bool Equals(object obj)
            {
                if(ReferenceEquals(this, obj))
                {
                    return true;
                }
                if(obj == null || !GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                Limb limb = (Limb) obj;
                return !(name != null ? !name.Equals(limb.name) : limb.name != null);
            }

            public override int GetHashCode()
            {
                return name != null  ? name.GetHashCode() : 0;
            }

            public override string ToString()
            {
                return "Limb{"
                    + "name='" + name + '\''
                    + '}';
            }

            public int GetClassId()
            {
                return 2;
            }

            public int GetClassVersion()
            {
                return 2;
            }

            public int GetFactoryId()
            {
                return 1;
            }

            public void ReadPortable(IPortableReader reader)
            {
                name = reader.ReadUTF("name");
            }

            public void WritePortable(IPortableWriter writer)
            {
                writer.WriteUTF("name", name);
            }
        }

    }
}