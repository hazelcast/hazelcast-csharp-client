// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    /// <summary>Transactional implementation of Queue</summary>
    public interface IHTxQueue<TItem> : ITransactionalObject
    {
        /// <summary>
        /// Transactional <see cref="IHQueue{T}.OfferAsync"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<bool> OfferAsync(TItem item);

        /// <summary>
        /// Transactional <see cref="IHQueue{T}.OfferAsync"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="timeToWait"></param>
        /// <returns></returns>
        Task<bool> OfferAsync(TItem item, TimeSpan timeToWait);

        /// <summary>
        /// Transactional <see cref="IHQueue{T}.PeekAsync"/>
        /// </summary>
        /// <param name="timeToWait"></param>
        /// <returns></returns>
        Task<TItem> PeekAsync(TimeSpan timeToWait = default);

        /// <summary>
        /// Transactional <see cref="IHQueue{T}.PollAsync"/>
        /// </summary>
        /// <param name="timeToWait"></param>
        /// <returns></returns>
        Task<TItem> PollAsync(TimeSpan timeToWait = default);

        /// <summary>
        /// Transactional <see cref="IHQueue{T}.GetSizeAsync"/>
        /// </summary>
        /// <returns></returns>
        Task<int> GetSizeAsync();

        /// <summary>
        /// Transactional <see cref="IHQueue{T}.TakeAsync"/>
        /// </summary>
        /// <returns></returns>
        Task<TItem> TakeAsync();
    }
}
