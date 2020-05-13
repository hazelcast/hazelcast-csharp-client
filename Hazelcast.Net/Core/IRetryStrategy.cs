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

namespace Hazelcast.Core
{
    /// <summary>
    /// Defines a retry strategy.
    /// </summary>
    public interface IRetryStrategy
    {
        /// <summary>
        /// Waits before retrying.
        /// </summary>
        /// <returns>Whether it is ok to retry.</returns>
        /// <remarks>
        /// <para>Returns false when the timeout has been reached.</para>
        /// </remarks>
        ValueTask<bool> WaitAsync();

        /// <summary>
        /// Restarts the strategy.
        /// </summary>
        void Restart();
    }
}