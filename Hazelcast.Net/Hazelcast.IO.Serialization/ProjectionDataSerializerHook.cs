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
using Hazelcast.Core;

namespace Hazelcast.IO.Serialization
{
    internal class ProjectionDataSerializerHook : IDataSerializerHook
    {
        public const int FactoryId = FactoryIds.ProjectionDsFactoryId;
        public const int SingleAttribute = 0;
        public const int MultiAttribute = 1;

        private const int Len = MultiAttribute + 1;

        public IDataSerializableFactory CreateFactory()
        {
            var constructors = new Func<IIdentifiedDataSerializable>[Len];
            constructors[SingleAttribute] = () => new SingleAttributeProjection();

            return new ArrayDataSerializableFactory(constructors);
        }

        public int GetFactoryId()
        {
            return FactoryId;
        }
    }
}