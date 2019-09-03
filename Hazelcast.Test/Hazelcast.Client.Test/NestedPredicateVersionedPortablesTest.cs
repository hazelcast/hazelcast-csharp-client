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
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    [Category("3.8")]
    public class NestedPredicateVersionedPortablesTest : SingleMemberBaseTest
    {
        private IMap<int, Body> _map;

        [SetUp]
        public void Init()
        {
            _map = Client.GetMap<int, Body>("map");
        }

        [TearDown]
        public void Destroy()
        {
            _map.Clear();
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
        public void AddingIndexes()
        {
            // single-attribute index
            _map.AddIndex("name", true);
            // nested-attribute index
            _map.AddIndex("limb.name", true);
        }
        
        [Test]
        public void SingleAttributeQuery_VersionedPortables_predicates()
        {
            // GIVEN
            _map.Put(1, new Body("body1", new Limb("hand")));
            _map.Put(2, new Body("body2", new Limb("leg")));

            // WHEN
            var predicate = Predicates.Property("limb.name").Equal("hand");
            var values = _map.Values(predicate);

            //THEN
            Assert.AreEqual(1, values.Count);
            var bt = new Body[1];
            values.CopyTo(bt, 0);
            Assert.AreEqual("body1", bt[0].Name);
        }

        [Test]
        public void NestedAttributeQuery_DistributedSql()
        {
            // GIVEN
            _map.Put(1, new Body("body1", new Limb("hand")));
            _map.Put(2, new Body("body2", new Limb("leg")));

            // WHEN
            var values = _map.Values(new SqlPredicate("limb.name == 'leg'"));

            // THEN
            Assert.AreEqual(1, values.Count);
            var bt = new Body[1];
            values.CopyTo(bt, 0); 
            Assert.AreEqual("body2", bt[0].Name);
        }

        private class Body : IVersionedPortable
        {
            private string _name;
            private Limb _limb;

            public Body(string name, Limb limb)
            {
                _name = name;
                _limb = limb;
            }

            public Body()
            {

            }

            public string Name => _name;
            public Limb Limb => _limb;

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
                var body = (Body)obj;
                return !(_limb != null ? !_limb.Equals(body._limb) : body._limb != null);
            }

            public override int GetHashCode()
            {
                var result = _name != null ? _name.GetHashCode() : 0;
                result = 31 * result + (_limb != null ? _limb.GetHashCode() : 0);
                return result;
            }

            public override string ToString()
            {
                return "Body{"
                    + "name='" + _name + '\''
                    + ", limb=" + _limb
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
                _name = reader.ReadUTF("name");
                _limb = reader.ReadPortable<Limb>("limb");
            }

            public void WritePortable(IPortableWriter writer)
            {
                writer.WriteUTF("name", _name);
                writer.WritePortable("limb", _limb);
            }
        }

        private class Limb : IVersionedPortable
        {
            private string _name;

            public Limb(string name)
            {
                _name = name;
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
                var limb = (Limb) obj;
                return !(_name != null ? !_name.Equals(limb._name) : limb._name != null);
            }

            public override int GetHashCode()
            {
                return _name != null  ? _name.GetHashCode() : 0;
            }

            public override string ToString()
            {
                return "Limb{"
                    + "name='" + _name + '\''
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
                _name = reader.ReadUTF("name");
            }

            public void WritePortable(IPortableWriter writer)
            {
                writer.WriteUTF("name", _name);
            }
        }
    }
}