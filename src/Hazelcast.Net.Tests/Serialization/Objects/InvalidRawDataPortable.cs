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

using Hazelcast.Serialization;

namespace Hazelcast.Tests.Serialization.Objects
{
    internal class InvalidRawDataPortable : RawDataPortable
    {
        public InvalidRawDataPortable()
        { }

        public InvalidRawDataPortable(long l, char[] c, NamedPortable p, int k, string s, ByteArrayDataSerializable sds)
            :
                base(l, c, p, k, s, sds)
        {
        }

        public override int ClassId => SerializationTestsConstants.INVALID_RAW_DATA_PORTABLE;

        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("l", l);
            var output = writer.GetRawDataOutput();
            output.WriteInt(k);
            output.WriteUTF(s);
            writer.WriteCharArray("c", c);
            output.WriteObject(sds);
            writer.WritePortable("p", p);
        }
    }
}
