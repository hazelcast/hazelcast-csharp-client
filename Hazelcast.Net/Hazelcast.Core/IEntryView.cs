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
    /// <summary>IEntryView represents a readonly view of a map entry.</summary>
    public interface IEntryView<TKey, TValue>
    {
        /// <summary>Returns the cost (in bytes) of the entry.</summary>
        /// <remarks>
        ///     Returns the cost (in bytes) of the entry.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns -1 if statistics is not enabled.
        ///     </p>
        /// </remarks>
        /// <returns>cost in bytes</returns>
        long Cost { get; }

        /// <summary>Returns the creation time of the entry.</summary>
        /// <remarks>
        ///     Returns the creation time of the entry.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns -1 if statistics is not enabled.
        ///     </p>
        /// </remarks>
        /// <returns>creation time</returns>
        long CreationTime { get; }

        /// <summary>Returns the expiration time of the entry.</summary>
        /// <remarks>Returns the expiration time of the entry.</remarks>
        /// <returns>expiration time</returns>
        long ExpirationTime { get; }

        /// <summary>Returns number of hits of the entry.</summary>
        /// <remarks>
        ///     Returns number of hits of the entry.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns -1 if statistics is not enabled.
        ///     </p>
        /// </remarks>
        /// <returns>hits</returns>
        long Hits { get; }

        /// <summary>Returns the key of the entry.</summary>
        /// <remarks>Returns the key of the entry.</remarks>
        /// <returns>key</returns>
        TKey Key { get; }

        /// <summary>Returns the last access time to the entry.</summary>
        /// <remarks>
        ///     Returns the last access time to the entry.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns -1 if statistics is not enabled.
        ///     </p>
        /// </remarks>
        /// <returns>last access time</returns>
        long LastAccessTime { get; }

        /// <summary>Returns the last time value is flushed to mapstore.</summary>
        /// <remarks>
        ///     Returns the last time value is flushed to mapstore.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns -1 if statistics is not enabled.
        ///     </p>
        /// </remarks>
        /// <returns>last store time</returns>
        long LastStoredTime { get; }

        /// <summary>Returns the last time value is updated.</summary>
        /// <remarks>
        ///     Returns the last time value is updated.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns -1 if statistics is not enabled.
        ///     </p>
        /// </remarks>
        /// <returns>last update time</returns>
        long LastUpdateTime { get; }

        /// <summary>Returns the value of the entry.</summary>
        /// <remarks>Returns the value of the entry.</remarks>
        /// <returns>value</returns>
        TValue Value { get; }

        /// <summary>Returns the version of the entry</summary>
        /// <returns>version</returns>
        long Version { get; }
    }
}