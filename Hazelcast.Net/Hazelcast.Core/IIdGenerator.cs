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

namespace Hazelcast.Core
{
    /// <summary>Cluster-wide unique id generator.</summary>
    /// <remarks>Cluster-wide unique id generator.</remarks>
    public interface IIdGenerator : IDistributedObject
    {
        /// <summary>Try to initialize this <c>IIdGenerator</c> instance with given id</summary>
        /// <returns>true if initialization success</returns>
        bool Init(long id);

        /// <summary>Generates and returns cluster-wide unique id.</summary>
        /// <remarks>
        ///     Generates and returns cluster-wide unique id.
        ///     Generated ids are guaranteed to be unique for the entire cluster
        ///     as long as the cluster is live. If the cluster restarts then
        ///     id generation will start from 0.
        /// </remarks>
        /// <returns>cluster-wide new unique id</returns>
        long NewId();
    }
}