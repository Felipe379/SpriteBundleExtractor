using System.Text;

namespace SpriteBundleExtractor
{
    public class ImgRaw
    {
        private const string Magic = "IMG fmt L\0\0\0";
        private const string Compression = "lz4 ";
        private const string DataMagic = "data";

        public string DataName { get; set; }
        public byte[] UnkFill { get; set; }
        public uint UnkData { get; set; }
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public byte[] CompressedData { get; set; }

        public static ImgRaw FromFile(BinaryReader reader)
        {
            var magic = Encoding.ASCII.GetString(reader.ReadBytes(12));
            if (magic != Magic)
                return null;

            var dataName = ReadString(reader);
            var unkFill = reader.ReadBytes(54);
            var unkData = reader.ReadUInt32();

            var compression = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (compression != Compression)
                throw new Exception("Unsupported compression format");

            var width = reader.ReadUInt16();
            var height = reader.ReadUInt16();

            var dataMagic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (dataMagic != DataMagic)
                throw new Exception("Invalid data block");

            var dataLength = reader.ReadUInt32();
            var compressedData = reader.ReadBytes((int)dataLength);

            return new ImgRaw
            {
                DataName = dataName,
                UnkFill = unkFill,
                UnkData = unkData,
                Width = width,
                Height = height,
                CompressedData = compressedData
            };
        }

        private static string ReadString(BinaryReader reader)
        {
            var bytes = new List<byte>();
            
            while (true)
            {
                var b = reader.ReadByte();
                if (b == 0)
                {
                    break;
                }
                bytes.Add(b);
            }

            return Encoding.ASCII.GetString(bytes.ToArray());
        }
    }
}
