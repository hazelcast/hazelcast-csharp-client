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

namespace Hazelcast.IO.Serialization
{
    [Serializable]
    internal class IncompatibleClassChangeError : SystemException
    {
    }

    internal class MorphingPortableReader : DefaultPortableReader
    {
        public MorphingPortableReader(PortableSerializer serializer, IBufferObjectDataInput input, IClassDefinition cd)
            : base(serializer, input, cd)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override bool ReadBoolean(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return false;
            }
            if (fd.GetFieldType() != FieldType.Boolean)
            {
                throw new IncompatibleClassChangeError();
            }
            return base.ReadBoolean(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override byte ReadByte(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return 0;
            }
            if (fd.GetFieldType() != FieldType.Byte)
            {
                throw new IncompatibleClassChangeError();
            }
            return base.ReadByte(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override byte[] ReadByteArray(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return null;
            }
            if (fd.GetFieldType() != FieldType.ByteArray)
            {
                throw new IncompatibleClassChangeError();
            }
            return base.ReadByteArray(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override char ReadChar(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return '\0';
            }
            if (fd.GetFieldType() != FieldType.Char)
            {
                throw new IncompatibleClassChangeError();
            }
            return base.ReadChar(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override char[] ReadCharArray(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return null;
            }
            if (fd.GetFieldType() != FieldType.CharArray)
            {
                throw new IncompatibleClassChangeError();
            }
            return base.ReadCharArray(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override double ReadDouble(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return 0d;
            }
            switch (fd.GetFieldType())
            {
                case FieldType.Double:
                {
                    return base.ReadDouble(fieldName);
                }

                case FieldType.Long:
                {
                    return base.ReadLong(fieldName);
                }

                case FieldType.Float:
                {
                    return base.ReadFloat(fieldName);
                }

                case FieldType.Int:
                {
                    return base.ReadInt(fieldName);
                }

                case FieldType.Byte:
                {
                    return base.ReadByte(fieldName);
                }

                case FieldType.Char:
                {
                    return base.ReadChar(fieldName);
                }

                case FieldType.Short:
                {
                    return base.ReadShort(fieldName);
                }

                default:
                {
                    throw new IncompatibleClassChangeError();
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override double[] ReadDoubleArray(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return null;
            }
            if (fd.GetFieldType() != FieldType.DoubleArray)
            {
                throw new IncompatibleClassChangeError();
            }
            return base.ReadDoubleArray(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override float ReadFloat(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return 0f;
            }
            switch (fd.GetFieldType())
            {
                case FieldType.Float:
                {
                    return base.ReadFloat(fieldName);
                }

                case FieldType.Int:
                {
                    return base.ReadInt(fieldName);
                }

                case FieldType.Byte:
                {
                    return base.ReadByte(fieldName);
                }

                case FieldType.Char:
                {
                    return base.ReadChar(fieldName);
                }

                case FieldType.Short:
                {
                    return base.ReadShort(fieldName);
                }

                default:
                {
                    throw new IncompatibleClassChangeError();
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override float[] ReadFloatArray(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return null;
            }
            if (fd.GetFieldType() != FieldType.FloatArray)
            {
                throw new IncompatibleClassChangeError();
            }
            return base.ReadFloatArray(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override int ReadInt(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return 0;
            }
            switch (fd.GetFieldType())
            {
                case FieldType.Int:
                {
                    return base.ReadInt(fieldName);
                }

                case FieldType.Byte:
                {
                    return base.ReadByte(fieldName);
                }

                case FieldType.Char:
                {
                    return base.ReadChar(fieldName);
                }

                case FieldType.Short:
                {
                    return base.ReadShort(fieldName);
                }

                default:
                {
                    throw new IncompatibleClassChangeError();
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override int[] ReadIntArray(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return null;
            }
            if (fd.GetFieldType() != FieldType.IntArray)
            {
                throw new IncompatibleClassChangeError();
            }
            return base.ReadIntArray(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override long ReadLong(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return 0L;
            }
            switch (fd.GetFieldType())
            {
                case FieldType.Long:
                {
                    return base.ReadLong(fieldName);
                }

                case FieldType.Int:
                {
                    return base.ReadInt(fieldName);
                }

                case FieldType.Byte:
                {
                    return base.ReadByte(fieldName);
                }

                case FieldType.Char:
                {
                    return base.ReadChar(fieldName);
                }

                case FieldType.Short:
                {
                    return base.ReadShort(fieldName);
                }

                default:
                {
                    throw new IncompatibleClassChangeError();
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override long[] ReadLongArray(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return null;
            }
            if (fd.GetFieldType() != FieldType.LongArray)
            {
                throw new IncompatibleClassChangeError();
            }
            return base.ReadLongArray(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override TPortable ReadPortable<TPortable>(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return default(TPortable);
            }
            if (fd.GetFieldType() != FieldType.Portable)
            {
                throw new IncompatibleClassChangeError();
            }
            return base.ReadPortable<TPortable>(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override IPortable[] ReadPortableArray(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return null;
            }
            if (fd.GetFieldType() != FieldType.PortableArray)
            {
                throw new IncompatibleClassChangeError();
            }
            return base.ReadPortableArray(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override short ReadShort(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return 0;
            }
            switch (fd.GetFieldType())
            {
                case FieldType.Short:
                {
                    return base.ReadShort(fieldName);
                }

                case FieldType.Byte:
                {
                    return base.ReadByte(fieldName);
                }

                default:
                {
                    throw new IncompatibleClassChangeError();
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override short[] ReadShortArray(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return null;
            }
            if (fd.GetFieldType() != FieldType.ShortArray)
            {
                throw new IncompatibleClassChangeError();
            }
            return base.ReadShortArray(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override string ReadUTF(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return null;
            }
            if (fd.GetFieldType() != FieldType.Utf)
            {
                throw new IncompatibleClassChangeError();
            }
            return base.ReadUTF(fieldName);
        }
    }
}