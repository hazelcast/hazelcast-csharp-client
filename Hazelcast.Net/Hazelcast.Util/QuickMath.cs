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

using System;

namespace Hazelcast.Util
{
	/// <summary>
	/// The class
	/// <code>QuickMath</code>
	/// contains methods to perform optimized mathematical operations.
	/// Methods are allowed to put additional constraints on the range of input values if required for efficiency.
	/// Methods are <b>not</b> required to perform validation of input arguments, but they have to indicate the constraints
	/// in theirs contract.
	/// </summary>
	internal sealed class QuickMath
	{
	    /// <summary>Return true if input argument is power of two.</summary>
		/// <remarks>
		/// Return true if input argument is power of two.
		/// Input has to be a a positive integer.
		/// Result is undefined for zero or negative integers.
		/// </remarks>
		/// <param name="x"></param>
		/// <returns><code>true</code> if <code>x</code> is power of two</returns>
		public static bool IsPowerOfTwo(long x)
		{
			return (x & (x - 1)) == 0;
		}

		public static int NextPowerOfTwo(int value)
		{
			if (!IsPowerOfTwo(value))
			{
				value--;
				value |= value >> 1;
				value |= value >> 2;
				value |= value >> 4;
				value |= value >> 8;
				value |= value >> 16;
				value++;
			}
			return value;
		}
	}
}
