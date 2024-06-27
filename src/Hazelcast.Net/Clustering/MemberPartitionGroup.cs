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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Models;
namespace Hazelcast.Clustering
{
    internal class MemberPartitionGroup : ISubsetClusterMembers
    {
        public const string VersionJsonField = "version";
        public const string PartitionGroupJsonField = "partition.groups";
        public const int InvalidVersion = -1;
        private object _mutex = new object();
        private MemberGroups _currentGroups
            = new MemberGroups(new List<IList<Guid>>(0), -1, Guid.Empty, Guid.Empty);


#region SubsetPicking

        /// <summary>
        /// Gets the best group between two partition groups object. Returns <see cref="MemberGroups.SelectedGroup"/> if newGroup is null.
        /// </summary>
        internal MemberGroups PickBestGroup(MemberGroups newGroup)
        {
            if (newGroup is null)
                return _currentGroups;

            var shouldNeedNewGroup = _currentGroups.ClusterId != newGroup.ClusterId || _currentGroups.SelectedGroup.Count == 0;

            if (shouldNeedNewGroup && newGroup.MemberReceivedFrom != Guid.Empty && newGroup.SelectedGroup.Count > 0)
                return newGroup;

            if (!shouldNeedNewGroup)
            {
                var pickedGroup = GetMostOverlappedGroup(newGroup.ClusterId, newGroup.MemberReceivedFrom, newGroup);

                if (pickedGroup.SelectedGroup.Count > 0)
                    return pickedGroup;
            }

            return GetBiggestGroup(newGroup);
        }

        // internal for testing
        internal MemberGroups GetMostOverlappedGroup(Guid clusterId, Guid memberIdOfGroup, MemberGroups newGroups)
        {
            if (_currentGroups.Version > newGroups.Version)
            {
                // Given group is stale. Stick with current one.
                return _currentGroups;
            }

            // Find the group that has the most overlap with the given groups.
            var maxOverlap = 0;
            ICollection<Guid> mostOverlappedGroup = null;
            foreach (var examinedGroup in newGroups.Groups)
            {
                var overlap = _currentGroups.SelectedGroup.Intersect(examinedGroup).Count();
                if (overlap > maxOverlap)
                {
                    maxOverlap = overlap;
                    mostOverlappedGroup = examinedGroup;
                }
            }
          
            return mostOverlappedGroup is { Count: > 0 } ? newGroups : _currentGroups;
        }

        // internal for testing
        internal MemberGroups GetBiggestGroup(MemberGroups groupRight)
        {
            var maxCount = int.MinValue;
            IList<Guid> biggestGroup = null;

            for (var i = 0; i < groupRight.Groups.Count; i++)
            {
                if (_currentGroups.Groups[i].Count > maxCount)
                {
                    maxCount = groupRight.Groups[i].Count;
                    biggestGroup = groupRight.Groups[i];
                }
            }

            return biggestGroup == null
                ? new MemberGroups(new List<IList<Guid>>(0), 0, Guid.Empty, Guid.Empty)
                : new MemberGroups(groupRight.Groups, groupRight.Version, groupRight.ClusterId, biggestGroup.First());
        }

#endregion

        // internal for testing
        internal MemberGroups CurrentGroups => _currentGroups;
        
        public IReadOnlyList<Guid> GetSubsetMembers() => _currentGroups.SelectedGroup;

        public void SetSubsetMembers(MemberGroups newGroup)
        {
            var pickedGroup = _currentGroups.Version == InvalidVersion ? newGroup :  PickBestGroup(newGroup);

            lock (_mutex)
            {
                _currentGroups = pickedGroup;
            }
        }
    }
}
