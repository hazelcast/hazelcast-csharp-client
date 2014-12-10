using Hazelcast.Net.Ext;

namespace Hazelcast.Util
{
	internal sealed class HashUtil
	{
		private static readonly bool IsLittleEndian = ByteOrder.LittleEndian == ByteOrder
			.NativeOrder();

		private const int DEFAULT_MURMUR_SEED = 0x01000193;

		public static int MurmurHash3_x86_32(byte[] data, int offset, int len)
		{
			return MurmurHash3_x86_32(data, offset, len, DEFAULT_MURMUR_SEED);
		}

		/// <summary>Returns the MurmurHash3_x86_32 hash.</summary>
		public static int MurmurHash3_x86_32(byte[] data, int offset, int len, int seed)
		{
			int c1 = unchecked((int)(0xcc9e2d51));
			int c2 = unchecked((int)(0x1b873593));
			int h1 = seed;
			int roundedEnd = offset + (len & unchecked((int)(0xfffffffc)));
			// round down to 4 byte block
			for (int i = offset; i < roundedEnd; i += 4)
			{
				// little endian load order
				int k1 = (data[i] & unchecked((int)(0xff))) | 
                    ((data[i + 1] & unchecked((int)(0xff))) << 8) | 
                    ((data[i + 2] & unchecked((int)(0xff))) << 16) | 
                    (data[i + 3] << 24);
				k1 *= c1;
				k1 = (k1 << 15) | ((int)(((uint)k1) >> 17));
				// ROTL32(k1,15);
				k1 *= c2;
				h1 ^= k1;
				h1 = (h1 << 13) | ((int)(((uint)h1) >> 19));
				// ROTL32(h1,13);
				h1 = h1 * 5 + unchecked((int)(0xe6546b64));
			}
			// tail
			int k1_1 = 0;
			switch (len & unchecked((int)(0x03)))
			{
				case 3:
				{
					k1_1 = (data[roundedEnd + 2] & unchecked((int)(0xff))) << 16;
					goto case 2;
				}

				case 2:
				{
					// fallthrough
					k1_1 |= (data[roundedEnd + 1] & unchecked((int)(0xff))) << 8;
					goto case 1;
				}

				case 1:
				{
					// fallthrough
					k1_1 |= data[roundedEnd] & unchecked((int)(0xff));
					k1_1 *= c1;
					k1_1 = (k1_1 << 15) | ((int)(((uint)k1_1) >> 17));
					// ROTL32(k1,15);
					k1_1 *= c2;
					h1 ^= k1_1;
				    break;
				}
			}
			// finalization
			h1 ^= len;
			h1 = MurmurHash3_fmix(h1);
			return h1;
		}

		public static long MurmurHash3_x64_64(byte[] data, int offset, int len)
		{
			return MurmurHash3_x64_64(data, offset, len, DEFAULT_MURMUR_SEED);
		}

