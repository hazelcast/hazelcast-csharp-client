// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Map
{
    public interface IEntryProcessor<TKey, TValue>
    {
        /// <summary>Get the entry processor to be applied to backup entries.</summary>
        /// <remarks>
        ///     Get the entry processor to be applied to backup entries.
        ///     <p />
        /// </remarks>
        /// <returns>back up processor</returns>
        IEntryBackupProcessor<TKey, TValue> GetBackupProcessor();

        /// <summary>Process the entry without worrying about concurrency.</summary>
        /// <remarks>
        ///     Process the entry without worrying about concurrency.
        ///     <p />
        /// </remarks>
        /// <param name="entry">entry to be processes</param>
        /// <returns>result of the process</returns>
        object Process(KeyValuePair<TKey, TValue> entry);
    }
}