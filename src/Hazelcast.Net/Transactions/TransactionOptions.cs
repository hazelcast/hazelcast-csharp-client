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

namespace Hazelcast.Transactions
{
    /// <summary>Contains the configuration for a transaction</summary>
    public sealed class TransactionOptions
    {
        private int _durability = 1;

#pragma warning disable CA1008 // Enums should have zero value - no, "default" is not a valid value
        public enum TransactionType
#pragma warning restore CA1008
        {
            /// <summary>
            /// Commits the transaction in two distinct phases.
            /// </summary>
            /// <remarks>
            /// <para>Two-phase commits commits in two phases: the first phase tries to prepare the commit, and fails in
            /// case of conflicts. The second phase actually writes the changes. If the first phase succeeded, then the
            /// second phase is guaranteed to succeed.</para>
            /// <para>Hazelcast also provides three phases transactions, by automatically copying the backlog to another
            /// member so that in case of failure during a commit, another member can result the commit from backup.</para>
            /// </remarks>
            TwoPhase = 1,

            /// <summary>
            /// Commits the transaction in one single final phase.
            /// </summary>
            /// <remarks>
            /// <para>Because there is no preparation of the transaction, conflicts are not detected. If there is a conflict,
            /// then when the transaction commits, some of the changes are written while others are not, leaving the system
            /// in a potentially permanent inconsistent state.</para>
            /// </remarks>
            OnePhase = 2
        }

        /// <summary>
        /// Gets or sets the transaction durability.
        /// </summary>
        public int Durability
        {
            get => _durability;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");
                _durability = value;
            }
        }

        /// <summary>
        /// Gets or sets the transaction timeout.
        /// </summary>
        // TODO: what happens when timeout is -1 (infinite)?
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Gets or sets the type of the transaction.
        /// </summary>
        public TransactionType Type { get; set; } = TransactionType.TwoPhase;
    }
}
