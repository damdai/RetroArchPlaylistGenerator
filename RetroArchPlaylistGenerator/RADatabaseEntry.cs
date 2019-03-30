using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace RetroArchPlaylistGenerator
{
    public class RADatabaseEntry
    {
        private const byte EntryStartByte = 0x80;

        public byte[] CRC;
        public string Description;
        public string Franchise;
        public byte[] MD5;
        public string Name;
        public string RomName;
        public byte[] SHA1;
        public ulong Size;

        public RADatabaseEntry(Stream stream)
        {
            //name
            var entryStartByte = stream.ReadByte();

            if (entryStartByte >= 0x90)
                throw new Exception($"Unexpected byte at offset {stream.Position}: {entryStartByte}");

            while (stream.Peek() >= 0x90 && stream.Peek() != 0xc0)
            {
                if (stream.Peek() == 0xdf)
                {
                    stream.ReadBytes(4);
                    return;
                }

                var fieldLength = int.Parse($"0{stream.ReadByte().ToString("X")[1]}", NumberStyles.HexNumber);
                var fieldName = Encoding.Default.GetString(stream.ReadBytes(fieldLength));
                var value = ParseValue(stream);

                switch (fieldName)
                {
                    case "name":
                        Name = value;
                        break;
                    case "description":
                        Description = value;
                        break;
                    case "rom_name":
                        RomName = value;
                        break;
                    case "size":
                        Size = value;
                        break;
                    case "franchise":
                        Franchise = value;
                        break;
                    case "crc":
                        CRC = value;
                        break;
                    case "md5":
                        MD5 = value;
                        break;
                    case "sha1":
                        SHA1 = value;
                        break;
                    //case "releasemonth":
                    //case "releaseyear":
                    //case "developer":
                    //case "publisher":
                    //case "serial":
                    //case "users":
                    //case "edge_rating":
                    //case "edge_issue":
                    //case "esrb_rating":
                    //case "origin":
                    //case "genre":
                    //case "tgdb_rating":
                    //case "coop":
                    //    break;
                    //default:
                    //    throw new Exception($"Unhandled field: {fieldName}");
                }
            }
        }

        private static dynamic ParseValue(Stream stream)
        {
            var typeIdentifier = stream.ReadByte();

            if (typeIdentifier < 0x80) //MPF_FIXMAP
                throw new Exception($"Unhandled value type: {typeIdentifier}");
            else if (typeIdentifier < 0x90) //MPF_FIXARRAY
                throw new Exception($"Unhandled value type: {typeIdentifier}");
            else if (typeIdentifier < 0xa0) //MPF_FIXSTR
                throw new Exception($"Unhandled value type: {typeIdentifier}");
            else if (typeIdentifier < 0xc0) //MPF_NIL
            {
                var len = typeIdentifier - 0xa0;
                return Encoding.Default.GetString(stream.ReadBytes(len));
            }

            switch (typeIdentifier)
            {
                case 0xD9:
                    return Encoding.Default.GetString(stream.ReadBytes(stream.ReadByte()));
                case 0xda:
                    return Encoding.Default.GetString(stream.ReadBytes(stream.ReadUInt16(true)));
                case 0xCE:
                    return stream.ReadUInt32(true);
                case 0xC4:
                    return stream.ReadBytes(stream.ReadByte());
                case 0xCC:
                    return stream.ReadUInt8();
                case 0xCD:
                    return stream.ReadUInt16(true);
                case 0xcf:
                    return stream.ReadUInt64(true);
                default:
                    throw new Exception($"Unhandled value type: {typeIdentifier}");
            }
        }
    }
}