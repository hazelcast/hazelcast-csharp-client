/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

﻿using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{
    internal class SampleIdentifiedDataSerializable : IIdentifiedDataSerializable
    {
        public const int CLASS_ID = 1;
        private char c;
        private int i;

        public SampleIdentifiedDataSerializable(char c, int i)
        {
            this.c = c;
            this.i = i;
        }

        public SampleIdentifiedDataSerializable()
        {
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteInt(i);
            output.WriteChar(c);
        }

        public void ReadData(IObjectDataInput input)
        {
            i = input.ReadInt();
            c = input.ReadChar();
        }

        public string GetJavaClassName()
        {
            return typeof (SampleIdentifiedDataSerializable).FullName;
        }

        public int GetFactoryId()
        {
            return TestSerializationConstants.PORTABLE_FACTORY_ID;
        }

        public int GetId()
        {
            return CLASS_ID;
        }

        protected bool Equals(SampleIdentifiedDataSerializable other)
        {
            return c == other.c && i == other.i;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SampleIdentifiedDataSerializable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (c.GetHashCode()*397) ^ i;
            }
        }

        public override string ToString()
        {
            return string.Format("C: {0}, I: {1}", c, i);
        }
    }
}