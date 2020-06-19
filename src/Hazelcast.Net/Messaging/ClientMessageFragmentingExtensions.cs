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
using Hazelcast.Core;
using Hazelcast.Exceptions;

namespace Hazelcast.Messaging
{
    /// <summary>
    /// Provides extension methods to the <see cref="ClientMessage"/> class for fragmenting messages.
    /// </summary>
    /// <remarks>
    /// <para>When a message is fragmented, it is sent</para>
    /// <para>For instance, if the original message is composed of the following message frames
    /// (MF): MF0, MF1, MF2, MF3, MF4, MF5(final) it could be fragmented into FF0(begin), MF0,
    /// MF1(final) then FF1, MF2, MF3 then FF2(end), MF4, MF5(final) with the fragmentation
    /// frames (FF) containing the fragment identifier.</para>
    /// </remarks>
    public static class ClientMessageFragmentingExtensions
    {
        // we can use one single static sequence of fragment identifiers
        // Java uses a static CallIdSequenceWithoutBackPressure
        private static readonly ISequence<long> FragmentIdSequence = new Int64Sequence();

        /// <summary>
        /// Creates a new fragment.
        /// </summary>
        /// <param name="frame">The first frame of the fragment.</param>
        /// <returns>The new fragment.</returns>
        private static ClientMessage NewFragment(Frame frame)
        {
            // TODO control allocations?
            // here we are allocating a small byte array which could come from an ArrayPool
            // but then, it would need to be returned to that same ArrayPool, which means
            // that we would need to flag those frames, and make sure we return their bytes
            // at the right time - which would be... when? should we make messages and
            // frames disposable then to avoid memory leaks?

            var f = new Frame(new byte[FrameFields.SizeOf.FragmentId]);
            f.WriteFragmentId(FragmentIdSequence.GetNext());
            return new ClientMessage(f).Append(frame.ShallowClone());
        }

        /// <summary>
        /// Fragments a message.
        /// </summary>
        /// <param name="message">The message to fragment.</param>
        /// <param name="maxSize">The maximum size of each fragment.</param>
        /// <returns>An enumeration of fragments.</returns>
        /// <remarks>
        /// <para>Some fragments may be larger than <paramref name="maxSize"/> if the original
        /// message contains frames large than <paramref name="maxSize"/>, which cannot be
        /// fragmented.</para>
        /// </remarks>
        public static IEnumerable<ClientMessage> Fragment(this ClientMessage message, int maxSize)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var size = 0;
            for (var frame = message.FirstFrame; frame != null; frame = frame.Next)
                size += frame.Length;

            // if this entire message is small enough,
            // return it without splitting it at all
            if (size < maxSize)
            {
                yield return message;
                yield break;
            }

            ClientMessage ready = null;
            ClientMessage current = null;
            size = 0;

            // whether when creating a new fragment, or appending to an existing fragment,
            // make sure to shallow-clone the frames, as their flags might be modified, ie
            // the last frame of each segment will be marked as final

            for (var frame = message.FirstFrame; frame != null; frame = frame.Next)
            {
                if (frame.Length > maxSize)
                {
                    // if this frame enough is too big, yield the current fragment if any,
                    // then yield a fragment containing only this frame, and continue from
                    // zero

                    if (ready != null)
                        yield return ready;
                    ready = current;

                    if (ready != null)
                        yield return ready;
                    ready = NewFragment(frame);

                    current = null;
                    size = 0;
                }
                else
                {
                    // otherwise, try to accumulate this frame

                    size += frame.Length;

                    if (size <= maxSize)
                    {
                        // if total size is still small enough, create a current fragment
                        // if necessary, and append the frame, and continue accumulating

                        if (current == null)
                            current = NewFragment(frame);
                        else
                            current.Append(frame.ShallowClone());
                    }
                    else
                    {
                        // otherwise, we have to have a current fragment (but just make sure),
                        // so yield it, and then continue with a new fragment

                        if (ready != null)
                            yield return ready;
                        ready = current ?? throw new HazelcastException("panic");

                        current = NewFragment(frame);
                        size = frame.Length;
                    }
                }
            }

            var last = current ?? ready;
            if (last != null) last.Flags |= ClientMessageFlags.EndFragment;

            if (ready != null) yield return ready;
            if (current != null) yield return current;
        }
    }
}
