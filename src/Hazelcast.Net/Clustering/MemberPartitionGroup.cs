// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Networking;
using Microsoft.Extensions.Logging;
namespace Hazelcast.Clustering
{
    internal class MemberPartitionGroup : ISubsetClusterMembers
    {
        public const string VersionJsonField = "version";
        public const string PartitionGroupRootJsonField = "memberGroups";
        public const string PartitionGroupJsonField = "groups";
        public const int InvalidVersion = -1;

        private NetworkingOptions _networkingOptions;
        private ILogger _logger;
        private readonly ReaderWriterLockSlim _mutex = new ReaderWriterLockSlim();
        private MemberGroups _currentGroups
            = new MemberGroups(new List<HashSet<Guid>>(), InvalidVersion, Guid.Empty, Guid.Empty);
        public MemberPartitionGroup(NetworkingOptions networkingOptions, ILogger logger)
        {
            _networkingOptions = networkingOptions;
            _logger = logger;
        }


#region SubsetPicking

        /// <summary>
        /// Gets the best group between two partition groups object. Returns <see cref="MemberGroups.SelectedGroup"/> if newGroup is null.
        /// </summary>
        internal MemberGroups PickBestGroup(MemberGroups newGroup)
        {
            if (newGroup is null || newGroup.Version <= 0)
                return _currentGroups;

            var isCurrentNull = _currentGroups.MemberReceivedFrom == Guid.Empty
                                || _currentGroups.SelectedGroup.Count == 0
                                || _currentGroups.ClusterId != newGroup.ClusterId;

            // Pick authenticator's group.
            if (isCurrentNull && _currentGroups.MemberReceivedFrom != newGroup.MemberReceivedFrom)
                return newGroup;

            // Pick most overlapped group.
            if (isCurrentNull == false)
            { // Given group is stale. Stick with current one.
                if (_currentGroups.Version <= newGroup.Version)
                {
                    var pickedGroup = GetMostOverlappedGroup(newGroup.ClusterId, _currentGroups.MemberReceivedFrom, newGroup);

                    if (pickedGroup.SelectedGroup.Count > 0)
                        return pickedGroup;
                }
                else
                {
                    return _currentGroups;
                }
            }

            // Pick biggest group.
            return GetBiggestGroup(newGroup);
        }

        // internal for testing
        internal MemberGroups GetMostOverlappedGroup(Guid clusterId, Guid memberIdOfGroup, MemberGroups newGroups)
        {
            // Find the group that has the most overlap with the given groups.
            var maxOverlap = 0;
            ICollection<Guid> mostOverlappedGroup = null;

            foreach (var examinedGroup in newGroups.Groups)
            {
                if (examinedGroup.Contains(memberIdOfGroup) == false)
                    continue;

                var overlap = _currentGroups.SelectedGroup.Intersect(examinedGroup).Count();
                if (overlap > maxOverlap)
                {
                    maxOverlap = overlap;
                    mostOverlappedGroup = examinedGroup;
                }
            }

            return mostOverlappedGroup is { Count: > 0 }
                // Selected the new group that has the most overlap with the current group.
                ? new MemberGroups(newGroups.Groups, newGroups.Version, newGroups.ClusterId, memberIdOfGroup)
                : _currentGroups;
        }

        // internal for testing
        internal MemberGroups GetBiggestGroup(MemberGroups newGroup)
        {
            var maxCount = int.MinValue;
            HashSet<Guid> biggestGroup = null;

            for (var i = 0; i < newGroup.Groups.Count; i++)
            {
                if (newGroup.Groups[i].Count > maxCount)
                {
                    maxCount = newGroup.Groups[i].Count;
                    biggestGroup = newGroup.Groups[i];
                }
            }

            return biggestGroup == null
                ? new MemberGroups(new List<HashSet<Guid>>(), 0, Guid.Empty, Guid.Empty)
                : new MemberGroups(newGroup.Groups, newGroup.Version, newGroup.ClusterId, biggestGroup.First());
        }

#endregion
        
        public MemberGroups CurrentGroups
        {
            get
            {
                try
                {
                    _mutex.EnterReadLock();
                    return _currentGroups;
                }
                finally
                {
                    _mutex.ExitReadLock();
                }
            }
        }

        public HashSet<Guid> GetSubsetMemberIds() => CurrentGroups.SelectedGroup;

        public void SetSubsetMembers(MemberGroups newGroup)
        {
            _mutex.EnterUpgradeableReadLock();

            try
            {
                var pickedGroup = PickBestGroup(newGroup);
                _mutex.EnterWriteLock();
                var old = _currentGroups;
                _currentGroups = pickedGroup;
                _logger.IfDebug()?.LogDebug("Updated member partition group. Old group: {OldGroup} New group: {PickedGroup}", old, pickedGroup);
            }
            finally
            {
                _mutex.ExitWriteLock();
                _mutex.ExitUpgradeableReadLock();
            }
        }

        public void RemoveSubsetMember(Guid memberId)
        {
            try
            {
                _mutex.EnterWriteLock();
                if (_currentGroups.SelectedGroup.Contains(memberId))
                {
                    var clearedGroup = new List<HashSet<Guid>>();

                    foreach (var group in _currentGroups.Groups)
                    {
                        var cleared = group.Where(id => id != memberId).ToHashSet();
                        if (cleared.Count > 0)
                        {
                            clearedGroup.Add(cleared);
                        }
                    }

                    var newGroup = new MemberGroups(clearedGroup, _currentGroups.Version, _currentGroups.ClusterId, _currentGroups.MemberReceivedFrom);
                    var old = _currentGroups;
                    _currentGroups = newGroup.SelectedGroup.Count > 0
                                     && newGroup.SelectedGroup.Contains(_currentGroups.MemberReceivedFrom)
                        ? newGroup
                        : new MemberGroups(new List<HashSet<Guid>>(0),
                            InvalidVersion,
                            Guid.Empty,
                            Guid.Empty);

                    _logger.IfDebug()?.LogDebug("Removed Member[{MemberId}] and updated member partition group. " +
                                                "Old group: {OldGroup} New group: {PickedGroup}", memberId, old, _currentGroups);
                }
            }
            finally
            {
                _mutex.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            _mutex.Dispose();
        }
    }
}
