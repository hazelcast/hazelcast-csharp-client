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

using System;

namespace Hazelcast.Transactions
{
    /// <summary>Contains the configuration for a transaction</summary>
    public sealed class TransactionOptions
    {
        public enum TransactionType
        {
            /// <summary>The two phase commit is separated in 2 parts.</summary>
            /// <remarks>
            /// The two phase commit is separated in 2 parts. First it tries to execute the prepare; if there are any conflicts,
            /// the prepare will fail. Once the prepare has succeeded, the commit (writing the changes) can be executed.
            /// Hazelcast also provides three phase transaction by automatically copying the backlog to another member so that in case
            /// of failure during a commit, another member can continue the commit from backup.
            /// </remarks>
            TwoPhase = 1,
        }

        /// <summary>
        /// Gets or sets the transaction durability.
        /// </summary>
        public int Durability { get; set; } = 1;

        /// <summary>
        /// Gets or sets the transaction timeout.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Gets or sets the type of the transaction.
        /// </summary>
        public TransactionType Type { get; set; } = TransactionType.TwoPhase;
    }
}
