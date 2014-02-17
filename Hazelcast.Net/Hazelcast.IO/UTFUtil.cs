using System;
using System.Text;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
    internal sealed class UTFUtil
    {
        private const int StringChunkSize = 16*1024;

        /// <exception cref="System.IO.IOException"></exception>
        public static void WriteUTF(IDataOutput output, string str)
        {
            bool isNull = str == null;
            output.WriteBoolean(isNull);
            if (isNull)
            {
                return;
            }
            int length = str.Length;
            output.WriteInt(length);
            int chunkSize = (length/StringChunkSize) + 1;
            for (int i = 0; i < chunkSize; i++)
            {
                int beginIndex = Math.Max(0, i*StringChunkSize - 1);
                WriteShortUTF(output, str.Substring(beginIndex, length));
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        private static void WriteShortUTF(IDataOutput output, string str)
        {
            int stringLen = str.Length;
            int utfLength = 0;
            int c;
            int count = 0;
            for (int i = 0; i < stringLen; i++)
            {
                c = str[i];
                if ((c >= unchecked(0x0001)) && (c <= unchecked(0x007F)))
                {
                    utfLength++;
                }
                else
                {
                    if (c > unchecked(0x07FF))
                    {
                        utfLength += 3;
                    }
                    else
                    {
                        utfLength += 2;
                    }
                }
            }
            if (utfLength > 65535)
            {
                throw new UriFormatException("encoded string too long:" + utfLength + " bytes");
            }
            output.WriteShort(utfLength);
            int i_1;
            var byteArray = new byte[utfLength];
            for (i_1 = 0; i_1 < stringLen; i_1++)
            {
                c = str[i_1];
                if (!((c >= unchecked(0x0001)) && (c <= unchecked(0x007F))))
                {
                    break;
                }
                byteArray[count++] = unchecked((byte) c);
            }
            for (; i_1 < stringLen; i_1++)
            {
                c = str[i_1];
                if ((c >= unchecked(0x0001)) && (c <= unchecked(0x007F)))
                {
                    byteArray[count++] = unchecked((byte) c);
                }
                else
                {
                    if (c > unchecked(0x07FF))
                    {
                        byteArray[count++] = unchecked((byte) (unchecked(0xE0) | ((c >> 12) & unchecked(0x0F))));
                        byteArray[count++] = unchecked((byte) (unchecked(0x80) | ((c >> 6) & unchecked(0x3F))));
                        byteArray[count++] = unchecked((byte) (unchecked(0x80) | ((c) & unchecked(0x3F))));
                    }
                    else
                    {
                        byteArray[count++] = unchecked((byte) (unchecked(0xC0) | ((c >> 6) & unchecked(0x1F))));
                        byteArray[count++] = unchecked((byte) (unchecked(0x80) | ((c) & unchecked(0x3F))));
                    }
                }
            }
            output.Write(byteArray, 0, utfLength);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public static string ReadUTF(IDataInput input)
        {
            bool isNull = input.ReadBoolean();
            if (isNull)
            {
                return null;
            }
            int length = input.ReadInt();
            var result = new StringBuilder(length);
            int chunkSize = length/StringChunkSize + 1;
            while (chunkSize > 0)
            {
                result.Append(ReadShortUTF(input));
                chunkSize--;
            }
            return result.ToString();
        }

        /// <exception cref="System.IO.IOException"></exception>
        private static string ReadShortUTF(IDataInput input)
        {
            int utflen = input.ReadShort();
            byte[] bytearr = null;
            char[] chararr = null;
            bytearr = new byte[utflen];
            chararr = new char[utflen];
            int c;
            int char2;
            int char3;
            int count = 0;
            int chararr_count = 0;
            input.ReadFully(bytearr, 0, utflen);
            while (count < utflen)
            {
                c = bytearr[count] & unchecked(0xff);
                if (c > 127)
                {
                    break;
                }
                count++;
                chararr[chararr_count++] = (char) c;
            }
            while (count < utflen)
            {
                c = bytearr[count] & unchecked(0xff);
                switch (c >> 4)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    {
                        count++;
                        chararr[chararr_count++] = (char) c;
                        break;
                    }

                    case 12:
                    case 13:
                    {
                        count += 2;
                        if (count > utflen)
                        {
                            throw new UriFormatException("malformed input: partial character at end");
                        }
                        char2 = bytearr[count - 1];
                        if ((char2 & unchecked(0xC0)) != unchecked(0x80))
                        {
                            throw new UriFormatException("malformed input around byte " + count);
                        }
                        chararr[chararr_count++] = (char) (((c & unchecked(0x1F)) << 6) | (char2 & unchecked(0x3F)));
                        break;
                    }

                    case 14:
                    {
                        count += 3;
                        if (count > utflen)
                        {
                            throw new UriFormatException("malformed input: partial character at end");
                        }
                        char2 = bytearr[count - 2];
                        char3 = bytearr[count - 1];
                        if (((char2 & unchecked(0xC0)) != unchecked(0x80)) ||
                            ((char3 & unchecked(0xC0)) != unchecked(0x80)))
                        {
                            throw new UriFormatException("malformed input around byte " + (count - 1));
                        }
                        chararr[chararr_count++] =
                            (char)
                                (((c & unchecked(0x0F)) << 12) | ((char2 & unchecked(0x3F)) << 6) |
                                 ((char3 & unchecked(0x3F)) << 0));
                        break;
                    }

                    default:
                    {
                        throw new UriFormatException("malformed input around byte " + count);
                    }
                }
            }
            // The number of chars produced may be less than utflen
            return new string(chararr, 0, chararr_count);
        }
    }
}