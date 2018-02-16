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

using System;
using System.Text;
using Hazelcast.Core;

namespace Hazelcast.Transaction
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
            /// of failure during a commit, another member can continue the commit from backup. For more information see the
            /// <see cref="TransactionOptions.SetDurability(int)"/>
            /// </remarks>
            TwoPhase = 1,
            [Obsolete("Use OnePhase instead")] Local = 2,

            /// <summary>The one phase transaction executes a transaction using a single step at the end; committing the changes.</summary>
            /// <remarks>
            /// The one phase transaction executes a transaction using a single step at the end; committing the changes. There
            /// is no prepare of the transactions, so conflicts are not detected. If there is a conflict, then when the transaction
            /// commits the changes, some of the changes are written and others are not; leaving the system in a potentially permanent
            /// inconsistent state.
            /// </remarks>
            OnePhase = 2
        }

        private int _durability;
        private long _timeoutMillis;

        private TransactionType _transactionType;

        /// <summary>Creates a new default configured TransactionsOptions.</summary>
        /// <remarks>
        ///     Creates a new default configured TransactionsOptions.
        ///     It will be configured with a timeout of 2 minutes, durability of 1 and a TransactionType.TWO_PHASE.
        /// </remarks>
        public TransactionOptions()
        {
            SetTimeout(2, TimeUnit.Minutes).SetDurability(1).SetTransactionType(TransactionType.TwoPhase);
        }

        /// <summary>Creates a new TransactionOptions configured with default settings.</summary>
        /// <remarks>Creates a new TransactionOptions configured with default settings.</remarks>
        /// <returns>the created default TransactionOptions.</returns>
        /// <seealso cref="TransactionOptions()">TransactionOptions()</seealso>
        public static TransactionOptions GetDefault()
        {
            return new TransactionOptions();
        }

        /// <summary>Gets the transaction durability.</summary>
        /// <remarks>Gets the transaction durability.</remarks>
        /// <returns>the transaction durability.</returns>
        /// <seealso cref="SetDurability(int)">SetDurability(int)</seealso>
        public int GetDurability()
        {
            return _durability;
        }

        /// <summary>Gets the timeout in milliseconds.</summary>
        /// <remarks>Gets the timeout in milliseconds.</remarks>
        /// <returns>the timeout in milliseconds.</returns>
        /// <seealso cref="SetTimeout(long, TimeUnit)">SetTimeout(long, TimeUnit)</seealso>
        public long GetTimeoutMillis()
        {
            return _timeoutMillis;
        }

        /// <summary>
        ///     Gets the
        ///     <see cref="TransactionType">TransactionType</see>
        ///     .
        /// </summary>
        /// <returns>the TransactionType.</returns>
        public TransactionType GetTransactionType()
        {
            return _transactionType;
        }

        /// <summary>Sets the transaction durability.</summary>
        /// <remarks>
        ///     Sets the transaction durability.
        ///     The durability is the number of machines that can take over if a member fails during a transaction
        ///     commit or rollback. This value only has meaning when
        ///     <see cref="TransactionType.TwoPhase">TransactionType.TwoPhase</see>
        ///     is selected.
        /// </remarks>
        /// <param name="durability">the durability</param>
        /// <returns>the updated TransactionOptions.</returns>
        /// <exception cref="System.ArgumentException">if durability smaller than 0.</exception>
        public TransactionOptions SetDurability(int durability)
        {
            if (durability < 0)
            {
                throw new ArgumentException("Durability cannot be negative!");
            }
            _durability = durability;
            return this;
        }

        /// <summary>Sets the timeout.</summary>
        /// <remarks>
        ///     Sets the timeout.
        ///     The timeout determines the maximum lifespan of a transaction. So if a transaction is configured with a
        ///     timeout of 2 minutes, then it will automatically rollback if it hasn't committed yet.
        /// </remarks>
        /// <param name="timeout">the timeout.</param>
        /// <param name="timeUnit">the TimeUnit of the timeout.</param>
        /// <returns>the updated TransactionOptions</returns>
        /// <exception cref="System.ArgumentException">if timeout smaller or equal than 0, or timeUnit is null.</exception>
        /// <seealso cref="GetTimeoutMillis()">GetTimeoutMillis()</seealso>
        public TransactionOptions SetTimeout(long timeout, TimeUnit timeUnit)
        {
            if (timeout <= 0)
            {
                throw new ArgumentException("Timeout must be positive!");
            }
            _timeoutMillis = timeUnit.ToMillis(timeout);
            return this;
        }

        /// <summary>
        ///     Sets the
        ///     <see cref="TransactionType">TransactionType</see>
        ///     .
        ///     A local transaction is less safe than a two phase transaction; when a member fails during the commit
        ///     of a local transaction, it could be that some of the changes are committed, while others are not and this
        ///     can leave your system in an inconsistent state.
        /// </summary>
        /// <param name="transactionType">the new TransactionType.</param>
        /// <returns>the updated TransactionOptions.</returns>
        /// <seealso cref="GetTransactionType()">GetTransactionType()</seealso>
        /// <seealso cref="SetDurability(int)">SetDurability(int)</seealso>
        public TransactionOptions SetTransactionType(TransactionType transactionType)
        {
            _transactionType = transactionType;
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("TransactionOptions");
            sb.Append("{timeoutMillis=").Append(_timeoutMillis);
            sb.Append(", durability=").Append(_durability);
            sb.Append(", txType=").Append((int) _transactionType);
            sb.Append('}');
            return sb.ToString();
        }
    }
}