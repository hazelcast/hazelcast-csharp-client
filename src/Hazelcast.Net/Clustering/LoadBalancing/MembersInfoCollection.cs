using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Hazelcast.Models;

namespace Hazelcast.Clustering.LoadBalancing
{
    public class MembersInfoCollection
    {
        /// <summary>
        /// Gets the data members.
        /// </summary>
        public ReadOnlyCollection<MemberInfo> DataMembers { get; }

        /// <summary>
        /// Gets the lite members.
        /// </summary>
        public ReadOnlyCollection<MemberInfo> LiteMembers { get; }

        public int Count => DataMembers.Count + LiteMembers.Count;

        public MembersInfoCollection(IEnumerable<MemberInfo> members)
        {
            if (members == null)
                throw new ArgumentNullException(nameof(members));

            var distinctIds = new HashSet<Guid>();
            var (liteMembers, dataMembers) = (new List<MemberInfo>(), new List<MemberInfo>());

            foreach (var member in members)
            {
                if (!distinctIds.Add(member.Id))
                    continue;

                if (member.IsLiteMember)
                    liteMembers.Add(member);
                else
                    dataMembers.Add(member);
            }

            LiteMembers = new ReadOnlyCollection<MemberInfo>(liteMembers);
            DataMembers = new ReadOnlyCollection<MemberInfo>(dataMembers);
        }
    }
}