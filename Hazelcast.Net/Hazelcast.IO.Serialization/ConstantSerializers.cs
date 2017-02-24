// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.IO.Serialization
{
    internal sealed class ConstantSerializers
    {
        internal sealed class NullSerializer : SingletonSerializer<object>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeNull;
            }

            public override object Read(IObjectDataInput input)
            {
                return null;
            }

            public override void Write(IObjectDataOutput output, object obj)
            {
            }
        }

        internal sealed class BooleanSerializer : SingletonSerializer<bool>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeBoolean;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override bool Read(IObjectDataInput input)
            {
                return input.ReadByte() != 0;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, bool obj)
            {
                output.Write((obj ? 1 : 0));
            }
        }

        internal sealed class BooleanArraySerializer : SingletonSerializer<bool[]>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeBooleanArray;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override bool[] Read(IObjectDataInput input)
            {
                return input.ReadBooleanArray();
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, bool[] obj)
            {
                output.WriteBooleanArray(obj);
            }
        }

        internal sealed class ByteSerializer : SingletonSerializer<byte>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeByte;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override byte Read(IObjectDataInput input)
            {
                return input.ReadByte();
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, byte obj)
            {
                output.WriteByte(obj);
            }
        }

        internal sealed class CharArraySerializer : SingletonSerializer<char[]>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeCharArray;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override char[] Read(IObjectDataInput input)
            {
                return input.ReadCharArray();
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, char[] obj)
            {
                output.WriteCharArray(obj);
            }
        }

        internal sealed class CharSerializer : SingletonSerializer<char>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeChar;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override char Read(IObjectDataInput input)
            {
                return input.ReadChar();
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, char obj)
            {
                output.WriteChar(obj);
            }
        }

        internal sealed class DoubleArraySerializer : SingletonSerializer<double[]>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeDoubleArray;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override double[] Read(IObjectDataInput input)
            {
                return input.ReadDoubleArray();
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, double[] obj)
            {
                output.WriteDoubleArray(obj);
            }
        }

        internal sealed class DoubleSerializer : SingletonSerializer<double>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeDouble;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override double Read(IObjectDataInput input)
            {
                return input.ReadDouble();
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, double obj)
            {
                output.WriteDouble(obj);
            }
        }

        internal sealed class FloatArraySerializer : SingletonSerializer<float[]>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeFloatArray;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override float[] Read(IObjectDataInput input)
            {
                return input.ReadFloatArray();
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, float[] obj)
            {
                output.WriteFloatArray(obj);
            }
        }

        internal sealed class FloatSerializer : SingletonSerializer<float>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeFloat;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override float Read(IObjectDataInput input)
            {
                return input.ReadFloat();
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, float obj)
            {
                output.WriteFloat(obj);
            }
        }

        internal sealed class IntegerArraySerializer : SingletonSerializer<int[]>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeIntegerArray;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override int[] Read(IObjectDataInput input)
            {
                return input.ReadIntArray();
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, int[] obj)
            {
                output.WriteIntArray(obj);
            }
        }

        internal sealed class IntegerSerializer : SingletonSerializer<int>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeInteger;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override int Read(IObjectDataInput input)
            {
                return input.ReadInt();
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, int obj)
            {
                output.WriteInt(obj);
            }
        }

        internal sealed class LongArraySerializer : SingletonSerializer<long[]>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeLongArray;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override long[] Read(IObjectDataInput input)
            {
                return input.ReadLongArray();
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, long[] obj)
            {
                output.WriteLongArray(obj);
            }
        }

        internal sealed class LongSerializer : SingletonSerializer<long>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeLong;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override long Read(IObjectDataInput input)
            {
                return input.ReadLong();
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, long obj)
            {
                output.WriteLong(obj);
            }
        }

        internal sealed class ShortArraySerializer : SingletonSerializer<short[]>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeShortArray;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override short[] Read(IObjectDataInput input)
            {
                return input.ReadShortArray();
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, short[] obj)
            {
                output.WriteShortArray(obj);
            }
        }

        internal sealed class ShortSerializer : SingletonSerializer<short>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeShort;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override short Read(IObjectDataInput input)
            {
                return input.ReadShort();
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, short obj)
            {
                output.WriteShort(obj);
            }
        }

        internal sealed class StringSerializer : SingletonSerializer<string>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeString;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override string Read(IObjectDataInput input)
            {
                return input.ReadUTF();
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, string obj)
            {
                output.WriteUTF(obj);
            }
        }

        internal sealed class StringArraySerializer : SingletonSerializer<string[]>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.ConstantTypeStringArray;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override string[] Read(IObjectDataInput input)
            {
                return input.ReadUTFArray();
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, string[] obj)
            {
                output.WriteUTFArray(obj);
            }
        }

        internal sealed class ByteArraySerializer : IByteArraySerializer<byte[]>
        {
            public int GetTypeId()
            {
                return SerializationConstants.ConstantTypeByteArray;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public byte[] Write(byte[] @object)
            {
                return @object;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public byte[] Read(byte[] buffer)
            {
                return buffer;
            }

            public void Destroy()
            {
            }
        }

        internal abstract class SingletonSerializer<T> : IStreamSerializer<T>
        {
            public virtual void Destroy()
            {
            }

            public abstract int GetTypeId();

            public abstract T Read(IObjectDataInput input);

            public abstract void Write(IObjectDataOutput output, T obj);
        }
    }
}