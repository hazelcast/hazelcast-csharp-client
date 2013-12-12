using System;
using System.Linq;
using System.Text;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using NUnit.Framework;

namespace Hazelcast.Test
{
    [TestFixture]
    public class PortableTest
    {
        public const int FACTORY_ID = 1;

        private void testBasics(bool bigEndian)
        {
            ISerializationService serializationService = createSerializationService(1, bigEndian);
            ISerializationService serializationService2 = createSerializationService(2, bigEndian);
            Data data;

            var nn = new NamedPortable[5];
            for (int i = 0; i < nn.Length; i++)
            {
                nn[i] = new NamedPortable("named-portable-" + i, i);
            }

            NamedPortable np = nn[0];
            data = serializationService.ToData(np);
            Assert.AreEqual(np, serializationService.ToObject(data));
            Assert.AreEqual(np, serializationService2.ToObject(data));

            var inner = new InnerPortable(new byte[] {0, 1, 2}, new[] {'c', 'h', 'a', 'r'},
                new short[] {3, 4, 5}, new[] {9, 8, 7, 6}, new long[] {0, 1, 5, 7, 9, 11},
                new[] {0.6543f, -3.56f, 45.67f}, new[] {456.456, 789.789, 321.321}, nn);

            data = serializationService.ToData(inner);
            Assert.AreEqual(inner, serializationService.ToObject(data));
            Assert.AreEqual(inner, serializationService2.ToObject(data));

            var main = new MainPortable(113, true, 'x', -500, 56789, -50992225L, 900.5678f,
                -897543.3678909d, "this is main portable object created for testing!", inner);

            data = serializationService.ToData(main);
            Assert.AreEqual(main, serializationService.ToObject(data));
            Assert.AreEqual(main, serializationService2.ToObject(data));
        }

        private ISerializationService createSerializationService(int version)
        {
            return createSerializationService(version, true);
        }

        private ISerializationService createSerializationService(int version, bool order)
        {
            return new SerializationServiceBuilder()
                .SetUseNativeByteOrder(false)
                .SetBigEndian(order)
                .SetVersion(version)
                .AddPortableFactory(FACTORY_ID, new TestPortableFactory())
                .Build();
        }

        [Test]
        public void testBasics()
        {
            testBasics(true/*BIG-ENDIAN*/);
        }

        //[Test]
        public void testBasicsLittleEndian()
        {
            testBasics(/*ByteOrder.LITTLE_ENDIAN*/ false);
        }




    //[Test]
    //public void testDifferentVersions() {
    //      SerializationService serializationService = new SerializationServiceBuilder().setVersion(1).addPortableFactory(FACTORY_ID, new IPortableFactory() {
    //        public Portable create(int classId) {
    //            return new NamedPortable();
    //        }

    //    }).build();

    //      SerializationService serializationService2 = new SerializationServiceBuilder().setVersion(2).addPortableFactory(FACTORY_ID, new PortableFactory() {
    //        public Portable create(int classId) {
    //            return new NamedPortableV2();
    //        }
    //    }).build();

    //    NamedPortable p1 = new NamedPortable("portable-v1", 111);
    //    Data data = serializationService.ToData(p1);

    //    NamedPortableV2 p2 = new NamedPortableV2("portable-v2", 123);
    //    Data data2 = serializationService2.ToData(p2);

    //    serializationService2.Toobject(data);
    //    serializationService.Toobject(data2);
    //}

    //[Test]
    //public void testPreDefinedDifferentVersions() {
    //    ClassDefinitionBuilder builder = new ClassDefinitionBuilder(FACTORY_ID, InnerPortable.CLASS_ID);
    //    builder.addByteArrayField("b");
    //    builder.addCharArrayField("c");
    //    builder.addShortArrayField("s");
    //    builder.addIntArrayField("i");
    //    builder.addLongArrayField("l");
    //    builder.addFloatArrayField("f");
    //    builder.addDoubleArrayField("d");
    //    ClassDefinition cd = createNamedPortableClassDefinition();
    //    builder.addPortableArrayField("nn", cd);

