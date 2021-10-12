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

namespace Hazelcast
{
    /// <summary>
    /// Represents preview options.
    /// </summary>
    /// <remarks>
    /// <para>Preview options are unsupported options that are provided to enable behaviors of the
    /// client that remain experimental and/or may break backward compatibility.</para>
    /// </remarks>
    public class PreviewOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreviewOptions"/> class.
        /// </summary>
        public PreviewOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreviewOptions"/> class.
        /// </summary>
        private PreviewOptions(PreviewOptions other)
        {
            EnableNewReconnectOptions = other.EnableNewReconnectOptions;
            EnableNewRetryOptions = other.EnableNewRetryOptions;
        }

        /// <summary>
        /// (unsupported) Whether to enable the new reconnect options.
        /// </summary>
        /// <remarks>
        /// <para>The <see cref="Networking.NetworkingOptions.ReconnectMode"/> option specifies a synchronous and an
        /// asynchronous mode, but they are both implemented in the same way. These modes actually don't make a lot
        /// of sense. And, reconnection is disabled by default.</para>
        /// <para>When the new reconnect options are enabled, this option is ignored, and replaced by the
        /// <see cref="Networking.NetworkingOptions.Reconnect"/> option, which is a boolean and indicates whether to
        /// reconnect or now. It is <c>true</c> by default. Invocations that fail because the client is reconnecting
        /// will be retried (all of them, reads and writes) until the client is reconnected, or the invocation times
        /// out.</para>
        /// </remarks>
        public bool EnableNewReconnectOptions { get; set; }

        /// <summary>
        /// (unsupported) Whether to enable the new retry options.
        /// </summary>
        /// <remarks>
        /// <para>Retrying failed invocations was originally controlled by the <see cref="Networking.NetworkingOptions.RedoOperations"/>
        /// option. This option belongs more to messaging, where we may want to enable finer-grain control of retries.</para>
        /// <para>When the new retry options are enabled, this option is ignored and replaced by the
        /// <see cref="Messaging.MessagingOptions.RetryUnsafeOperations"/> option: all safe (read) operations are retried by
        /// default, and this option controls whether to retry unsafe (write) operations. It is <c>false</c> by default.In
        /// addition, a new option is introduced, <see cref="Messaging.MessagingOptions.RetryOnClientReconnecting"/> which
        /// controls retries when the operation cannot even start because the client is reconnecting. It is <c>true</c> by default.</para>
        /// </remarks>
        public bool EnableNewRetryOptions { get; set; }

        /// <summary>
        /// Clones the options.
        /// </summary>
        public PreviewOptions Clone() => new PreviewOptions(this);
    }
}
