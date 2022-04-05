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

namespace Hazelcast.Exceptions
{
    /// <summary>
    /// Defines common exception messages.
    /// </summary>
    internal static class ExceptionMessages
    {
        /// <summary>
        /// Internal error.
        /// </summary>
        public const string InternalError = "Internal error.";
        
        /// <summary>
        /// Not enough bytes.
        /// </summary>
        public const string NotEnoughBytes = "Not enough bytes.";

        /// <summary>
        /// Invalid target.
        /// </summary>
        public const string InvalidTarget = "Invalid target.";

        /// <summary>
        /// Value cannot be null nor empty.
        /// </summary>
        public const string NullOrEmpty = "Value cannot be null nor empty.";

        /// <summary>
        /// Cached value is not of the expected type.
        /// </summary>
        public const string InvalidCacheCast = "Cached value is not of the expected type.";

        /// <summary>
        /// Property is now read only and cannot be modified.
        /// </summary>
        public const string PropertyIsNowReadOnly = "The property is now readonly and cannot be modified.";

        /// <summary>
        /// Default message for the configuration exception.
        /// </summary>
        public const string ConfigurationException = "Invalid configuration.";

        /// <summary>
        /// Default message for the serialization exception.
        /// </summary>
        public const string SerializationException = "Serialization or de-serialization error.";

        /// <summary>
        /// Default message for the invalid portable field exception.
        /// </summary>
        public const string InvalidPortableFieldException = "Invalid portable field.";

        /// <summary>
        /// Default message for the service factory exception.
        /// </summary>
        public const string ServiceFactoryException = "Failed to create an instance.";

        /// <summary>
        /// Default message for the transaction exception.
        /// </summary>
        public const string TransactionException = "Transaction exception.";

        /// <summary>
        /// Default message for the transaction not active exception.
        /// </summary>
        public const string TransactionNotActiveException = "Missing an active transaction.";

        /// <summary>
        /// Default message for the transaction timed out exception.
        /// </summary>
        public const string TransactionTimedOutException = "Transaction timed out.";

        /// <summary>
        /// Default message for the authentication exception.
        /// </summary>
        public const string AuthenticationException = "Failed to authenticate.";

        /// <summary>
        /// Default message for the client not connected exception.
        /// </summary>
        public const string ClientNotConnectedException = "Hazelcast client is not connected.";

        /// <summary>
        /// Default message for the connection exception.
        /// </summary>
        public const string ConnectionException = "Failed to connect to a member.";

        /// <summary>
        /// Default message for the target disconnected exception.
        /// </summary>
        public const string TargetDisconnectedException = "Target disconnected.";

        /// <summary>
        /// Default message for the target unreachable exception.
        /// </summary>
        public const string TargetUnreachableException = "Target not reachable.";

        /// <summary>
        /// Default message for the timeout exception.
        /// </summary>
        public const string Timeout = "Operation timed out.";

        /// <summary>
        /// Default message for the timeout exception with details in an inner exception.
        /// </summary>
        public const string TimeoutWithInner = "Operation timed out (see inner exception).";
    }
}
