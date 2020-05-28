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

using System.Collections.Generic;
using Hazelcast.Core;

namespace Hazelcast.Serialization
{
    internal class DefaultPortableWriter : IPortableWriter
    {
        private readonly int _begin;
        private readonly IClassDefinition _cd;
        private readonly int _offset;
        private readonly IBufferObjectDataOutput _out;
        private readonly PortableSerializer _serializer;
        private readonly ISet<string> _writtenFields;
        private bool _raw;

        /// <exception cref="System.IO.IOException" />
        public DefaultPortableWriter(PortableSerializer serializer, IBufferObjectDataOutput @out, IClassDefinition cd)
        {
            _serializer = serializer;
            _out = @out;
            _cd = cd;
            _writtenFields = new HashSet<string>(); //cd.GetFieldCount()
            _begin = @out.Position();
            // room for final offset
            @out.WriteZeroBytes(4);
            @out.WriteInt(cd.GetFieldCount());
            _offset = @out.Position();
            // one additional for raw data
            var fieldIndexesLength = (cd.GetFieldCount() + 1)* BytesExtensions.SizeOfInt;
            @out.WriteZeroBytes(fieldIndexesLength);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteInt(string fieldName, int value)
        {
            SetPosition(fieldName, FieldType.Int);
            _out.WriteInt(value);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteLong(string fieldName, long value)
        {
            SetPosition(fieldName, FieldType.Long);
            _out.WriteLong(value);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteUTF(string fieldName, string str)
        {
            SetPosition(fieldName, FieldType.Utf);
            _out.WriteUtf(str);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteBoolean(string fieldName, bool value)
        {
            SetPosition(fieldName, FieldType.Boolean);
            _out.WriteBoolean(value);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteByte(string fieldName, byte value)
        {
            SetPosition(fieldName, FieldType.Byte);
            _out.WriteByte(value);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteChar(string fieldName, int value)
        {
            SetPosition(fieldName, FieldType.Char);
            _out.WriteChar(value);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteDouble(string fieldName, double value)
        {
            SetPosition(fieldName, FieldType.Double);
            _out.WriteDouble(value);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteFloat(string fieldName, float value)
        {
            SetPosition(fieldName, FieldType.Float);
            _out.WriteFloat(value);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteShort(string fieldName, short value)
        {
            SetPosition(fieldName, FieldType.Short);
            _out.WriteShort(value);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WritePortable(string fieldName, IPortable portable)
        {
            var fd = SetPosition(fieldName, FieldType.Portable);
            var isNull = portable == null;
            _out.WriteBoolean(isNull);
            _out.WriteInt(fd.GetFactoryId());
            _out.WriteInt(fd.GetClassId());
            if (!isNull)
            {
                CheckPortableAttributes(fd, portable);
                _serializer.WriteInternal(_out, portable);
            }
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteNullPortable(string fieldName, int factoryId, int classId)
        {
            SetPosition(fieldName, FieldType.Portable);
            _out.WriteBoolean(true);
            _out.WriteInt(factoryId);
            _out.WriteInt(classId);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteBooleanArray(string fieldName, bool[] values)
        {
            SetPosition(fieldName, FieldType.BooleanArray);
            _out.WriteBooleanArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteByteArray(string fieldName, byte[] values)
        {
            SetPosition(fieldName, FieldType.ByteArray);
            _out.WriteByteArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteCharArray(string fieldName, char[] values)
        {
            SetPosition(fieldName, FieldType.CharArray);
            _out.WriteCharArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteIntArray(string fieldName, int[] values)
        {
            SetPosition(fieldName, FieldType.IntArray);
            _out.WriteIntArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteLongArray(string fieldName, long[] values)
        {
            SetPosition(fieldName, FieldType.LongArray);
            _out.WriteLongArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteDoubleArray(string fieldName, double[] values)
        {
            SetPosition(fieldName, FieldType.DoubleArray);
            _out.WriteDoubleArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteFloatArray(string fieldName, float[] values)
        {
            SetPosition(fieldName, FieldType.FloatArray);
            _out.WriteFloatArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteShortArray(string fieldName, short[] values)
        {
            SetPosition(fieldName, FieldType.ShortArray);
            _out.WriteShortArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteUTFArray(string fieldName, string[] values)
        {
            SetPosition(fieldName, FieldType.UtfArray);
            _out.WriteUtfArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WritePortableArray(string fieldName, IPortable[] portables)
        {
            var fd = SetPosition(fieldName, FieldType.PortableArray);
            var len = portables == null ? ArraySerializer.NullArrayLength : portables.Length;
            _out.WriteInt(len);
            _out.WriteInt(fd.GetFactoryId());
            _out.WriteInt(fd.GetClassId());
            if (len > 0)
            {
                var offset = _out.Position();
                _out.WriteZeroBytes(len*4);
                for (var i = 0; i < portables.Length; i++)
                {
                    var portable = portables[i];
                    CheckPortableAttributes(fd, portable);
                    var position = _out.Position();
                    _out.WriteInt(offset + i* BytesExtensions.SizeOfInt, position);
                    _serializer.WriteInternal(_out, portable);
                }
            }
        }

        /// <exception cref="System.IO.IOException" />
        public virtual IObjectDataOutput GetRawDataOutput()
        {
            if (!_raw)
            {
                var pos = _out.Position();
                // last index
                var index = _cd.GetFieldCount();
                _out.WriteInt(_offset + index* BytesExtensions.SizeOfInt, pos);
            }
            _raw = true;
            return _out;
        }

        public virtual int GetVersion()
        {
            return _cd.GetVersion();
        }

        /// <exception cref="System.IO.IOException" />
        internal virtual void End()
        {
            // write final offset
            var position = _out.Position();
            _out.WriteInt(_begin, position);
        }

        private void CheckPortableAttributes(IFieldDefinition fd, IPortable portable)
        {
            if (fd.GetFactoryId() != portable.GetFactoryId())
            {
                throw new HazelcastSerializationException(
                    "Wrong Portable type! Generic portable types are not supported! " + " Expected factory-id: " +
                    fd.GetFactoryId() + ", Actual factory-id: " + portable.GetFactoryId());
            }
            if (fd.GetClassId() != portable.GetClassId())
            {
                throw new HazelcastSerializationException(
                    "Wrong Portable type! Generic portable types are not supported! " + "Expected class-id: " +
                    fd.GetClassId() + ", Actual class-id: " + portable.GetClassId());
            }
        }

        /// <exception cref="System.IO.IOException" />
        private IFieldDefinition SetPosition(string fieldName, FieldType fieldType)
        {
            if (_raw)
            {
                throw new HazelcastSerializationException(
                    "Cannot write Portable fields after getRawDataOutput() is called!");
            }
            var fd = _cd.GetField(fieldName);
            if (fd == null)
            {
                throw new HazelcastSerializationException("Invalid field name: '" + fieldName +
                                                          "' for ClassDefinition {id: " + _cd.GetClassId() +
                                                          ", version: " + _cd.GetVersion() + "}");
            }
            if (_writtenFields.Add(fieldName))
            {
                var pos = _out.Position();
                var index = fd.GetIndex();
                _out.WriteInt(_offset + index* BytesExtensions.SizeOfInt, pos);
                _out.WriteShort(fieldName.Length);
                _out.WriteBytes(fieldName);
                _out.WriteByte((byte) fieldType);
            }
            else
            {
                throw new HazelcastSerializationException("Field '" + fieldName + "' has already been written!");
            }
            return fd;
        }
    }
}
