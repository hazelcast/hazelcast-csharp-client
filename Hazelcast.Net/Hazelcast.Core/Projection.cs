// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Core
{
    /// <summary>
    /// Marker interface for server side projection implementation.
    /// Enables transforming object into other objects.
    /// Only 1:1 transformations allowed. Use an Aggregator to perform N:1 or N:M aggregations.
    /// <br/>
    /// Projection implementations must be hazelcast serializable and must have a counterpart/implementation on server side.
    /// </summary>
    public class IProjection
    {
    }

    /// <summary>
    /// Projection that extracts the values of the given attribute and returns it.<br/>
    /// The attributePath does not support the [any] operator.
    /// </summary>
    public sealed class SingleAttributeProjection : IProjection, IIdentifiedDataSerializable
    {
        private string attributePath;

        public SingleAttributeProjection()
        {
        }

        public SingleAttributeProjection(string attributePath)
        {
            ValidationUtil.HasText(attributePath, "attributePath must not be null or empty");
            if (attributePath.Contains("[any]"))
            {
                throw new ArgumentException("attributePath must not contain [any] operators");
            }
            this.attributePath = attributePath;
        }

        public void ReadData(IObjectDataInput input)
        {
            attributePath = input.ReadUTF();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(attributePath);
        }

        public int GetFactoryId()
        {
            return FactoryIds.ProjectionDsFactoryId;
        }

        public int GetId()
        {
            return ProjectionDataSerializerHook.SingleAttribute;
        }
    }

}