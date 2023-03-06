// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    /// <summary>
    /// Defines a retry strategy.
    /// </summary>
    internal interface IRetryStrategy
    {
        /// <summary>
        /// Determines whether it is possible to retry, optionally waiting for some time.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns><c>true</c> if it is possible to retry; otherwise <c>false</c>.</returns>
        ValueTask<bool> WaitAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Restarts the strategy.
        /// </summary>
        void Restart();
    }
}
