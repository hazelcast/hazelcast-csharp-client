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

using Hazelcast.IO;

#pragma warning disable CS1591
namespace Hazelcast.Client.Spi
{
    /// <summary>
    /// Partition service for Hazelcast clients.
    /// </summary>
    /// <remarks>
    /// Allows to retrieve information about the partition count, partition owner or partition ID of a key.
    /// </remarks>
    public interface IClientPartitionService
    {
        int GetPartitionCount();
        int GetPartitionId(object key);
        Address GetPartitionOwner(int partitionId);
    }
}