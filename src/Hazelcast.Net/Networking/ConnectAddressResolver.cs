// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Models;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Resolves connect addresses for members by determining whether to use internal or public addresses.
    /// </summary>
    internal class ConnectAddressResolver
    {
        private readonly NetworkingOptions _options;
        private readonly ILogger _logger;

        public ConnectAddressResolver(NetworkingOptions options, ILoggerFactory loggerFactory)
        {
            _options = options;
            _logger = loggerFactory.CreateLogger<ConnectAddressResolver>();
        }

        /// <summary>
        /// Determines whether to use public addresses.
        /// </summary>
        /// <param name="members">A collection of members.</param>
        /// <returns><c>true</c> if public addresses must be used; otherwise <c>false</c>, indicating that internal addresses can be used.</returns>
        public async Task<bool> DetermineUsePublicAddresses(IReadOnlyCollection<MemberInfo> members)
        {
            // if the user has specified its intention, respect it, otherwise try to decide
            // automatically whether to use private or public addresses by trying to reach
            // a few members

            if (_options.UsePublicAddresses is {} usePublicAddresses)
            {
                _logger.IfDebug()?.LogDebug(usePublicAddresses
                    ? "NetworkingOptions.UsePublicAddresses is true, use public addresses."
                    : "NetworkingOptions.UsePublicAddresses is false, use internal addresses.");

                return usePublicAddresses;
            }

            _logger.IfDebug()?.LogDebug("NetworkingOptions.UsePublicAddresses is not set, decide by ourselves.");

            // if ssl is enabled, then the client uses internal addresses
            if (_options.Ssl.Enabled)
            {
                _logger.IfDebug()?.LogDebug("Ssl is enabled, use internal addresses.");

                return false;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var text = new StringBuilder();
                text.Append("Members [");
                text.Append(members.Count);
                text.Append("] {");
                text.AppendLine();
                foreach (var member in members)
                {
                    text.Append("    ");
                    text.Append(member.ToShortString(false));
                    text.AppendLine();
                    foreach (var entry in member.AddressMap)
                    {
                        text.Append("        ");
                        text.Append(entry.Key);
                        text.Append(": ");
                        text.Append(entry.Value);
                        text.AppendLine();
                    }
                }
                text.Append('}');
                _logger.IfDebug()?.LogDebug(text.ToString());
            }

            // if at least one member has its internal address that matches options, assume we can use internal addresses
            if (DetermineAnyMemberInternalAddressMatchesOptions(members))
            {
                _logger.IfDebug()?.LogDebug("At least one member's internal address matches options, assume that the client can use internal addresses.");
                return false;
            }

            // if one member does not have a public address, then the client has to use internal addresses
            if (members.Any(x => x.PublicAddress is null))
            {
                _logger.IfDebug()?.LogDebug("At least one member does not have a public address, use internal addresses.");

                return false;
            }

            // else try to reach addresses to figure out which ones to use
            return await DeterminePublicAddressesAreRequired(members).CfAwait();
        }

        // determines whether at least one member has its internal address specified in options,
        // which would mean that the client can reach the configured addresses and we can use
        // internal addresses
        private bool DetermineAnyMemberInternalAddressMatchesOptions(IReadOnlyCollection<MemberInfo> members)
        {
            // both NodeJS and Java code plainly ignore ports and only focus on the host name

            var optionHosts = _options.Addresses
                .Select(x => NetworkAddress.TryParse(x, out var a) ? a : null)
                .Where(x => x != null)
                .Select(x => x.HostName);

            var memberHosts = members.Select(x => x.Address.HostName);

            return memberHosts.Intersect(optionHosts).Any();
        }

        // determines whether using public addresses is required
        // by testing a subset of all members
        private Task<bool> DeterminePublicAddressesAreRequired(IReadOnlyCollection<MemberInfo> members)
            => DeterminePublicAddressesAreRequired(members.Shuffle(), _options.AddressResolver_SampleSize);

        // determines whether using public addresses is required
        private async Task<bool> DeterminePublicAddressesAreRequired(IReadOnlyCollection<MemberInfo> members, int sampleCount)
        {
            var count = 0;
            var requirePublic = false;

            foreach (var member in members)
            {
                // we failed to find a member that can be reached at its internal address, but enough members can
                // be reached at their public addresses, so assume public addresses are required for all
                if (count++ == sampleCount && requirePublic)
                {
                    _logger.IfDebug()?.LogDebug("At least {Count} members only respond on their public address, the client has to use public addresses.", sampleCount);

                    return true;
                }

                var canReachInternal = await member.Address.TryReachAsync(_options.AddressResolver_InternalAddressTryReachTimeout).CfAwait();

                // if one member can be reached at its internal address then assume internal addresses are ok for all
                if (canReachInternal)
                {
                    _logger.IfDebug()?.LogDebug("Member {Member} responds on its internal address, assume that the client can use internal addresses.", member.Address);

                    return false;
                }

                var canReachPublic = await member.PublicAddress.TryReachAsync(_options.AddressResolver_PublicAddressTryReachTimeout).CfAwait();

                // if the member cannot be reached at its internal address but can be reached at its public address,
                // this would indicate that the client has to use public addresses, but we are going to try a few
                // more members just to be sure - maybe the failure to reach the internal address was a glitch and
                // another member will make it
                if (canReachPublic)
                {
                    _logger.IfDebug()?.LogDebug("Member {Member} does not respond on its internal address, responds on its public address {PublicAddress}.", member.Address, member.PublicAddress);

                    requirePublic = true;
                }

                // otherwise, the client cannot be reached at all - both NodeJS and Java immediately return false,
                // but really - this could very well be a glitch and we should probably try a few more members
            }

            // we failed to find a member that can be reached at its internal address, but members can be reached at
            // their public addresses, so assume public addresses are required for all
            if (requirePublic)
            {
                _logger.IfDebug()?.LogDebug("Members can only be reached at their public address, the client has to use public addresses.");
                return true;
            }

            // otherwise, we tested all members and could not reach any or them, neither on internal nor on public address,
            // and this is a sad situation indeed - we're going to go with internal addresses but... something is wrong
            _logger.IfDebug()?.LogDebug("Could not connect to any member. Assume the client can use internal addresses.");
            return false;
        }
    }
}
