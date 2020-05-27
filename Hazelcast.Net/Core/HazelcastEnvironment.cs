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

namespace Hazelcast.Core
{

    /// <summary>
    /// Provides information about the current Hazelcast environment.
    /// </summary>
    public static class HazelcastEnvironment
    {
        private static int? GetInt32(string variableName)
        {
            var variableValue = Environment.GetEnvironmentVariable(variableName);
            if (variableValue == null) return null;
            if (int.TryParse(variableName, out var value)) return value;
            throw new EnvironmentException($"Environment variable {variableName} must be a valid integer (Int32).");
        }

        private static long? GetInt64(string variableName)
        {
            var variableValue = Environment.GetEnvironmentVariable(variableName);
            if (variableValue == null) return null;
            if (long.TryParse(variableName, out var value)) return value;
            throw new EnvironmentException($"Environment variable {variableName} must be a valid long integer (Int64).");
        }

        /// <summary>
        /// Provides information about the current Hazelcast clock environment.
        /// </summary>
        public static class Clock
        {
            /// <summary>
            /// Gets the environment variable name of the clock offset.
            /// </summary>
            public const string OffsetName =
                "com.hazelcast.clock.offset";

            /// <summary>
            /// Gets the clock offset, or null if it is not specified.
            /// </summary>
            public static long? Offset => GetInt64(OffsetName);
        }

        /// <summary>
        /// Provides information about the current Hazelcast near cache environment.
        /// </summary>
        public static class NearCache
        {
            /// <summary>
            /// Gets the environment variable name of the near cache reconciliation interval.
            /// </summary>
            public const string ReconciliationIntervalSecondsName =
                "hazelcast.invalidation.reconciliation.interval.seconds";

            /// <summary>
            /// Gets the near cache reconciliation interval, or null if it is not specified.
            /// </summary>
            public static int? ReconciliationIntervalSeconds => GetInt32(ReconciliationIntervalSecondsName);

            /// <summary>
            /// Gets the environment variable name of the near cache minimal reconciliation interval.
            /// </summary>
            public const string MinReconciliationIntervalSecondsName =
                "hazelcast.invalidation.min.reconciliation.interval.seconds";

            /// <summary>
            /// Gets the near cache minimal reconciliation interval, or null if it is not specified.
            /// </summary>
            public static int? MinReconciliationIntervalSeconds => GetInt32(MinReconciliationIntervalSecondsName);

            /// <summary>
            /// Gets the environment variable name of near cache maximal tolerated miss count.
            /// </summary>
            public const string MaxToleratedMissCountName
                = "hazelcast.invalidation.max.tolerated.miss.count";

            /// <summary>
            /// Gets the near cache maximal tolerated miss count, or null if it is not specified.
            /// </summary>
            public static int? MaxToleratedMissCount => GetInt32(MaxToleratedMissCountName);
        }

        /// <summary>
        /// Provides information about the current Hazelcast invocation environment.
        /// </summary>
        public static class Invocation
        {
            /// <summary>
            /// Gets the environment variable name of the default invocation retry delay.
            /// </summary>
            public const string MinRetryDelayMillisecondsName =
                "hazelcast.client.invocation.retry.pause.millis";

            /// <summary>
            /// Gets the environment variable name of the default invocation timeout.
            /// </summary>
            public const string DefaultTimeoutSecondsName =
                "hazelcast.client.invocation.timeout.seconds";

            /// <summary>
            /// Gets the default invocation retry delay.
            /// </summary>
            public static int? MinRetryDelayMilliseconds => GetInt32(MinRetryDelayMillisecondsName);

            /// <summary>
            /// Gets the default invocation timeout.
            /// </summary>
            public static int? DefaultTimeoutSeconds => GetInt32(DefaultTimeoutSecondsName);
        }
    }
}