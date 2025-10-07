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

namespace Hazelcast.Models
{
    /// <summary>
    /// Represents the version of a cluster member.
    /// </summary>
    public class MemberVersion: IEquatable<MemberVersion>, IComparable<MemberVersion>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberVersion"/> class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        public MemberVersion(byte major, byte minor, byte patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        /// <summary>
        /// Gets the major version number.
        /// </summary>
        public byte Major { get; }

        /// <summary>
        /// Gets the minor version number.
        /// </summary>
        public byte Minor { get; }

        /// <summary>
        /// Gets the patch version number.
        /// </summary>
        public byte Patch { get; }

        /// <summary>
        /// Returns a string that represents current member version.
        /// </summary>
        /// <param name="ignorePatchVersion">Whether to skip or include path version to the string.</param>
        public string ToString(bool ignorePatchVersion)
        {
            return ignorePatchVersion
                ? $"{Major}.{Minor}"
                : $"{Major}.{Minor}.{Patch}";
        }

        /// <summary>
        /// Returns a string that represents current member version.
        /// </summary>
        public override string ToString() => ToString(ignorePatchVersion: false);

        #region Equality members

        /// <summary>
        /// Checks if this member version is equal to <paramref name="other"/>.
        /// </summary>
        /// <param name="other">Other member version to compare with.</param>
        /// <param name="ignorePatchVersion">Whether to ignore Patch number differences.</param>
        /// <returns><c>true</c> if versions are equal, <c>false</c> otherwise.</returns>
        public bool Equals(MemberVersion other, bool ignorePatchVersion)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Major == other.Major && Minor == other.Minor &&
                (ignorePatchVersion || Patch == other.Patch);
        }

        /// <summary>
        /// Checks if this member version is equal to <paramref name="other"/>.
        /// </summary>
        /// <param name="other">Other member version to compare with.</param>
        /// <returns><c>true</c> if versions are equal, <c>false</c> otherwise.</returns>
        public bool Equals(MemberVersion other) => Equals(other, ignorePatchVersion: false);

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MemberVersion)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Patch);
        }

        /// <summary>
        /// Checks if this member version is equal to <paramref name="right"/>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(MemberVersion left, MemberVersion right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Checks if this member version is not equal to <paramref name="right"/>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(MemberVersion left, MemberVersion right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region Relational members

        /// <summary>
        /// Compares this member version to <paramref name="other"/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(MemberVersion other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            var majorComparison = Major.CompareTo(other.Major);
            if (majorComparison != 0) return majorComparison;

            var minorComparison = Minor.CompareTo(other.Minor);
            if (minorComparison != 0) return minorComparison;

            return Patch.CompareTo(other.Patch);
        }

        /// <summary>
        /// Checks if this member version is less than <paramref name="right"/>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <(MemberVersion left, MemberVersion right)
        {
            return Comparer<MemberVersion>.Default.Compare(left, right) < 0;
        }

        /// <summary>
        /// Checks if this member version is greater than <paramref name="right"/>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >(MemberVersion left, MemberVersion right)
        {
            return Comparer<MemberVersion>.Default.Compare(left, right) > 0;
        }

        /// <summary>
        /// Checks if this member version is less than or equal to <paramref name="right"/>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <=(MemberVersion left, MemberVersion right)
        {
            return Comparer<MemberVersion>.Default.Compare(left, right) <= 0;
        }

        /// <summary>
        /// Checks if this member version is greater than or equal to <paramref name="right"/>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >=(MemberVersion left, MemberVersion right)
        {
            return Comparer<MemberVersion>.Default.Compare(left, right) >= 0;
        }

        #endregion

    }
}
