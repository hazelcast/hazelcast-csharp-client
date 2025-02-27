// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Serialization;

namespace Hazelcast.Tests.Serialization.Objects
{
    internal class MorphingPortableBase : IPortable
    {
        internal bool aBoolean;
        internal byte aByte;
        internal double aDouble;
        internal float aFloat;
        internal long aLong;
        internal short aShort;
        internal string aString;
        internal char character;
        internal int integer;

        public MorphingPortableBase(byte aByte, bool aBoolean, char character, short aShort, int integer, long aLong,
            float aFloat, double aDouble, string aString)
        {
            this.aByte = aByte;
            this.aBoolean = aBoolean;
            this.character = character;
            this.aShort = aShort;
            this.integer = integer;
            this.aLong = aLong;
            this.aFloat = aFloat;
            this.aDouble = aDouble;
            this.aString = aString;
        }

        public MorphingPortableBase()
        { }

        public virtual int FactoryId => SerializationTestsConstants.PORTABLE_FACTORY_ID;

        public virtual int ClassId => SerializationTestsConstants.MORPHING_PORTABLE_ID;

        /// <exception cref="System.IO.IOException"/>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteByte("byte", aByte);
            writer.WriteBoolean("boolean", aBoolean);
            writer.WriteChar("char", character);
            writer.WriteShort("short", aShort);
            writer.WriteInt("int", integer);
            writer.WriteLong("long", aLong);
            writer.WriteFloat("float", aFloat);
            writer.WriteDouble("double", aDouble);
            writer.WriteString("string", aString);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual void ReadPortable(IPortableReader reader)
        {
            aByte = reader.ReadByte("byte");
            aBoolean = reader.ReadBoolean("boolean");
            character = reader.ReadChar("char");
            aShort = reader.ReadShort("short");
            integer = reader.ReadInt("int");
            aLong = reader.ReadLong("long");
            aFloat = reader.ReadFloat("float");
            aDouble = reader.ReadDouble("double");
            aString = reader.ReadString("string");
        }
    }
}
