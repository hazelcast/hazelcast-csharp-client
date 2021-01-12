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

using System;
using Hazelcast.Core;

namespace Hazelcast.Serialization.ConstantSerializers
{
    internal class GuidSerializer : SingletonSerializerBase<Guid>
    {
        public override int TypeId => SerializationConstants.ConstantTypeUuid;

        /// <exception cref="System.IO.IOException"></exception>
        public override Guid Read(IObjectDataInput input)
        {
            var order = default(JavaUuidOrder);

            order.X0 = input.ReadByte();
            order.X1 = input.ReadByte();
            order.X2 = input.ReadByte();
            order.X3 = input.ReadByte();

            order.X4 = input.ReadByte();
            order.X5 = input.ReadByte();
            order.X6 = input.ReadByte();
            order.X7 = input.ReadByte();

            order.X8 = input.ReadByte();
            order.X9 = input.ReadByte();
            order.XA = input.ReadByte();
            order.XB = input.ReadByte();

            order.XC = input.ReadByte();
            order.XD = input.ReadByte();
            order.XE = input.ReadByte();
            order.XF = input.ReadByte();

            return order.Value;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IObjectDataOutput output, Guid obj)
        {
            var order = default(JavaUuidOrder);
            order.Value = obj;
            output.WriteByte(order.X0);
            output.WriteByte(order.X1);
            output.WriteByte(order.X2);
            output.WriteByte(order.X3);

            output.WriteByte(order.X4);
            output.WriteByte(order.X5);
            output.WriteByte(order.X6);
            output.WriteByte(order.X7);

            output.WriteByte(order.X8);
            output.WriteByte(order.X9);
            output.WriteByte(order.XA);
            output.WriteByte(order.XB);

            output.WriteByte(order.XC);
            output.WriteByte(order.XD);
            output.WriteByte(order.XE);
            output.WriteByte(order.XF);
        }
    }
}
