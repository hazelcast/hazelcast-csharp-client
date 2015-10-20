/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

﻿using System;
using System.Linq;
using System.Threading;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Model
{


    public class Item : IPortable
    {
        private Header _header;
        private int[] _enabled;
        private int[] _disabled;

        internal Item()
        {
        }

        public Item(Header header, int[] enabled, int[] disabled)
        {
            _header = header;
            _enabled = enabled;
            _disabled = disabled;
        }

        public Header Header
        {
            get { return _header; }
        }

        #region IPortable Implementation

        int IPortable.GetClassId()
        {
            return (int)ClassIds.Item;
        }

        int IPortable.GetFactoryId()
        {
            return (int)ClassIds.Factory;
        }

        void IPortable.ReadPortable(IPortableReader reader)
        {
            _header = reader.ReadPortable<Header>("header");
            _enabled = reader.ReadIntArray("enabled");
            _disabled = reader.ReadIntArray("disabled");
        }

        void IPortable.WritePortable(IPortableWriter writer)
        {
            writer.WritePortable("header", _header);
            writer.WriteIntArray("enabled", _enabled);
            writer.WriteIntArray("disabled", _disabled);
        }

        #endregion

        protected bool Equals(Item other)
        {
            return _header.Equals(other._header) && _enabled.SequenceEqual(other._enabled) && _disabled.SequenceEqual(other._disabled);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Item) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = _header.GetHashCode();
                hashCode = (hashCode*397) ^ (_enabled != null ? _enabled.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (_disabled != null ? _disabled.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class ItemGenerator
    {
        private const int MaxGroups = 20;
        private const int GroupNumber = 200;

        public static Item GenerateItem(long id)
        {
            var header = new Header(id, new Handle(true));

            var allowedGroups = new int[RandomGenerator.Instance.Next(MaxGroups + 1)];
            for (int i = 0; i < allowedGroups.Length; i++)
                allowedGroups[i] = RandomGenerator.Instance.Next(GroupNumber);

            var deniedUsers = new int[RandomGenerator.Instance.Next(MaxGroups + 1)];
            for (int i = 0; i < deniedUsers.Length; i++)
                deniedUsers[i] = RandomGenerator.Instance.Next(GroupNumber);
            return new Item(header, allowedGroups, deniedUsers);
        }
    }
    public struct Header : IPortable
    {
        private long _id;
        private Handle _handle;

        public Header(long id, Handle handle)
        {
            _id = id;
            _handle = handle;
        }

        public long Id { get { return _id; } }

        public Handle Handle { get { return _handle; } }

        #region IPortable Implementation

        int IPortable.GetClassId()
        {
            return (int)ClassIds.Header;
        }

        int IPortable.GetFactoryId()
        {
            return (int)ClassIds.Factory;
        }

        void IPortable.ReadPortable(IPortableReader reader)
        {
            _id = reader.ReadLong("id");
            _handle = reader.ReadPortable<Handle>("handle");
        }

        void IPortable.WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("id", _id);
            writer.WritePortable("handle", _handle);
        }

        public bool Equals(Header other)
        {
            return _id == other._id && _handle.Equals(other._handle);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Header && Equals((Header) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_id.GetHashCode()*397) ^ _handle.GetHashCode();
            }
        }

        #endregion
    }

    public struct Handle : IPortable
    {
        private long _handle;

        public Handle(bool isActive)
        {
            _handle = isActive ? 1L : 0L;
        }

        public bool IsActive { get { return _handle % 2L == 1L; } }

        #region IPortable Implementation

        int IPortable.GetClassId()
        {
            return (int)ClassIds.Handle;
        }

        int IPortable.GetFactoryId()
        {
            return (int)ClassIds.Factory;
        }

        void IPortable.ReadPortable(IPortableReader reader)
        {
            _handle = reader.ReadLong("handle");
        }

        void IPortable.WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("handle", _handle);
        }

        #endregion

        public bool Equals(Handle other)
        {
            return _handle == other._handle;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Handle && Equals((Handle) obj);
        }

        public override int GetHashCode()
        {
            return _handle.GetHashCode();
        }
    }

    internal enum ClassIds
    {
        Factory = 1,
        Handle = 2,
        Header = 3,
        Item = 4
    }

    internal static class RandomGenerator
    {
        private static readonly Random MainRandom = new Random();

        private static readonly ThreadLocal<Random> LocalRandom = new ThreadLocal<Random>(() =>
        {
            lock (MainRandom)
            {
                return new Random(MainRandom.Next());
            }
        });

        public static Random Instance
        {
            get
            {
                return LocalRandom.Value;
            }
        }
    }

    public class PortableFactory : IPortableFactory
    {
        public IPortable Create(int classId)
        {
            switch ((ClassIds)classId)
            {
                case ClassIds.Handle:
                    return new Handle();
                case ClassIds.Header:
                    return new Header();
                case ClassIds.Item:
                    return new Item();
                default:
                    return null;
            }
        }
    }
}