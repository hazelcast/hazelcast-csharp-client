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

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Defines a concurrent, distributed, non-partitioned and listenable set
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Hazelcast <c>IHSet</c> is not a partitioned data-structure. Entire contents
    /// of an <c>IHSet</c> is stored on a single machine (and in the backup). The <c>IHSet</c>
    /// will not scale by adding more members to the cluster.
    /// </para>
    /// </remarks>
    public interface IHSet<T> : IHCollection<T>
    { }
}
