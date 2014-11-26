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
		public QuickMath()
		{
		}

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

		/// <summary>Computes the remainder of the division of <code>a</code> by <code>b</code>.
		/// 	</summary>
		/// <remarks>
		/// Computes the remainder of the division of <code>a</code> by <code>b</code>.
		/// <code>a</code> has to be non-negative integer and <code>b</code> has to be power of two
		/// otherwise the result is undefined.
		/// </remarks>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>remainder of the division of a by b.</returns>
		public static int ModPowerOfTwo(int a, int b)
		{
			return a & (b - 1);
		}

		/// <summary>Computes the remainder of the division of <code>a</code> by <code>b</code>.
		/// 	</summary>
		/// <remarks>
		/// Computes the remainder of the division of <code>a</code> by <code>b</code>.
		/// <code>a</code> has to be non-negative integer and <code>b</code> has to be power of two
		/// otherwise the result is undefined.
		/// </remarks>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>remainder of the division of a by b.</returns>
		public static long ModPowerOfTwo(long a, int b)
		{
			return a & (b - 1);
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

		public static long NextPowerOfTwo(long value)
		{
			if (!IsPowerOfTwo(value))
			{
				value--;
				value |= value >> 1;
				value |= value >> 2;
				value |= value >> 4;
				value |= value >> 8;
				value |= value >> 16;
				value |= value >> 32;
				value++;
			}
			return value;
		}

        //public static int Log2(int value)
        //{
        //    return 31 - NumberOfLeadingZeros(value);
        //}

        //public static int Log2(long value)
        //{
        //    return 63 - long.NumberOfLeadingZeros(value);
        //}

		public static int DivideByAndCeilToInt(double d, int k)
		{
			return (int)Math.Ceiling(d / k);
		}

		public static long DivideByAndCeilToLong(double d, int k)
		{
            return (long)Math.Ceiling(d / k);
		}

		public static int DivideByAndRoundToInt(double d, int k)
		{
			return (int)Math.Round(d / k);
		}

		public static long DivideByAndRoundToLong(double d, int k)
		{
            return (long)Math.Round(d / k);
		}

		public static int Normalize(int value, int factor)
		{
			return DivideByAndCeilToInt(value, factor) * factor;
		}

		public static long Normalize(long value, int factor)
		{
			return DivideByAndCeilToLong(value, factor) * factor;
		}

            
        //public static uint NumberOfLeadingZeros(int i)
        //{
        //    var j = (uint) i;
        //    // HD, Figure 5-6
        //    if (j == 0) return 32;
        //    uint n = 1;
        //    if (j >> 16 == 0) { n += 16; j <<= 16; }
        //    if (j >> 24 == 0) { n +=  8; j <<=  8; }
        //    if (j >> 28 == 0) { n +=  4; j <<=  4; }
        //    if (j >> 30 == 0) { n +=  2; j <<=  2; }
        //    n -= j >> 31;
        //    return n;
        //}

	}
}
