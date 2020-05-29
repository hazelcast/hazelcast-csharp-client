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
using System.Collections.ObjectModel;

namespace Hazelcast.Networking
{
    public class HazelcastProperty
    {
        public HazelcastProperty(string name, string defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue;
        }

        public string Name { get; }

        public string DefaultValue { get; }
    }

    public class HazelcastProperties // TODO: kill this eventually
    {
        public static readonly HazelcastProperty ShuffleMemberList =
            new HazelcastProperty("hazelcast.client.shuffle.member.list", "true");

        public static readonly HazelcastProperty HeartbeatTimeout =
            new HazelcastProperty("hazelcast.client.heartbeat.timeout", "60000");

        public static readonly HazelcastProperty HeartbeatInterval =
            new HazelcastProperty("hazelcast.client.heartbeat.interval", "5000");

        public static readonly HazelcastProperty EventThreadCount =
            new HazelcastProperty("hazelcast.client.event.thread.count", "5");

        public static readonly HazelcastProperty EventQueueCapacity =
            new HazelcastProperty("hazelcast.client.event.queue.capacity", "1000000");

        public static readonly HazelcastProperty InvocationTimeoutSeconds =
            new HazelcastProperty("hazelcast.client.invocation.timeout.seconds", "120");

        public static readonly HazelcastProperty ConfigFilePath = new HazelcastProperty("hazelcast.client.config", ".");

        public static readonly HazelcastProperty LoggingLevel = new HazelcastProperty("hazelcast.logging.level", "all");

        public static readonly HazelcastProperty LoggingType = new HazelcastProperty("hazelcast.logging.type", "none");


        public static readonly HazelcastProperty CloudUrlBase =
            new HazelcastProperty("hazelcast.client.cloud.url", "https://coordinator.hazelcast.cloud");


        public static readonly HazelcastProperty ReconciliationIntervalSecondsProperty =
            new HazelcastProperty("hazelcast.invalidation.reconciliation.interval.seconds", "60");

        public static readonly HazelcastProperty MinReconciliationIntervalSecondsProperty =
            new HazelcastProperty("hazelcast.invalidation.min.reconciliation.interval.seconds", "30");

        public static readonly HazelcastProperty MaxToleratedMissCountProperty =
            new HazelcastProperty("hazelcast.invalidation.max.tolerated.miss.count", "10");


        /// <summary>
        /// Certificate Name to be validated against SAN field of the remote certificate, if not present then the CN part of the Certificate Subject.
        /// </summary>
        public static readonly string CertificateName = "CertificateServerName";

        /// <summary>
        /// Certificate File path.
        /// </summary>
        public static readonly string CertificateFilePath = "CertificateFilePath";

        /// <summary>
        /// Password need to import the certificates.
        /// </summary>
        public static readonly string CertificatePassword = "CertificatePassword";

        /// <summary>
        /// SSL/TLS protocol. string value of enum type <see cref="System.Security.Authentication.SslProtocols"/>
        /// </summary>
        public static readonly string SslProtocol = "SslProtocol";

        /// <summary>
        /// specifies whether the certificate revocation list is checked during authentication.
        /// </summary>
        public static readonly string CheckCertificateRevocation = "CheckCertificateRevocation";

        /// <summary>
        /// The property is used to configure ssl to enable certificate chain validation.
        /// </summary>
        public static readonly string ValidateCertificateChain = "ValidateCertificateChain";

        /// <summary>
        /// The property is used to configure ssl to enable Certificate name validation
        /// </summary>
        public static readonly string ValidateCertificateName = "ValidateCertificateName";


        internal IReadOnlyDictionary<string, string> Properties { get; }

        internal HazelcastProperties(IDictionary<string, string> properties)
        {
            Properties = new ReadOnlyDictionary<string, string>(properties);
        }

        internal string StringValue(string name) => Properties.TryGetValue(name, out var val) ? val : null;

        internal string StringValue(HazelcastProperty hazelcastProperty)
        {
            if (Properties.TryGetValue(hazelcastProperty.Name, out var val))
            {
                return val;
            }
            var envVarValue = Environment.GetEnvironmentVariable(hazelcastProperty.Name);
            return envVarValue ?? hazelcastProperty.DefaultValue;
        }

        internal bool BoolValue(HazelcastProperty hazelcastProperty) => Convert.ToBoolean(StringValue(hazelcastProperty));
        internal int IntValue(HazelcastProperty hazelcastProperty) => Int32.Parse(StringValue(hazelcastProperty));
        internal long LongValue(HazelcastProperty hazelcastProperty) => Int64.Parse(StringValue(hazelcastProperty));
    }
}
