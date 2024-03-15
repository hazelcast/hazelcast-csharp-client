// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

using Microsoft.Extensions.Logging;

namespace Hazelcast.Core
{
    internal static class LoggingExtensions
    {
        // TODO: log using message templates or IfDebug() for perf purposes!

        /// <summary>
        /// Returns this <see cref="ILogger"/> if the specified <see cref="LogLevel"/> is enabled, otherwise <c>null</c>.
        /// </summary>
        /// <param name="logger">This logger.</param>
        /// <param name="logLevel">The <see cref="LogLevel"/> which must be enabled.</param>
        /// <remarks>
        /// <para>Usage: <c>logger.If(LogLevel.Xxx)?.LogXxx(...)</c> - avoids building arguments for <c>LogXxx(...)</c> when not needed.</para>
        /// </remarks>
        public static ILogger If(this ILogger logger, LogLevel logLevel) => logger.IsEnabled(logLevel) ? logger : null;

        /// <summary>
        /// Returns this <see cref="ILogger"/> if <see cref="LogLevel.Debug"/> is enabled, otherwise <c>null</c>.
        /// </summary>
        /// <param name="logger">This logger.</param>
        /// <remarks>
        /// <para>Usage: <c>logger.IfDebug()?.LogDebug(...)</c> - avoids building arguments for <c>LogDebug(...)</c> when not needed.</para>
        /// </remarks>
        public static ILogger IfDebug(this ILogger logger) => logger.IsEnabled(LogLevel.Debug) ? logger : null;

        /// <summary>
        /// Returns this <see cref="ILogger"/> if <see cref="LogLevel.Warning"/> is enabled, otherwise <c>null</c>.
        /// </summary>
        /// <param name="logger">This logger.</param>
        /// <remarks>
        /// <para>Usage: <c>logger.IfWarning()?.LogWarning(...)</c> - avoids building arguments for <c>LogWarning(...)</c> when not needed.</para>
        /// </remarks>
        public static ILogger IfWarning(this ILogger logger) => logger.IsEnabled(LogLevel.Warning) ? logger : null;
    }
}
