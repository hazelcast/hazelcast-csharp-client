using System;
using System.Threading.Tasks;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Serialization
{
    public class SerializationConcurrencyTest
    {
        internal const short FactoryId = 1;
        internal readonly Random rand = new Random();

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="System.Exception" />
        [Test]
        public virtual void Test()
        {
            var portableFactory = new PortableFactoryFunc(i =>
            {
                if (i == 1) return new PortablePerson();
                if (i == 2) return new PortableAddress();
                throw new ArgumentException();
            });

            var ss = new SerializationServiceBuilder()
                .AddPortableFactory(FactoryId, portableFactory).Build();
            var k = 10;
            var tasks = new Task[k];
            for (var i = 0; i < k; i++)
            {
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    for (var j = 0; j < 10000; j++)
                    {
                        var key = "key" + Rnd();
                        var dataKey = ss.ToData(key);
                        Assert.AreEqual(key, ss.ToObject<string>(dataKey));
                        var value = 123L + Rnd();
                        var dataValue = ss.ToData(value);
                        Assert.AreEqual(value, ss.ToObject<long>(dataValue));
                        var address = new Address("here here" + Rnd(), 13131 + Rnd());
                        var dataAddress = ss.ToData(address);
                        Assert.AreEqual(address, ss.ToObject<Address>(dataAddress));
                        var person = new Person(13 + Rnd(), 199L + Rnd(), 56.89d, "mehmet", address);
                        var dataPerson = ss.ToData(person);
                        Assert.AreEqual(person, ss.ToObject<Person>(dataPerson));
                        var portableAddress = new PortableAddress("there there " + Rnd(), 90909 + Rnd());
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
            return rand.Next();
        }
    }

    internal class PortableAddress : IPortable
    {
        private int no;
        private string street;

        public PortableAddress()
        {
        }

        public PortableAddress(string street, int no)
        {
            this.street = street;
            this.no = no;
        }

        public virtual int GetClassId()
        {
            return 2;
        }

        public virtual int GetFactoryId()
        {
            return SerializationConcurrencyTest.FactoryId;
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void ReadPortable(IPortableReader reader)
        {
            street = reader.ReadUTF("street");
            no = reader.ReadInt("no");
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteInt("no", no);
            writer.WriteUTF("street", street);
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

    internal class Address : IDataSerializable
    {
        private int no;
        private string street;

        public Address()
        {
        }

        public Address(string street, int no)
        {
            this.street = street;
            this.no = no;
        }

        public virtual void WriteData(IObjectDataOutput @out)
        {
            @out.WriteUTF(street);
            @out.WriteInt(no);
        }

        public virtual void ReadData(IObjectDataInput @in)
        {
            street = @in.ReadUTF();
            no = @in.ReadInt();
        }

        public string GetJavaClassName()
        {
            return typeof (Address).FullName;
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
            return string.Format("No: {0}, Street: {1}", no, street);
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
        {
        }

        public PortablePerson(int age, long height, string name, PortableAddress address)
        {
            this.age = age;
            this.height = height;
            this.name = name;
            this.address = address;
        }

        public virtual int GetClassId()
        {
            return 1;
        }

        public virtual int GetFactoryId()
        {
            return SerializationConcurrencyTest.FactoryId;
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("name");
            address = reader.ReadPortable<PortableAddress>("address");
            height = reader.ReadLong("height");
            age = reader.ReadInt("age");
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("height", height);
            writer.WriteInt("age", age);
            writer.WriteUTF("name", name);
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

    internal class Person : IDataSerializable
    {
        private Address address;
        private int age;
        private long height;
        private string name;
        private double weight;

        public Person()
        {
        }

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
            @out.WriteUTF(name);
            @out.WriteObject(address);
            @out.WriteInt(age);
            @out.WriteLong(height);
            @out.WriteDouble(weight);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void ReadData(IObjectDataInput @in)
        {
            name = @in.ReadUTF();
            address = @in.ReadObject<Address>();
            age = @in.ReadInt();
            height = @in.ReadLong();
            weight = @in.ReadDouble();
        }

        public string GetJavaClassName()
        {
            return typeof (Person).FullName;
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