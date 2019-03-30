using System;
using System.IO;
using System.Linq;
using System.Text;

namespace RetroArchPlaylistGenerator
{
    public static class StreamExtensions
    {
        public static short ReadInt16(this Stream stream, bool bigEndian = false)
        {
            var buf = stream.ReadBytes(2);
            if (bigEndian) buf = buf.Reverse().ToArray();
            return BitConverter.ToInt16(buf, 0);
        }

        public static int ReadInt32(this Stream stream, bool bigEndian = false)
        {
            var buf = stream.ReadBytes(4);
            if (bigEndian) buf = buf.Reverse().ToArray();
            return BitConverter.ToInt32(buf, 0);
        }

        public static long ReadInt64(this Stream stream, bool bigEndian = false)
        {
            var buf = stream.ReadBytes(8);
            if (bigEndian) buf = buf.Reverse().ToArray();
            return BitConverter.ToInt64(buf, 0);
        }

        public static uint ReadUInt8(this Stream stream)
        {
            return (uint)stream.ReadByte();
        }

        public static ushort ReadUInt16(this Stream stream, bool bigEndian = false)
        {
            var buf = stream.ReadBytes(2);
            if (bigEndian) buf = buf.Reverse().ToArray();
            return BitConverter.ToUInt16(buf, 0);
        }

        public static uint ReadUInt32(this Stream stream, bool bigEndian = false)
        {
            var buf = stream.ReadBytes(4);
            if (bigEndian) buf = buf.Reverse().ToArray();
            return BitConverter.ToUInt32(buf, 0);
        }

        public static ulong ReadUInt64(this Stream stream, bool bigEndian = false)
        {
            var buf = stream.ReadBytes(8);
            if (bigEndian) buf = buf.Reverse().ToArray();
            return BitConverter.ToUInt64(buf, 0);
        }

        public static double ReadDouble(this Stream stream, bool bigEndian = false)
        {
            var buf = stream.ReadBytes(8);
            if (bigEndian) buf = buf.Reverse().ToArray();
            return BitConverter.ToDouble(buf, 0);
        }

        public static float ReadSingle(this Stream stream, bool bigEndian = false)
        {
            var buf = stream.ReadBytes(4);
            if (bigEndian) buf = buf.Reverse().ToArray();
            return BitConverter.ToSingle(buf, 0);
        }

        public static byte[] ReadBytes(this Stream stream, long length)
        {
            var buf = new byte[length];
            stream.Read(buf, 0, buf.Length);
            return buf;
        }

        public static string ReadString(this Stream stream, int length, Encoding encoding)
        {
            var buf = new byte[length];
            stream.Read(buf, 0, buf.Length);
            return encoding.GetString(buf, 0, buf.Length).TrimEnd('\0');
        }

        public static bool ReadBoolean(this Stream stream)
        {
            var buf = stream.ReadBytes(1);
            return BitConverter.ToBoolean(buf, 0);
        }

        public static int Peek(this Stream stream)
        {
            var b = stream.ReadByte();
            stream.Seek(-1, SeekOrigin.Current);
            return b;
        }
    }
}