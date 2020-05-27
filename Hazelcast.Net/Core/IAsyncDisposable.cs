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

using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System
{
#if NETSTANDARD2_0
    /// <summary>
    /// Provides a mechanism for releasing unmanaged resources asynchronously.
    /// </summary>
    /// <remarks>
    /// <para>This class is a substitute for the true IAsyncDisposable which is
    /// only available in C# 8 and netstandard 2.1. Though it allows to asynchronously
    /// dispose resources, asynchronous using and other C# 8 features are of
    /// course not supported in netstandard 2.0.</para>
    /// </remarks>
    public interface IAsyncDisposable
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        ValueTask DisposeAsync();
    }
#endif
}