    //      SerializationService serializationService = createSerializationService(1);
    //    serializationService.getSerializationContext().registerClassDefinition(builder.build());

    //      SerializationService serializationService2 = createSerializationService(2);
    //    serializationService2.getSerializationContext().registerClassDefinition(builder.build());

    //      MainPortable mainWithNullInner = new MainPortable((byte) 113, true, 'x', (short) -500, 56789, -50992225L, 900.5678f,
    //            -897543.3678909d, "this is main portable object created for testing!", null);

    //      Data data = serializationService.ToData(mainWithNullInner);
    //    Assert.AreEqual(mainWithNullInner, serializationService2.Toobject(data));

    //    NamedPortable[] nn = new NamedPortable[1];
    //    nn[0] = new NamedPortable("name", 123);
    //    InnerPortable inner = new InnerPortable(new byte[]{0, 1, 2}, new char[]{'c', 'h', 'a', 'r'},
    //            new short[]{3, 4, 5}, new int[]{9, 8, 7, 6}, new long[]{0, 1, 5, 7, 9, 11},
    //            new float[]{0.6543f, -3.56f, 45.67f}, new double[]{456.456, 789.789, 321.321}, nn);

    //      MainPortable mainWithInner = new MainPortable((byte) 113, true, 'x', (short) -500, 56789, -50992225L, 900.5678f,
    //            -897543.3678909d, "this is main portable object created for testing!", inner);

    //      Data data2 = serializationService.ToData(mainWithInner);
    //    Assert.AreEqual(mainWithInner, serializationService2.Toobject(data2));
    //}

        private IClassDefinition createNamedPortableClassDefinition()
        {
            ClassDefinitionBuilder builder2 = new ClassDefinitionBuilder(FACTORY_ID, NamedPortable.CLASS_ID);
            builder2.AddUTFField("name");
            builder2.AddIntField("myint");
            return builder2.Build();
        }

        [Test]
        public void testRawData() {

            var className = "Hazelcast.Test.SimpleDataSerializable";
            var type = Type.GetType(className, false, true);
          
            ISerializationService serializationService = createSerializationService(1);
            RawDataPortable p = new RawDataPortable(new DateTime().Ticks, "test chars".ToCharArray(),
                    new NamedPortable("named portable", 34567),
                    9876, "Testing raw portable", new SimpleDataSerializable(Encoding.UTF8.GetBytes("test bytes")));
            ClassDefinitionBuilder builder = new ClassDefinitionBuilder(p.GetFactoryId(), p.GetClassId());
            builder.AddLongField("l").AddCharArrayField("c").AddPortableField("p", createNamedPortableClassDefinition());
            serializationService.GetSerializationContext().RegisterClassDefinition(builder.Build());

                Data data = serializationService.ToData(p);
            Assert.AreEqual(p, serializationService.ToObject(data));
        }

    [Test]
    public void testRawDataWithoutRegistering() {
          ISerializationService serializationService = createSerializationService(1);
          RawDataPortable p = new RawDataPortable(new DateTime().Ticks, "test chars".ToCharArray(),
                new NamedPortable("named portable", 34567),
                9876, "Testing raw portable", new SimpleDataSerializable(Encoding.UTF8.GetBytes("test bytes")));

          Data data = serializationService.ToData(p);
        Assert.AreEqual(p, serializationService.ToObject(data));
    }

