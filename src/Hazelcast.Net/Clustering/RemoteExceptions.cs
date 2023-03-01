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

using System;
using System.Collections.Generic;
using System.Text;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Models;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Maps remote exceptions to C# exceptions.
    /// </summary>
    internal static class RemoteExceptions
    {
        private static readonly HashSet<RemoteError> RetryableExceptions = new HashSet<RemoteError>
        {
            RemoteError.CallerNotMember,
            RemoteError.MemberLeft,
            RemoteError.PartitionMigrating,
            RemoteError.RetryableHazelcast,
            RemoteError.RetryableIO,
            RemoteError.TargetNotMember,
            RemoteError.WrongTarget,
            RemoteError.TargetNotReplicaException,
            RemoteError.CannotReplicateException,
            RemoteError.HazelcastInstanceNotActive,

            // this one must *not* automatically retryable, because it requires more
            // checks on the client and message in order to decide whether to retry
            //RemoteError.TargetDisconnected,
        };

        /// <summary>
        /// Creates a C# exception that represents an exception that was thrown remotely on a server.
        /// </summary>
        /// <param name="memberId">The unique identifier of the member that threw the exception.</param>
        /// <param name="errorHolders">Server errors.</param>
        /// <returns>An exception representing the specified server errors.</returns>
        internal static RemoteException CreateException(Guid memberId, IEnumerable<ErrorHolder> errorHolders)
        {
            if (errorHolders == null) throw new ArgumentNullException(nameof(errorHolders));

            return CreateException(memberId, errorHolders.GetEnumerator());
        }

        /// <summary>
        /// Create a C# exception that represents an exception that was thrown remotely on a server.
        /// </summary>
        /// <param name="memberId">The unique identifier of the member that threw the exception.</param>
        /// <param name="errorHolders">Server errors.</param>
        /// <returns>An exception representing the specified server errors.</returns>
        private static RemoteException CreateException(Guid memberId, IEnumerator<ErrorHolder> errorHolders)
        {
            if (errorHolders == null) throw new ArgumentNullException(nameof(errorHolders));

            if (!errorHolders.MoveNext())
                return null;

            var errorHolder = errorHolders.Current;
            if (errorHolder == null) return new RemoteException(memberId, RemoteError.Undefined);

            var innerException = CreateException(memberId, errorHolders);

            var error = RemoteError.Undefined;
            if (Enum.IsDefined(typeof(RemoteError), errorHolder.ErrorCode))
                error = (RemoteError) errorHolder.ErrorCode;
            var retryable = RetryableExceptions.Contains(error);

            var serverStackTrace = new StringBuilder();
            var first = true;
            foreach (var stackTraceElement in errorHolder.StackTraceElements)
            {
                if (first) first = false; else serverStackTrace.AppendLine();
                serverStackTrace.Append("   ").Append(stackTraceElement);
            }

            return new RemoteException(memberId, error, errorHolder.Message, innerException, serverStackTrace.ToString(), retryable);
        }
    }
}
