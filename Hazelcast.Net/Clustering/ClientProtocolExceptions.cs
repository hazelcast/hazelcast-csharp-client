using System;
using System.Collections.Generic;
using System.Text;
using Hazelcast.Exceptions;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Data;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Maps server error codes to C# exceptions.
    /// </summary>
    internal static class ClientProtocolExceptions
    {
        private static readonly HashSet<ClientProtocolErrors> RetryableErrors = new HashSet<ClientProtocolErrors>
        {
            ClientProtocolErrors.CallerNotMember,
            ClientProtocolErrors.MemberLeft,
            ClientProtocolErrors.PartitionMigrating,
            ClientProtocolErrors.RetryableHazelcast,
            ClientProtocolErrors.RetryableIO,
            ClientProtocolErrors.TargetNotMember,
            ClientProtocolErrors.WrongTarget,
            ClientProtocolErrors.TargetNotReplicaException,
            ClientProtocolErrors.CannotReplicateException,
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
            if (errorHolder == null) return new ClientProtocolException(ClientProtocolErrors.Undefined);

            var innerException = CreateException(errorHolders);

            var error = ClientProtocolErrors.Undefined;
            if (Enum.IsDefined(typeof(ClientProtocolErrors), errorHolder.ErrorCode))
                error = (ClientProtocolErrors) errorHolder.ErrorCode;

            var retryable = RetryableErrors.Contains(error);
            var exception = new ClientProtocolException(error, errorHolder.Message, innerException, retryable);

            var sb = new StringBuilder();
            var first = true;
            foreach (var stackTraceElement in errorHolder.StackTraceElements)
            {
                if (first) first = false;
                else sb.AppendLine();
                sb.Append("   at ").Append(stackTraceElement);
            }

            exception.Data.Add("server", sb.ToString());
            return exception;
        }
    }
}