    //[Test](expected = HazelcastSerializationException.class)
    //public void testRawDataInvalidWrite() {
    //      SerializationService serializationService = createSerializationService(1);
    //    RawDataPortable p = new InvalidRawDataPortable(System.currentTimeMillis(), "test chars".ToCharArray(),
    //            new NamedPortable("named portable", 34567),
    //            9876, "Testing raw portable", new SimpleDataSerializable("test bytes".getBytes()));
    //    ClassDefinitionBuilder builder = new ClassDefinitionBuilder(p.getFactoryId(), p.GetClassId());
    //    builder.addLongField("l").addCharArrayField("c").addPortableField("p", createNamedPortableClassDefinition());
    //    serializationService.getSerializationContext().registerClassDefinition(builder.build());

    //      Data data = serializationService.ToData(p);
    //    Assert.AreEqual(p, serializationService.Toobject(data));
    //}

    //[Test](expected = HazelcastSerializationException.class)
    //public void testRawDataInvalidRead() {
    //      SerializationService serializationService = createSerializationService(1);
    //    RawDataPortable p = new InvalidRawDataPortable2(System.currentTimeMillis(), "test chars".ToCharArray(),
    //            new NamedPortable("named portable", 34567),
    //            9876, "Testing raw portable", new SimpleDataSerializable("test bytes".getBytes()));
    //    ClassDefinitionBuilder builder = new ClassDefinitionBuilder(p.getFactoryId(), p.GetClassId());
    //    builder.addLongField("l").addCharArrayField("c").addPortableField("p", createNamedPortableClassDefinition());
    //    serializationService.getSerializationContext().registerClassDefinition(builder.build());

    //      Data data = serializationService.ToData(p);
    //    Assert.AreEqual(p, serializationService.Toobject(data));
    //}

    //[Test]
    //public void testClassDefinitionConfigWithErrors() throws Exception {
    //    SerializationConfig serializationConfig = new SerializationConfig();
    //    serializationConfig.addPortableFactory(FACTORY_ID, new TestPortableFactory());
    //    serializationConfig.setPortableVersion(1);
    //    serializationConfig.addClassDefinition(
    //            new ClassDefinitionBuilder(FACTORY_ID, RawDataPortable.CLASS_ID)
    //                    .addLongField("l").addCharArrayField("c").addPortableField("p", createNamedPortableClassDefinition()).build());

    //    try {
    //        new SerializationServiceBuilder().setConfig(serializationConfig).build();
    //        fail("Should throw HazelcastSerializationException!");
    //    } catch (HazelcastSerializationException e) {
    //    }

    //    new SerializationServiceBuilder().setConfig(serializationConfig).setCheckClassDefErrors(false).build();

    //    // -- OR --

    //    serializationConfig.setCheckClassDefErrors(false);
    //    new SerializationServiceBuilder().setConfig(serializationConfig).build();
    //}

    //[Test]
    //public void testClassDefinitionConfig() throws Exception {
    //    SerializationConfig serializationConfig = new SerializationConfig();
    //    serializationConfig.addPortableFactory(FACTORY_ID, new TestPortableFactory());
    //    serializationConfig.setPortableVersion(1);
    //    serializationConfig
    //            .addClassDefinition(
    //                new ClassDefinitionBuilder(FACTORY_ID, RawDataPortable.CLASS_ID)
    //                    .addLongField("l").addCharArrayField("c").addPortableField("p", createNamedPortableClassDefinition()).build())
    //            .addClassDefinition(
    //                new ClassDefinitionBuilder(FACTORY_ID, NamedPortable.CLASS_ID)
    //                    .addUTFField("name").addIntField("myint").build()
    //            );

    //    SerializationService serializationService = new SerializationServiceBuilder().setConfig(serializationConfig).build();
    //    RawDataPortable p = new RawDataPortable(System.currentTimeMillis(), "test chars".ToCharArray(),
    //            new NamedPortable("named portable", 34567),
    //            9876, "Testing raw portable", new SimpleDataSerializable("test bytes".getBytes()));

    //      Data data = serializationService.ToData(p);
    //    Assert.AreEqual(p, serializationService.Toobject(data));
    //}

