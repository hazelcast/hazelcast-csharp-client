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
        internal HazelcastSqlException(Guid clientId, int errorCode, string message)
            : base(message)
        {
            ClientId = clientId;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastSqlException"/> class.
        /// </summary>
        internal HazelcastSqlException(Guid clientId, SqlErrorCode errorCode, string message) // FIXME here it is _cluster.ClientID = a *client* ID
            : this(clientId, (int)errorCode, message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastSqlException"/> class.
        /// </summary>
        internal HazelcastSqlException(SqlError error)
            : this(error.OriginatingMemberId, error.Code, error.Message) // FIXME here it is OriginatingMemberId = a *member* ID
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastSqlException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected HazelcastSqlException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ClientId = (Guid) info.GetValue(nameof(ClientId), typeof(Guid));
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