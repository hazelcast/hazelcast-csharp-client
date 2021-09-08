// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Hazelcast.Exceptions;
using Hazelcast.Core;

namespace Hazelcast.Sql
{
    /// <summary>
    /// Represents the exception that is thrown by the SQL service in case of an error.
    /// </summary>
    [Serializable]
    public class HazelcastSqlException : HazelcastException
    {
        // NOTE: as per CA1032 we implement all constructors, but... keep them private/internal.
        #pragma warning disable IDE0051 // Remove unused private members - of course

        // TODO: consider adding values to Data["..."] the way RemoteException does?

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastSqlException"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private HazelcastSqlException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastSqlException"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private HazelcastSqlException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastSqlException"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private HazelcastSqlException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastSqlException"/> class.
        /// </summary>
        private HazelcastSqlException(Guid clientId, int errorCode, string message)
            : base(message)
        {
            ClientId = clientId;
            ErrorCode = errorCode;
        }

        // FIXME [Oleksii] what is ClientId?
        //
        // the 1st ctor below is used for client-side detected anomalies, i.e. a member
        // is not involved, and then the id is the client id
        //
        // the 2nd ctor below is used when the response to a request contains an error,
        // in which case the error is deserialized by the codec and it contains the
        // originating member id, ie the id of the member on which the error occurred
        //
        // so ClientId can be two totally different things and that is wrong. besides,
        // these client/member identifiers could be used outside of SQL
        // - would it make sense for HazelcastException to always include client id?
        // - would it make sense for some exceptions to always include member id?
        //   that would be for RemoteException, really, which is the other remote
        //   error that the server can raise

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastSqlException"/> class.
        /// </summary>
        internal HazelcastSqlException(Guid clientId, SqlErrorCode errorCode, string message)
            : this(clientId, (int)errorCode, message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastSqlException"/> class.
        /// </summary>
        internal HazelcastSqlException(SqlError error)
            : this(error.OriginatingMemberId, error.Code, error.Message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastSqlException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected HazelcastSqlException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ClientId = info.GetGuid(nameof(ClientId));
            ErrorCode = info.GetInt32(nameof(ErrorCode));
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(ClientId), ClientId);
            info.AddValue(nameof(ErrorCode), ErrorCode);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Get the identifier of the FIXME - see above, is this a client or member ID?
        /// </summary>
        public Guid ClientId { get; }

        /// <summary>
        /// Gets the code representing the error that occurred.
        /// </summary>
        internal int ErrorCode { get; } // TODO: consider exposing this as a public enum?
    }
}