    //[Test]
    //public void testPortableNestedInOthers() {
    //    SerializationService serializationService = createSerializationService(1);
    //    object o1 = new ComplexDataSerializable(new NamedPortable("test-portable", 137),
    //            new SimpleDataSerializable("test-data-serializable".getBytes()),
    //            new SimpleDataSerializable("test-data-serializable-2".getBytes()));

    //    Data data = serializationService.ToData(o1);
    //    SerializationService serializationService2 = createSerializationService(2);
    //    object o2 = serializationService2.Toobject(data);
    //    Assert.AreEqual(o1, o2);
    //}
        
    }

    public class TestPortableFactory : IPortableFactory
    {
        public IPortable Create(int classId)
        {
            switch (classId)
            {
                case MainPortable.CLASS_ID:
                    return new MainPortable();
                case InnerPortable.CLASS_ID:
                    return new InnerPortable();
                case NamedPortable.CLASS_ID:
                    return new NamedPortable();
                case RawDataPortable.CLASS_ID:
                    return new RawDataPortable();
                case InvalidRawDataPortable.CLASS_ID:
                    return new InvalidRawDataPortable();
                case InvalidRawDataPortable2.CLASS_ID:
                    return new InvalidRawDataPortable2();
            }
            return null;
        }
    }

    public class MainPortable : IPortable
    {
        public const int CLASS_ID = 1;

        private byte b;
        private bool bl;
        private char c;
        private double d;
        private float f;
        private int i;
        private long l;
        private InnerPortable p;
        private short s;
        private String str;

        public MainPortable()
        {
        }

        public MainPortable(byte b, bool bl, char c, short s, int i, long l, float f,
            double d, String str, InnerPortable p)
        {
            this.b = b;
            this.bl = bl;
            this.c = c;
            this.s = s;
            this.i = i;
            this.l = l;
            this.f = f;
            this.d = d;
            this.str = str;
            this.p = p;
        }

        public int GetClassId()
        {
            return CLASS_ID;
        }

        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteByte("b", b);
            writer.WriteBoolean("bl", bl);
            writer.WriteChar("c", c);
            writer.WriteShort("s", s);
            writer.WriteInt("i", i);
            writer.WriteLong("l", l);
            writer.WriteFloat("f", f);
            writer.WriteDouble("d", d);
            writer.WriteUTF("str", str);
            if (p != null)
            {
                writer.WritePortable("p", p);
            }
            else
            {
                writer.WriteNullPortable("p", PortableTest.FACTORY_ID, InnerPortable.CLASS_ID);
            }
        }

        public void ReadPortable(IPortableReader reader)
        {
            b = reader.ReadByte("b");
            bl = reader.ReadBoolean("bl");
            c = reader.ReadChar("c");
            s = reader.ReadShort("s");
            i = reader.ReadInt("i");
            l = reader.ReadLong("l");
            f = reader.ReadFloat("f");
            d = reader.ReadDouble("d");
            str = reader.ReadUTF("str");
            p = reader.ReadPortable<InnerPortable>("p");
        }

        public int GetFactoryId()
        {
            return PortableTest.FACTORY_ID;
        }


        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            var that = (MainPortable) o;

            if (b != that.b) return false;
            if (bl != that.bl) return false;
            if (c != that.c) return false;
            if (d != that.d) return false;
            if (f != that.f) return false;
            if (i != that.i) return false;
            if (l != that.l) return false;
            if (s != that.s) return false;
            if (p != null ? !p.Equals(that.p) : that.p != null) return false;
            if (str != null ? !str.Equals(that.str) : that.str != null) return false;

