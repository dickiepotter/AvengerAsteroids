using System;
using System.IO;
using System.IO.Compression;

namespace Asteroids.Core
{
    /// <summary>Tiny dependency-free PNG writer (8-bit RGBA) for headless capture.</summary>
    public static class Png
    {
        public static void Save(string path, int width, int height, byte[] rgba)
        {
            using var fs = File.Create(path);
            Span<byte> signature = stackalloc byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
            fs.Write(signature);

            var ihdr = new byte[13];
            WriteBE(ihdr, 0, (uint)width);
            WriteBE(ihdr, 4, (uint)height);
            ihdr[8] = 8;
            ihdr[9] = 6;
            WriteChunk(fs, "IHDR", ihdr);

            var raw = new byte[height * (width * 4 + 1)];
            int p = 0;
            for (int y = 0; y < height; y++)
            {
                raw[p++] = 0;
                Array.Copy(rgba, y * width * 4, raw, p, width * 4);
                p += width * 4;
            }
            WriteChunk(fs, "IDAT", ZlibCompress(raw));
            WriteChunk(fs, "IEND", Array.Empty<byte>());
        }

        private static byte[] ZlibCompress(byte[] data)
        {
            using var ms = new MemoryStream();
            ms.WriteByte(0x78);
            ms.WriteByte(0x9C);
            using (var deflate = new DeflateStream(ms, CompressionLevel.Optimal, leaveOpen: true))
                deflate.Write(data, 0, data.Length);
            WriteBE(ms, Adler32(data));
            return ms.ToArray();
        }

        private static void WriteChunk(Stream s, string type, byte[] data)
        {
            Span<byte> len = stackalloc byte[4];
            WriteBE(len, 0, (uint)data.Length);
            s.Write(len);
            var typeBytes = new byte[4];
            for (int i = 0; i < 4; i++) typeBytes[i] = (byte)type[i];
            s.Write(typeBytes);
            s.Write(data);
            Span<byte> crc = stackalloc byte[4];
            WriteBE(crc, 0, Crc32(typeBytes, data));
            s.Write(crc);
        }

        private static void WriteBE(Span<byte> dst, int offset, uint value)
        {
            dst[offset] = (byte)(value >> 24);
            dst[offset + 1] = (byte)(value >> 16);
            dst[offset + 2] = (byte)(value >> 8);
            dst[offset + 3] = (byte)value;
        }

        private static void WriteBE(Stream s, uint value)
        {
            s.WriteByte((byte)(value >> 24));
            s.WriteByte((byte)(value >> 16));
            s.WriteByte((byte)(value >> 8));
            s.WriteByte((byte)value);
        }

        private static uint Adler32(byte[] data)
        {
            const uint mod = 65521;
            uint a = 1, b = 0;
            foreach (var d in data) { a = (a + d) % mod; b = (b + a) % mod; }
            return (b << 16) | a;
        }

        private static readonly uint[] CrcTable = BuildCrcTable();

        private static uint[] BuildCrcTable()
        {
            var t = new uint[256];
            for (uint n = 0; n < 256; n++)
            {
                uint c = n;
                for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
                t[n] = c;
            }
            return t;
        }

        private static uint Crc32(byte[] type, byte[] data)
        {
            uint c = 0xFFFFFFFF;
            foreach (var b in type) c = CrcTable[(c ^ b) & 0xFF] ^ (c >> 8);
            foreach (var b in data) c = CrcTable[(c ^ b) & 0xFF] ^ (c >> 8);
            return c ^ 0xFFFFFFFF;
        }
    }
}
