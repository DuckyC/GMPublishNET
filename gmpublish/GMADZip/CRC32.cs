using System;
using System.IO;

namespace gmpublish.GMADZip
{
    /// <summary>
    /// Provides methods to compute CRC32 checksums.
    /// </summary>
    public class CRC32
    {

        private uint _CRC = 0xffffffff;

        public void Reset()
        {
            _CRC = 0xffffffff;
        }

        public void Update(byte[] bytes, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                byte index = (byte)(((_CRC) & 0xff) ^ bytes[i]);
                _CRC = (uint)((_CRC >> 8) ^ table[index]);
            }
        }

        public uint CRC
        {
            get
            {
                return ~_CRC;
            }
        }

        public static uint ComputeChecksum(Stream stream)
        {
            var CRC = new CRC32();
            byte[] buff = new byte[1024];
            while (stream.Length != stream.Position)
            {
                int count = stream.Read(buff, 0, buff.Length);
                CRC.Update(buff, count);
            }
            return CRC.CRC;

        }


        /// <summary>
        /// The table containing calculation polynomials.
        /// </summary>
        static uint[] table;

        /// <summary>
        /// Calculates the CRC32 checksum for the provided byte array.
        /// </summary>
        /// <param name="bytes">The bytes to calculate the checksum for.</param>
        /// <returns>The checksum as an unsigned integer.</returns>
        public static uint ComputeChecksum(byte[] bytes)
        {
            uint crc = 0xffffffff;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                crc = (uint)((crc >> 8) ^ table[index]);
            }
            return ~crc;
        }

        /// <summary>
        /// Calculates the CRC32 checksum for the provided byte array.
        /// </summary>
        /// <param name="bytes">The bytes to calculate the checksum for.</param>
        /// <returns>The checksum as an array of bytes.</returns>
        public static byte[] ComputeChecksumBytes(byte[] bytes)
        {
            return BitConverter.GetBytes(ComputeChecksum(bytes));
        }

        /// <summary>
        /// Sets up the CRC32 generator by calculating the polynomial values.
        /// </summary>
        static CRC32()
        {
            uint poly = 0xedb88320;
            table = new uint[256];
            uint temp = 0;
            for (uint i = 0; i < table.Length; ++i)
            {
                temp = i;
                for (int j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                    {
                        temp = (uint)((temp >> 1) ^ poly);
                    }
                    else
                    {
                        temp >>= 1;
                    }
                }
                table[i] = temp;
            }
        }
    }
}