            return true;
        }


        public override int GetHashCode()
        {
            int result;
            long temp;
            result = b;
            result = 31*result + (bl ? 1 : 0);
            result = 31*result + c;
            result = 31*result + s;
            result = 31*result + i;
            result = 31*result + (int) (l ^ (((uint) l) >> 32));
            result = 31*result + (f != +0.0f ? BitConverter.ToInt32(BitConverter.GetBytes(f), 0) : 0);
            temp = d != +0.0d ? BitConverter.DoubleToInt64Bits(d) : 0L;
            result = 31*result + (int) (temp ^ ((long) (((ulong) temp) >> 32)));
            result = 31*result + (str != null ? str.GetHashCode() : 0);
            result = 31*result + (p != null ? p.GetHashCode() : 0);
            return result;
        }
    }

    public class InnerPortable : IPortable
    {
        public const int CLASS_ID = 2;

        private byte[] bb;
        private char[] cc;
        private double[] dd;
        private float[] ff;
        private int[] ii;
        private long[] ll;
        private NamedPortable[] nn;
        private short[] ss;

        public InnerPortable()
        {
        }

        public InnerPortable(byte[] bb, char[] cc, short[] ss, int[] ii, long[] ll,
            float[] ff, double[] dd, NamedPortable[] nn)
        {
            this.bb = bb;
            this.cc = cc;
            this.ss = ss;
            this.ii = ii;
            this.ll = ll;
            this.ff = ff;
            this.dd = dd;
            this.nn = nn;
        }

        public int GetClassId()
        {
            return CLASS_ID;
        }

        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteByteArray("b", bb);
            writer.WriteCharArray("c", cc);
            writer.WriteShortArray("s", ss);
            writer.WriteIntArray("i", ii);
            writer.WriteLongArray("l", ll);
            writer.WriteFloatArray("f", ff);
            writer.WriteDoubleArray("d", dd);
            writer.WritePortableArray("nn", nn);
        }

        public void ReadPortable(IPortableReader reader)
        {
            bb = reader.ReadByteArray("b");
            cc = reader.ReadCharArray("c");
            ss = reader.ReadShortArray("s");
            ii = reader.ReadIntArray("i");
            ll = reader.ReadLongArray("l");
            ff = reader.ReadFloatArray("f");
            dd = reader.ReadDoubleArray("d");
            IPortable[] pp = reader.ReadPortableArray("nn");
            nn = new NamedPortable[pp.Length];
            Array.Copy(pp, 0, nn, 0, nn.Length);
        }

        public int GetFactoryId()
        {
            return PortableTest.FACTORY_ID;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            var that = (InnerPortable) o;
            if (!(bb.SequenceEqual(that.bb))) return false;
            if (!(cc.SequenceEqual(that.cc))) return false;
            if (!(dd.SequenceEqual(that.dd))) return false;
            if (!(ff.SequenceEqual(that.ff))) return false;
            if (!(ii.SequenceEqual(that.ii))) return false;
            if (!(ll.SequenceEqual(that.ll))) return false;
            if (!(nn.SequenceEqual(that.nn))) return false;
            if (!(ss.SequenceEqual(that.ss))) return false;

            return true;
        }

        public override int GetHashCode()
        {
            int result = bb != null ? bb.GetHashCode() : 0;
            result = 31*result + (cc != null ? cc.GetHashCode() : 0);
            result = 31*result + (ss != null ? ss.GetHashCode() : 0);
            result = 31*result + (ii != null ? ii.GetHashCode() : 0);
            result = 31*result + (ll != null ? ll.GetHashCode() : 0);
            result = 31*result + (ff != null ? ff.GetHashCode() : 0);
            result = 31*result + (dd != null ? dd.GetHashCode() : 0);
            result = 31*result + (nn != null ? nn.GetHashCode() : 0);
            return result;
        }
    }

    public class NamedPortable : IPortable
    {
        public const int CLASS_ID = 3;

        private int k;
        private String name;

        public NamedPortable()
        {
        }

        public NamedPortable(String name, int k)
        {
            this.name = name;
            this.k = k;
        }

        public int GetClassId()
        {
            return CLASS_ID;
        }

        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("name", name);
            writer.WriteInt("myint", k);
        }

        public virtual void ReadPortable(IPortableReader reader)
        {
            k = reader.ReadInt("myint");
            name = reader.ReadUTF("name");
        }

        public virtual int GetFactoryId()
        {
            return PortableTest.FACTORY_ID;
        }


        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            var that = (NamedPortable) o;

            if (k != that.k) return false;
            if (name != null ? !name.Equals(that.name) : that.name != null) return false;

            return true;
        }


        public override int GetHashCode()
        {
            int result = name != null ? name.GetHashCode() : 0;
            result = 31*result + k;
            return result;
        }


        public String toString()
        {
            var sb = new StringBuilder("NamedPortable{");
            sb.Append("name='").Append(name).Append('\'');
            sb.Append(", k=").Append(k);
            sb.Append('}');
            return sb.ToString();
        }
    }

    public class NamedPortableV2 : NamedPortable, IPortable
    {
        private int v;

        public NamedPortableV2()
        {
        }

        public NamedPortableV2(int v)
        {
            this.v = v;
        }

        public NamedPortableV2(String name, int v) : base(name, v*10)
        {
            this.v = v;
        }


        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            writer.WriteInt("v", v);
        }


        public override void ReadPortable(IPortableReader reader)
        {
            base.ReadPortable(reader);
            v = reader.ReadInt("v");
        }

        public override int GetFactoryId()
        {
            return PortableTest.FACTORY_ID;
        }
    }

    public class RawDataPortable : IPortable
    {
        public const int CLASS_ID = 4;

        protected char[] c;
        protected int k;
        protected long l;
        protected NamedPortable p;
        protected String s;
        protected SimpleDataSerializable sds;

        public RawDataPortable()
        {
        }

        public RawDataPortable(long l, char[] c, NamedPortable p, int k, String s, SimpleDataSerializable sds)
        {
            this.l = l;
            this.c = c;
            this.p = p;
            this.k = k;
            this.s = s;
            this.sds = sds;
        }

        public int GetClassId()
        {
            return CLASS_ID;
        }

        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("l", l);
            writer.WriteCharArray("c", c);
            writer.WritePortable("p", p);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteInt(k);
            output.WriteUTF(s);
            output.WriteObject(sds);
        }

        public virtual void ReadPortable(IPortableReader reader)
        {
            l = reader.ReadLong("l");
            c = reader.ReadCharArray("c");
            p = reader.ReadPortable<NamedPortable>("p");
            IObjectDataInput input = reader.GetRawDataInput();
            k = input.ReadInt();
            s = input.ReadUTF();
            sds = input.ReadObject<SimpleDataSerializable>();
        }

        public int GetFactoryId()
        {
            return PortableTest.FACTORY_ID;
        }


        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            var that = (RawDataPortable) o;

            if (k != that.k) return false;
            if (l != that.l) return false;
            if (!c.SequenceEqual(that.c)) return false;
            if (p != null ? !p.Equals(that.p) : that.p != null) return false;
            if (s != null ? !s.Equals(that.s) : that.s != null) return false;
            if (sds != null ? !sds.Equals(that.sds) : that.sds != null) return false;

            return true;
        }


        public override int GetHashCode()
        {
            var result = (int) (l ^ (((uint) l) >> 32));
            result = 31*result + (c != null ? c.GetHashCode() : 0);
            result = 31*result + (p != null ? p.GetHashCode() : 0);
            result = 31*result + k;
            result = 31*result + (s != null ? s.GetHashCode() : 0);
            result = 31*result + (sds != null ? sds.GetHashCode() : 0);
            return result;
        }
    }

    public class InvalidRawDataPortable : RawDataPortable
    {
        public const int CLASS_ID = 5;

        public InvalidRawDataPortable()
        {
        }

        public InvalidRawDataPortable(long l, char[] c, NamedPortable p, int k, String s, SimpleDataSerializable sds)
            : base(l, c, p, k, s, sds)
        {
        }

        public int GetClassId()
        {
            return CLASS_ID;
        }

        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("l", l);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteInt(k);
            output.WriteUTF(s);
            writer.WriteCharArray("c", c);
            output.WriteObject(sds);
            writer.WritePortable("p", p);
        }
    }

    public class InvalidRawDataPortable2 : RawDataPortable
    {
        public const int CLASS_ID = 6;

        public InvalidRawDataPortable2()
        {
        }

        public InvalidRawDataPortable2(long l, char[] c, NamedPortable p, int k, String s, SimpleDataSerializable sds)
            : base(l, c, p, k, s, sds)
        {
        }

        public int GetClassId()
        {
            return CLASS_ID;
        }

        public override void ReadPortable(IPortableReader reader)
        {
            c = reader.ReadCharArray("c");
            IObjectDataInput input = reader.GetRawDataInput();
            k = input.ReadInt();
            l = reader.ReadLong("l");
            s = input.ReadUTF();
            p = reader.ReadPortable<NamedPortable>("p");
            sds = input.ReadObject<SimpleDataSerializable>();
        }
    }

    public class SimpleDataSerializable : IDataSerializable
    {
        private byte[] data;

        public SimpleDataSerializable()
        {
        }

        public SimpleDataSerializable(byte[] data)
        {
            this.data = data;
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteInt(data.Length);
            output.Write(data);
        }

        public void ReadData(IObjectDataInput input)
        {
            int len = input.ReadInt();
            data = new byte[len];
            input.ReadFully(data);
        }

        public string GetJavaClassName()
        {
            throw new NotImplementedException();
        }


        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            var that = (SimpleDataSerializable) o;

            if (!data.SequenceEqual(that.data)) return false;

            return true;
        }


        public override int GetHashCode()
        {
            return data != null ? data.GetHashCode() : 0;
        }


        public override String ToString()
        {
            var sb = new StringBuilder("SimpleDataSerializable{");
            sb.Append("data=").Append(data);
            sb.Append('}');
            return sb.ToString();
        }
    }

    public class ComplexDataSerializable : IDataSerializable
    {
        private SimpleDataSerializable ds;
        private SimpleDataSerializable ds2;
        private NamedPortable portable;

        public ComplexDataSerializable()
        {
        }

        public ComplexDataSerializable(NamedPortable portable, SimpleDataSerializable ds, SimpleDataSerializable ds2)
        {
            this.portable = portable;
            this.ds = ds;
            this.ds2 = ds2;
        }


        public void WriteData(IObjectDataOutput output)
        {
            ds.WriteData(output);
            output.WriteObject(portable);
            ds2.WriteData(output);
        }


        public void ReadData(IObjectDataInput input)
        {
            ds = new SimpleDataSerializable();
            ds.ReadData(input);
            portable = input.ReadObject<NamedPortable>();
            ds2 = new SimpleDataSerializable();
            ds2.ReadData(input);
        }

        public string GetJavaClassName()
        {
            throw new NotImplementedException();
        }


        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            var that = (ComplexDataSerializable) o;

            if (ds != null ? !ds.Equals(that.ds) : that.ds != null) return false;
            if (ds2 != null ? !ds2.Equals(that.ds2) : that.ds2 != null) return false;
            if (portable != null ? !portable.Equals(that.portable) : that.portable != null) return false;

            return true;
        }


        public override int GetHashCode()
        {
            int result = ds != null ? ds.GetHashCode() : 0;
            result = 31*result + (portable != null ? portable.GetHashCode() : 0);
            result = 31*result + (ds2 != null ? ds2.GetHashCode() : 0);
            return result;
        }


        public override String ToString()
        {
            var sb = new StringBuilder("ComplexDataSerializable{");
            sb.Append("ds=").Append(ds);
            sb.Append(", portable=").Append(portable);
            sb.Append(", ds2=").Append(ds2);
            sb.Append('}');
            return sb.ToString();
        }
    }
}