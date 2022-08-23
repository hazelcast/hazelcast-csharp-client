// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="CancellationTokenSource"/> class.
    /// </summary>
    internal static class CancellationTokenSourceExtensions
    {
        /// <summary>
        /// Creates a cancellation source by combining a source and a cancellation token.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The combined cancellation.</returns>
        public static CancellationTokenSource LinkedWith(this CancellationTokenSource source, CancellationToken cancellationToken)
            => CancellationTokenSource.CreateLinkedTokenSource(source.Token, cancellationToken);


        /// <summary>
        /// Throws an <see cref="OperationCanceledException"/> if this source has had cancellation requested.
        /// </summary>
        /// <param name="source">A cancellation token source.</param>
        public static void ThrowIfCancellationRequested(this CancellationTokenSource source)
            => source.Token.ThrowIfCancellationRequested();
    }
}
