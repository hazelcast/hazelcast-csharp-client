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
using System.IO;
using Hazelcast.Core;

namespace Hazelcast.Serialization
{
    internal class DefaultPortableReader : IPortableReader
    {
        private const char NestedFieldPattern = '.';

        private readonly int _finalPosition;
        private readonly IBufferObjectDataInput _in;
        private readonly int _offset;

        protected readonly IClassDefinition Cd;
        private readonly PortableSerializer Serializer;
        private bool _raw;

        public DefaultPortableReader(PortableSerializer serializer, IBufferObjectDataInput @in, IClassDefinition cd)
        {
            _in = @in;
            Serializer = serializer;
            Cd = cd;
            int fieldCount;
            try
            {
                // final position after portable is read
                _finalPosition = @in.ReadInt();
                // field count
                fieldCount = @in.ReadInt();
            }
            catch (IOException e)
            {
                throw new SerializationException(e);
            }
            if (fieldCount != cd.GetFieldCount())
            {
                throw new InvalidOperationException("Field count[" + fieldCount + "] in stream does not match " + cd);
            }
            _offset = @in.Position();
        }

        public virtual int GetVersion()
        {
            return Cd.GetVersion();
        }

        public virtual bool HasField(string fieldName)
        {
            return Cd.HasField(fieldName);
        }

        public virtual ICollection<string> GetFieldNames()
        {
            return Cd.GetFieldNames();
        }

        public virtual FieldType GetFieldType(string fieldName)
        {
            return Cd.GetFieldType(fieldName);
        }

