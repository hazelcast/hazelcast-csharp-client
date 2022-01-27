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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Serialization.ConstantSerializers;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization
{
    public class SerializationConcurrencyTest
    {
        internal const short FactoryId = 1;

        [Test]
        public virtual void Test()
        {
            var portableFactory = new PortableFactoryFunc(i => i switch
            {
                1 => new PortablePerson(),
                2 => new PortableAddress(),
                _ => throw new ArgumentException()
            });

            var ss = new SerializationServiceBuilder(new NullLoggerFactory())
                .AddDefinitions(new ConstantSerializerDefinitions()) // use constant serializers not CLR serialization
                .AddPortableFactory(FactoryId, portableFactory)
                .AddDataSerializableFactory(FactoryId, new ArrayDataSerializableFactory(new Func<IIdentifiedDataSerializable>[]
                {
                    () => new Address(),
                    () => new Person(),
                }))
                .Build();

            const int tasksCount = 10;
            var tasks = new Task[tasksCount];
            for (var i = 0; i < tasksCount; i++)
            {
                var ti = i;
                tasks[i] = Task.Run(() =>
                {
                    for (var j = 0; j < 10000; j++)
                    {
                        var key = $"key-{ti}-{j}-{Rnd()}";
                        var dataKey = ss.ToData(key);
                        Assert.AreEqual(key, ss.ToObject<string>(dataKey));

                        var other = $"other-{ti}-{j}-{Rnd()}";
                        var dataOther = ss.ToData(other);
                        Assert.AreEqual(other, ss.ToObject<string>(dataOther));

                        var value = 123L + Rnd();
                        var dataValue = ss.ToData(value);
                        Assert.AreEqual(value, ss.ToObject<long>(dataValue));

                        var address = new Address($"here-{ti}-{j}-{Rnd()}", 13131 + Rnd());
                        var dataAddress = ss.ToData(address);
                        Assert.AreEqual(address, ss.ToObject<Address>(dataAddress));

                        var person = new Person(13 + Rnd(), 199L + Rnd(), 56.89d, "mehmet", address);
                        var dataPerson = ss.ToData(person);
                        Assert.AreEqual(person, ss.ToObject<Person>(dataPerson));

                        var portableAddress = new PortableAddress($"there-{ti}-{j}-{Rnd()}", 90909 + Rnd());
                        var dataPortableAddress = ss.ToData(portableAddress);
                        Assert.AreEqual(portableAddress, ss.ToObject<PortableAddress>(dataPortableAddress));

                        var portablePerson = new PortablePerson(63 + Rnd(), 167L + Rnd(), "ahmet", portableAddress);
                        var dataPortablePerson = ss.ToData(portablePerson);
                        Assert.AreEqual(portablePerson, ss.ToObject<PortablePerson>(dataPortablePerson));
                    }
                });
            }

            Task.WaitAll(tasks, new TimeSpan(0, 0, 0, 30));
        }

        private int Rnd()
        {
            return RandomProvider.Random.Next();
        }
    }

    internal class PortableAddress : IPortable
    {
        private int no;
        private string street;

        public PortableAddress()
        { }

        public PortableAddress(string street, int no)
        {
            this.street = street;
            this.no = no;
        }

        public virtual int ClassId => 2;

        public virtual int FactoryId => SerializationConcurrencyTest.FactoryId;

        /// <exception cref="System.IO.IOException" />
        public virtual void ReadPortable(IPortableReader reader)
        {
            street = reader.ReadString("street");
            no = reader.ReadInt("no");
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteInt("no", no);
            writer.WriteString("street", street);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PortableAddress) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (no*397) ^ (street != null ? street.GetHashCode() : 0);
            }
        }

        protected bool Equals(PortableAddress other)
        {
            return no == other.no && string.Equals(street, other.street);
        }
    }

    internal class Address : IIdentifiedDataSerializable
    {
        private int no;
        private string street;

        public Address()
        { }

        public Address(string street, int no)
        {
            this.street = street;
            this.no = no;
        }

        public virtual void WriteData(IObjectDataOutput @out)
        {
            @out.WriteString(street);
            @out.WriteInt(no);
        }

        public int FactoryId => SerializationConcurrencyTest.FactoryId;

        public int ClassId => 0;

        public virtual void ReadData(IObjectDataInput @in)
        {
            street = @in.ReadString();
            no = @in.ReadInt();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Address) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((street != null ? street.GetHashCode() : 0)*397) ^ no;
            }
        }

        public override string ToString()
        {
            return $"No: {no}, Street: {street}";
        }

        protected bool Equals(Address other)
        {
            return string.Equals(street, other.street) && no == other.no;
        }
    }

    internal class PortablePerson : IPortable
    {
        private PortableAddress address;
        private int age;
        private long height;
        private string name;

        public PortablePerson()
        { }

        public PortablePerson(int age, long height, string name, PortableAddress address)
        {
            this.age = age;
            this.height = height;
            this.name = name;
            this.address = address;
        }

        public virtual int ClassId => 1;

        public virtual int FactoryId => SerializationConcurrencyTest.FactoryId;

        /// <exception cref="System.IO.IOException" />
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadString("name");
            address = reader.ReadPortable<PortableAddress>("address");
            height = reader.ReadLong("height");
            age = reader.ReadInt("age");
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("height", height);
            writer.WriteInt("age", age);
            writer.WriteString("name", name);
            writer.WritePortable("address", address);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PortablePerson) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (address != null ? address.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ age;
                hashCode = (hashCode*397) ^ height.GetHashCode();
                hashCode = (hashCode*397) ^ (name != null ? name.GetHashCode() : 0);
                return hashCode;
            }
        }

        protected bool Equals(PortablePerson other)
        {
            return Equals(address, other.address) && age == other.age && height == other.height &&
                   string.Equals(name, other.name);
        }
    }

    internal class Person : IIdentifiedDataSerializable
    {
        private Address address;
        private int age;
        private long height;
        private string name;
        private double weight;

        public Person()
        { }

        public Person(int age, long height, double weight, string name, Address address)
        {
            this.age = age;
            this.height = height;
            this.weight = weight;
            this.name = name;
            this.address = address;
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteData(IObjectDataOutput @out)
        {
            @out.WriteString(name);
            @out.WriteObject(address);
            @out.WriteInt(age);
            @out.WriteLong(height);
            @out.WriteDouble(weight);
        }

        public int FactoryId => SerializationConcurrencyTest.FactoryId;

        public int ClassId => 1;

        /// <exception cref="System.IO.IOException" />
        public virtual void ReadData(IObjectDataInput @in)
        {
            name = @in.ReadString();
            address = @in.ReadObject<Address>();
            age = @in.ReadInt();
            height = @in.ReadLong();
            weight = @in.ReadDouble();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Person) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = age;
                hashCode = (hashCode*397) ^ height.GetHashCode();
                hashCode = (hashCode*397) ^ weight.GetHashCode();
                hashCode = (hashCode*397) ^ (name != null ? name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (address != null ? address.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("Address: {0}, Age: {1}, Height: {2}, Name: {3}, Weight: {4}", address, age, height,
                name, weight);
        }

        protected bool Equals(Person other)
        {
            return age == other.age && height == other.height && weight.Equals(other.weight) &&
                   string.Equals(name, other.name) && Equals(address, other.address);
        }
    }
}
