﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Serialization.DefaultSerializers
{
    internal class ListSerializer<T> : SingletonSerializerBase<List<T>>
    {
        public override int TypeId => SerializationConstants.JavaDefaultTypeArrayList;

        public override List<T> Read(IObjectDataInput input)
        {
            var size = input.ReadInt();
            if (size <= ArraySerializer.NullArrayLength) return null;

            var list = new List<T>(size);
            for (var i = 0; i < size; i++)
            {
                list.Add(input.ReadObject<T>());
            }
            return list;
        }

        public override void Write(IObjectDataOutput output, List<T> obj)
        {
            var size = obj == null ? ArraySerializer.NullArrayLength : obj.Count;
            output.Write(size);
            foreach (var o in obj)
            {
                output.WriteObject(o);
            }
        }
    }
}
