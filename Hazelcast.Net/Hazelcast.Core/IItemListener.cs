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

namespace Hazelcast.Core
{
    /// <summary>
    ///     Item listener for
    ///     <see cref="IQueue{E}"/>
    ///     ,
    ///     <see cref="IHSet{E}"/>
    ///     and
    ///     <see cref="IHList{E}"/>
    /// </summary>
    public interface IItemListener<TE> : IEventListener
    {
        /// <summary>Invoked when an item is added.</summary>
        /// <remarks>Invoked when an item is added.</remarks>
        /// <param name="item">added item</param>
        void ItemAdded(ItemEvent<TE> item);

        /// <summary>Invoked when an item is removed.</summary>
        /// <remarks>Invoked when an item is removed.</remarks>
        /// <param name="item">removed item.</param>
        void ItemRemoved(ItemEvent<TE> item);
    }
}