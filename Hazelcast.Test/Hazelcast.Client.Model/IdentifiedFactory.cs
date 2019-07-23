// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Client.Model;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test
{
    internal class IdentifiedFactory : IDataSerializableFactory
    {
        internal const int FactoryId = 66;

        public IIdentifiedDataSerializable Create(int typeId)
        {
            if (typeId == IdentifiedEntryProcessor.ClassId)
            {
                return new IdentifiedEntryProcessor();
            }
            if (typeId == CustomComparator.ClassId)
            {
                return new CustomComparator();
            }
            return null;
        }
    }
}