using System.Collections.Generic;
using Hazelcast.Messaging;

namespace Hazelcast.Protocol
{
    internal static class FrameEnumeratorExtensions
    {
        /// <summary>
        /// Determines whether the frame enumerator has more non-end frames.
        /// </summary>
        /// <param name="enumerator">The enumerator.</param>
        /// <returns><c>true</c> is the enumerator can move next to a non-end frame; otherwise <c>false</c>.</returns>
        public static bool NextIsNotTheEnd(this IEnumerator<Frame> enumerator)
        {
            var next = enumerator.Current?.Next;
            return next != null && !next.IsEndStruct;
        }
    }
}