		public static long MurmurHash3_x64_64(byte[] data, int offset, int len, int seed)
		{
			long h1 = unchecked((long)(0x9368e53c2f6af274L)) ^ seed;
			long h2 = unchecked((long)(0x586dcd208f7cd3fdL)) ^ seed;
			long c1 = unchecked((long)(0x87c37b91114253d5L));
			long c2 = unchecked((long)(0x4cf5ad432745937fL));
			long k1 = 0;
			long k2 = 0;
			for (int i = 0; i < len / 16; i++)
			{
				k1 = MurmurHash3_getBlock(data, (i * 2 * 8) + offset);
				k2 = MurmurHash3_getBlock(data, ((i * 2 + 1) * 8) + offset);
				// bmix(state);
				k1 *= c1;
				k1 = (k1 << 23) | ((long)(((ulong)k1) >> 64 - 23));
				k1 *= c2;
				h1 ^= k1;
				h1 += h2;
				h2 = (h2 << 41) | ((long)(((ulong)h2) >> 64 - 41));
				k2 *= c2;
				k2 = (k2 << 23) | ((long)(((ulong)k2) >> 64 - 23));
				k2 *= c1;
				h2 ^= k2;
				h2 += h1;
				h1 = h1 * 3 + unchecked((int)(0x52dce729));
				h2 = h2 * 3 + unchecked((int)(0x38495ab5));
				c1 = c1 * 5 + unchecked((int)(0x7b7d159c));
				c2 = c2 * 5 + unchecked((int)(0x6bce6396));
			}
			k1 = 0;
			k2 = 0;
			int tail = (((int)(((uint)len) >> 4)) << 4) + offset;
			switch (len & 15)
			{
				case 15:
				{
					k2 ^= (long)data[tail + 14] << 48;
					goto case 14;
				}

				case 14:
				{
					k2 ^= (long)data[tail + 13] << 40;
					goto case 13;
				}

				case 13:
				{
					k2 ^= (long)data[tail + 12] << 32;
					goto case 12;
				}

				case 12:
				{
					k2 ^= (long)data[tail + 11] << 24;
					goto case 11;
				}

				case 11:
				{
					k2 ^= (long)data[tail + 10] << 16;
					goto case 10;
				}

				case 10:
				{
					k2 ^= (long)data[tail + 9] << 8;
					goto case 9;
				}

				case 9:
				{
					k2 ^= data[tail + 8];
					goto case 8;
				}

				case 8:
				{
					k1 ^= (long)data[tail + 7] << 56;
					goto case 7;
				}

				case 7:
				{
					k1 ^= (long)data[tail + 6] << 48;
					goto case 6;
				}

				case 6:
				{
					k1 ^= (long)data[tail + 5] << 40;
					goto case 5;
				}

				case 5:
				{
					k1 ^= (long)data[tail + 4] << 32;
					goto case 4;
				}

				case 4:
				{
					k1 ^= (long)data[tail + 3] << 24;
					goto case 3;
				}

				case 3:
				{
					k1 ^= (long)data[tail + 2] << 16;
					goto case 2;
				}

				case 2:
				{
					k1 ^= (long)data[tail + 1] << 8;
					goto case 1;
				}

				case 1:
				{
					k1 ^= data[tail];
					// bmix();
					k1 *= c1;
					k1 = (k1 << 23) | ((long)(((ulong)k1) >> 64 - 23));
					k1 *= c2;
					h1 ^= k1;
					h1 += h2;
					h2 = (h2 << 41) | ((long)(((ulong)h2) >> 64 - 41));
					k2 *= c2;
					k2 = (k2 << 23) | ((long)(((ulong)k2) >> 64 - 23));
					k2 *= c1;
					h2 ^= k2;
					h2 += h1;
					h1 = h1 * 3 + unchecked((int)(0x52dce729));
					h2 = h2 * 3 + unchecked((int)(0x38495ab5));
				    break;
				}
			}
			//                c1 = c1 * 5 + 0x7b7d159c;   // unused, used only for 128-bit version
			//                c2 = c2 * 5 + 0x6bce6396;   // unused, used only for 128-bit version
			h2 ^= len;
			h1 += h2;
			h2 += h1;
			h1 = MurmurHash3_fmix(h1);
			h2 = MurmurHash3_fmix(h2);
			h1 += h2;
			//        h2 += h1; // unused, used only for 128-bit version
			return h1;
		}

		private static long MurmurHash3_getBlock(byte[] key, int i)
		{
			return ((key[i] & unchecked((long)(0x00000000000000FFL)))) | ((key[i + 1] & unchecked(
				(long)(0x00000000000000FFL))) << 8) | ((key[i + 2] & unchecked((long)(0x00000000000000FFL
				))) << 16) | ((key[i + 3] & unchecked((long)(0x00000000000000FFL))) << 24) | ((key
				[i + 4] & unchecked((long)(0x00000000000000FFL))) << 32) | ((key[i + 5] & unchecked(
				(long)(0x00000000000000FFL))) << 40) | ((key[i + 6] & unchecked((long)(0x00000000000000FFL
				))) << 48) | ((key[i + 7] & unchecked((long)(0x00000000000000FFL))) << 56);
		}

		public static int MurmurHash3_fmix(int k)
		{
			k ^= (int)(((uint)k) >> 16);
			k *= unchecked((int)(0x85ebca6b));
			k ^= (int)(((uint)k) >> 13);
			k *= unchecked((int)(0xc2b2ae35));
			k ^= (int)(((uint)k) >> 16);
			return k;
		}

		public static long MurmurHash3_fmix(long k)
		{
			k ^= (long)(((ulong)k) >> 33);
			k *= unchecked((long)(0xff51afd7ed558ccdL));
			k ^= (long)(((ulong)k) >> 33);
			k *= unchecked((long)(0xc4ceb9fe1a85ec53L));
			k ^= (long)(((ulong)k) >> 33);
			return k;
		}

	}
}
