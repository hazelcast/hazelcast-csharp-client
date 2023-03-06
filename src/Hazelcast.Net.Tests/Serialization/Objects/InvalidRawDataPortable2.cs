// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
    internal class InvalidRawDataPortable2 : RawDataPortable
    {
        public InvalidRawDataPortable2()
        { }

        public InvalidRawDataPortable2(long l, char[] c, NamedPortable p, int k, string s, ByteArrayDataSerializable sds)
            :
                base(l, c, p, k, s, sds)
        {
        }

        public override int ClassId => SerializationTestsConstants.INVALID_RAW_DATA_PORTABLE_2;

        public override void ReadPortable(IPortableReader reader)
        {
            c = reader.ReadCharArray("c");
            var input = reader.GetRawDataInput();
            k = input.ReadInt();
            l = reader.ReadLong("l");
            s = input.ReadString();
            p = reader.ReadPortable<NamedPortable>("p");
            sds = input.ReadObject<ByteArrayDataSerializable>();
        }
    }
}
