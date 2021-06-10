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

namespace Hazelcast.Serialization
{
    internal class MorphingPortableReader : DefaultPortableReader
    {
        public MorphingPortableReader(PortableSerializer serializer, ObjectDataInput input, IClassDefinition cd)
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
            if (fd.FieldType != FieldType.Boolean)
            {
                throw new InvalidPortableFieldException();
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
            if (fd.FieldType != FieldType.Byte)
            {
                throw new InvalidPortableFieldException();
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
            if (fd.FieldType != FieldType.ByteArray)
            {
                throw new InvalidPortableFieldException();
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
            if (fd.FieldType != FieldType.Char)
            {
                throw new InvalidPortableFieldException();
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
            if (fd.FieldType != FieldType.CharArray)
            {
                throw new InvalidPortableFieldException();
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
            switch (fd.FieldType)
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
                    throw new InvalidPortableFieldException();
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
            if (fd.FieldType != FieldType.DoubleArray)
            {
                throw new InvalidPortableFieldException();
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
            switch (fd.FieldType)
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
                    throw new InvalidPortableFieldException();
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
            if (fd.FieldType != FieldType.FloatArray)
            {
                throw new InvalidPortableFieldException();
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
            switch (fd.FieldType)
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
                    throw new InvalidPortableFieldException();
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
            if (fd.FieldType != FieldType.IntArray)
            {
                throw new InvalidPortableFieldException();
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
            switch (fd.FieldType)
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
                    throw new InvalidPortableFieldException();
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
            if (fd.FieldType != FieldType.LongArray)
            {
                throw new InvalidPortableFieldException();
            }
            return base.ReadLongArray(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override TPortable ReadPortable<TPortable>(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return default;
            }
            if (fd.FieldType != FieldType.Portable)
            {
                throw new InvalidPortableFieldException();
            }
            return base.ReadPortable<TPortable>(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override TPortable[] ReadPortableArray<TPortable>(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return null;
            }
            if (fd.FieldType != FieldType.PortableArray)
            {
                throw new InvalidPortableFieldException();
            }
            return base.ReadPortableArray<TPortable>(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override short ReadShort(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return 0;
            }
            switch (fd.FieldType)
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
                    throw new InvalidPortableFieldException();
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
            if (fd.FieldType != FieldType.ShortArray)
            {
                throw new InvalidPortableFieldException();
            }
            return base.ReadShortArray(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override string ReadString(string fieldName)
        {
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return null;
            }
            if (fd.FieldType != FieldType.Utf)
            {
                throw new InvalidPortableFieldException();
            }
            return base.ReadString(fieldName);
        }
    }
}
