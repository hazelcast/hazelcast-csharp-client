// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.IO.Serialization
{
    /// <summary>
    /// Base interface of custom serialization interfaces
    /// <p/>
    /// see
    /// <see cref="IByteArraySerializer{T}">IByteArraySerializer&lt;T&gt;</see>
    /// see
    /// <see cref="IStreamSerializer{T}">IStreamSerializer&lt;T&gt;</see>
    /// </summary>
    public interface ISerializer
    {
        /// <summary>Called when instance is shutting down.</summary>
        /// <remarks>Called when instance is shutting down. It can be used to clear used resources.
        /// 	</remarks>
        void Destroy();

        /// <returns>typeId of serializer</returns>
        int GetTypeId();
    }
}