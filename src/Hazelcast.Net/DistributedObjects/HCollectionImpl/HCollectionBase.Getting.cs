﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.DistributedObjects.HCollectionImpl
{
    internal partial class HCollectionBase<T> // Getting
    {
        /// <inheritdoc />
        public abstract Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract Task<bool> ContainsAsync(T item, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract Task<int> CountAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract Task<bool> ContainsAllAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken = default)
            where TItem : T;

        /// <inheritdoc />
        public abstract Task<bool> IsEmptyAsync(CancellationToken cancellationToken = default);
    }
}
