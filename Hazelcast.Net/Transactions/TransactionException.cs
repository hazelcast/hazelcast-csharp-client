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
using Hazelcast.Exceptions;

namespace Hazelcast.Transactions
{
    /// <summary>
    ///     A
    ///     <see cref="HazelcastException">Hazelcast.Core.HazelcastException</see>
    ///     that is thrown when something goes wrong while dealing with transactions and transactional
    ///     data-structures.
    /// </summary>
    [Serializable]
    internal class TransactionException : HazelcastException
    {
        public TransactionException()
        {
        }

        public TransactionException(string message) : base(message)
        {
        }

        public TransactionException(string message, Exception cause) : base(message, cause)
        {
        }

        public TransactionException(Exception cause) : base(cause)
        {
        }
    }
}