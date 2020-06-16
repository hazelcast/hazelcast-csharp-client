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
using System.Collections.Generic;
using System.Text;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Data;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Maps server error codes to C# exceptions.
    /// </summary>
    internal static class ClientProtocolExceptions
    {
        private static readonly HashSet<ClientProtocolError> RetryableErrors = new HashSet<ClientProtocolError>
        {
            ClientProtocolError.CallerNotMember,
            ClientProtocolError.MemberLeft,
            ClientProtocolError.PartitionMigrating,
            ClientProtocolError.RetryableHazelcast,
            ClientProtocolError.RetryableIO,
            ClientProtocolError.TargetNotMember,
            ClientProtocolError.WrongTarget,
            ClientProtocolError.TargetNotReplicaException,
            ClientProtocolError.CannotReplicateException,
            ClientProtocolError.HazelcastInstanceNotActive,

            // this one must *not* automatically retryable, because it requires more
            // checks on the client and message in order to decide whether to retry
            //ClientProtocolErrors.TargetDisconnected,
        };

        /// <summary>
        /// Creates an exception representing server errors.
        /// </summary>
        /// <param name="errorHolders">Server errors.</param>
        /// <returns>An exception representing the specified server errors.</returns>
        internal static ClientProtocolException CreateException(IEnumerable<ErrorHolder> errorHolders)
        {
            if (errorHolders == null) throw new ArgumentNullException(nameof(errorHolders));

            return CreateException(errorHolders.GetEnumerator());
        }

        /// <summary>
        /// Create an exception representing server errors.
        /// </summary>
        /// <param name="errorHolders">Server errors.</param>
        /// <returns>An exception representing the specified server errors.</returns>
        private static ClientProtocolException CreateException(IEnumerator<ErrorHolder> errorHolders)
        {
            if (errorHolders == null) throw new ArgumentNullException(nameof(errorHolders));

            if (!errorHolders.MoveNext())
                return null;

            var errorHolder = errorHolders.Current;
            if (errorHolder == null) return new ClientProtocolException(ClientProtocolError.Undefined);

            var innerException = CreateException(errorHolders);

            var error = ClientProtocolError.Undefined;
            if (Enum.IsDefined(typeof(ClientProtocolError), errorHolder.ErrorCode))
                error = (ClientProtocolError) errorHolder.ErrorCode;

            var retryable = RetryableErrors.Contains(error);
            var exception = new ClientProtocolException(error, errorHolder.Message, innerException, retryable);

            var sb = new StringBuilder();
            var first = true;
            foreach (var stackTraceElement in errorHolder.StackTraceElements)
            {
                if (first) first = false;
                else sb.AppendLine();
                sb.Append("   ").Append(stackTraceElement);
            }

            exception.Data.Add("server", sb.ToString());
            return exception;
        }
    }
}