        public virtual int GetFieldClassId(string fieldName)
        {
            return Cd.GetFieldClassId(fieldName);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual int ReadInt(string fieldName)
        {
            var pos = ReadPosition(fieldName, FieldType.Int);
            return _in.ReadInt(pos);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual long ReadLong(string fieldName)
        {
            var pos = ReadPosition(fieldName, FieldType.Long);
            return _in.ReadLong(pos);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual string ReadUTF(string fieldName)
        {
            var currentPos = _in.Position();
            try
            {
                var pos = ReadPosition(fieldName, FieldType.Utf);
                _in.Position(pos);
                return _in.ReadUtf();
            }
            finally
            {
                _in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual bool ReadBoolean(string fieldName)
        {
            var pos = ReadPosition(fieldName, FieldType.Boolean);
            return _in.ReadBoolean(pos);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual byte ReadByte(string fieldName)
        {
            var pos = ReadPosition(fieldName, FieldType.Byte);
            return _in.ReadByte(pos);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual char ReadChar(string fieldName)
        {
            var pos = ReadPosition(fieldName, FieldType.Char);
            return _in.ReadChar(pos);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual double ReadDouble(string fieldName)
        {
            var pos = ReadPosition(fieldName, FieldType.Double);
            return _in.ReadDouble(pos);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual float ReadFloat(string fieldName)
        {
            var pos = ReadPosition(fieldName, FieldType.Float);
            return _in.ReadFloat(pos);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual short ReadShort(string fieldName)
        {
            var pos = ReadPosition(fieldName, FieldType.Short);
            return _in.ReadShort(pos);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual bool[] ReadBooleanArray(string fieldName)
        {
            var currentPos = _in.Position();
            try
            {
                var pos = ReadPosition(fieldName, FieldType.BooleanArray);
                _in.Position(pos);
                return _in.ReadBooleanArray();
            }
            finally
            {
                _in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual byte[] ReadByteArray(string fieldName)
        {
            var currentPos = _in.Position();
            try
            {
                var pos = ReadPosition(fieldName, FieldType.ByteArray);
                _in.Position(pos);
                return _in.ReadByteArray();
            }
            finally
            {
                _in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual char[] ReadCharArray(string fieldName)
        {
            var currentPos = _in.Position();
            try
            {
                var pos = ReadPosition(fieldName, FieldType.CharArray);
                _in.Position(pos);
                return _in.ReadCharArray();
            }
            finally
            {
                _in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual int[] ReadIntArray(string fieldName)
        {
            var currentPos = _in.Position();
            try
            {
                var pos = ReadPosition(fieldName, FieldType.IntArray);
                _in.Position(pos);
                return _in.ReadIntArray();
            }
            finally
            {
                _in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual long[] ReadLongArray(string fieldName)
        {
            var currentPos = _in.Position();
            try
            {
                var pos = ReadPosition(fieldName, FieldType.LongArray);
                _in.Position(pos);
                return _in.ReadLongArray();
            }
            finally
            {
                _in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual double[] ReadDoubleArray(string fieldName)
        {
            var currentPos = _in.Position();
            try
            {
                var pos = ReadPosition(fieldName, FieldType.DoubleArray);
                _in.Position(pos);
                return _in.ReadDoubleArray();
            }
            finally
            {
                _in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual float[] ReadFloatArray(string fieldName)
        {
            var currentPos = _in.Position();
            try
            {
                var pos = ReadPosition(fieldName, FieldType.FloatArray);
                _in.Position(pos);
                return _in.ReadFloatArray();
            }
            finally
            {
                _in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual short[] ReadShortArray(string fieldName)
        {
            var currentPos = _in.Position();
            try
            {
                var pos = ReadPosition(fieldName, FieldType.ShortArray);
                _in.Position(pos);
                return _in.ReadShortArray();
            }
            finally
            {
                _in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual string[] ReadUTFArray(string fieldName)
        {
            var currentPos = _in.Position();
            try
            {
                var pos = ReadPosition(fieldName, FieldType.UtfArray);
                _in.Position(pos);
                return _in.ReadUtfArray();
            }
            finally
            {
                _in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual TPortable ReadPortable<TPortable>(string fieldName) where TPortable : IPortable
        {
            var currentPos = _in.Position();
            try
            {
                var fd = Cd.GetField(fieldName);
                if (fd == null)
                {
                    throw ThrowUnknownFieldException(fieldName);
                }
                if (fd.GetFieldType() != FieldType.Portable)
                {
                    throw new SerializationException("Not a Portable field: " + fieldName);
                }
                var pos = ReadPosition(fd);
                _in.Position(pos);
                var isNull = _in.ReadBoolean();
                var factoryId = _in.ReadInt();
                var classId = _in.ReadInt();
                CheckFactoryAndClass(fd, factoryId, classId);
                if (!isNull)
                {
                    return (TPortable) Serializer.Read(_in, factoryId, classId);
                }
                return default;
            }
            finally
            {
                _in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual IPortable[] ReadPortableArray(string fieldName)
        {
            var currentPos = _in.Position();
            try
            {
                var fd = Cd.GetField(fieldName);
                if (fd == null)
                {
                    throw ThrowUnknownFieldException(fieldName);
                }
                if (fd.GetFieldType() != FieldType.PortableArray)
                {
                    throw new SerializationException("Not a Portable array field: " + fieldName);
                }
                var pos = ReadPosition(fd);
                _in.Position(pos);
                var len = _in.ReadInt();
                var factoryId = _in.ReadInt();
                var classId = _in.ReadInt();

                if (len == ArraySerializer.NullArrayLength) return null;

                CheckFactoryAndClass(fd, factoryId, classId);
                var portables = new IPortable[len];
                if (len > 0)
                {
                    var offset = _in.Position();
                    for (var i = 0; i < len; i++)
                    {
                        var start = _in.ReadInt(offset + i* BytesExtensions.SizeOfInt);
                        _in.Position(start);
                        portables[i] = Serializer.Read(_in, factoryId, classId);
                    }
                }
                return portables;
            }
            finally
            {
                _in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual IObjectDataInput GetRawDataInput()
        {
            if (!_raw)
            {
                var pos = _in.ReadInt(_offset + Cd.GetFieldCount()* BytesExtensions.SizeOfInt);
                _in.Position(pos);
            }
            _raw = true;
            return _in;
        }

        /// <exception cref="System.IO.IOException"/>
        internal void End()
        {
            _in.Position(_finalPosition);
        }

        private static void CheckFactoryAndClass(IFieldDefinition fd, int factoryId, int classId)
        {
            if (factoryId != fd.GetFactoryId())
            {
                throw new ArgumentException("Invalid factoryId! Expected: " + fd.GetFactoryId() + ", Current: " +
                                            factoryId);
            }
            if (classId != fd.GetClassId())
            {
                throw new ArgumentException("Invalid classId! Expected: " + fd.GetClassId() + ", Current: " + classId);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        private int ReadNestedPosition(string fieldName, FieldType type)
        {
            var fieldNames = fieldName.Split(NestedFieldPattern);
            if (fieldNames.Length > 1)
            {
                IFieldDefinition fd = null;
                var reader = this;
                for (var i = 0; i < fieldNames.Length; i++)
                {
                    fd = reader.Cd.GetField(fieldNames[i]);
                    if (fd == null)
                    {
                        break;
                    }
                    if (i == fieldNames.Length - 1)
                    {
                        break;
                    }
                    var pos = reader.ReadPosition(fd);
                    _in.Position(pos);
                    var isNull = _in.ReadBoolean();
                    if (isNull)
                    {
                        throw new ArgumentNullException("Parent field is null: " + fieldNames[i]);
                    }
                    reader = Serializer.CreateReader(_in);
                }
                if (fd == null)
                {
                    throw ThrowUnknownFieldException(fieldName);
                }
                if (fd.GetFieldType() != type)
                {
                    throw new SerializationException("Not a '" + type + "' field: " + fieldName);
                }
                return reader.ReadPosition(fd);
            }
            throw ThrowUnknownFieldException(fieldName);
        }

        /// <exception cref="System.IO.IOException"/>
        private int ReadPosition(string fieldName, FieldType type)
        {
            if (_raw)
            {
                throw new SerializationException(
                    "Cannot read Portable fields after getRawDataInput() is called!");
            }
            var fd = Cd.GetField(fieldName);
            if (fd == null)
            {
                return ReadNestedPosition(fieldName, type);
            }
            if (fd.GetFieldType() != type)
            {
                throw new SerializationException("Not a '" + type + "' field: " + fieldName);
            }
            return ReadPosition(fd);
        }

        /// <exception cref="System.IO.IOException"/>
        private int ReadPosition(IFieldDefinition fd)
        {
            var pos = _in.ReadInt(_offset + fd.GetIndex()* BytesExtensions.SizeOfInt);
            var len = _in.ReadShort(pos);
            // name + len + type
            return pos + BytesExtensions.SizeOfShort + len + 1;
        }

        private SerializationException ThrowUnknownFieldException(string fieldName)
        {
            return
                new SerializationException("Unknown field name: '" + fieldName + "' for ClassDefinition {id: " +
                                                    Cd.GetClassId() + ", version: " + Cd.GetVersion() + "}");
        }
    }
}
