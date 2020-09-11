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

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a distributed dictionary with weak consistency and values locally stored on every node of the cluster.
    /// </summary>
    /// <remarks>
    /// <p>Whenever a value is written asynchronously, the new value will be internally
    /// distributed to all existing cluster members, and eventually every node will have
    /// the new value.</p>
    /// <p>When a new node joins the cluster, the new node initially will request existing
    ///  values from older nodes and replicate them locally.</p>
    /// </remarks>
    /// <typeparam name="TKey">the type of keys maintained by this map</typeparam>
    /// <typeparam name="TValue">the type of mapped values</typeparam>
    public partial interface IHReplicatedDictionary<TKey, TValue> : IDistributedObject
    { }
}
