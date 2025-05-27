﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Core;

/// <summary>
/// Represents an <see cref="IAsyncDisposable"/> that executes a <see cref="Func{Task}"/> when disposed.
/// </summary>
internal class DisposeAsyncAction : IAsyncDisposable
{
    private readonly Func<ValueTask> _action;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisposeAction"/> class with an action.
    /// </summary>
    /// <param name="action">The action to execute when this instance is disposed.</param>
    public DisposeAsyncAction(Func<ValueTask> action)
    {
        _action = action;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => _action();
}
