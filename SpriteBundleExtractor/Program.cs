using LZ4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace SpriteBundleExtractor
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Write down the path to the file or a folder:");
            string path = Console.ReadLine();

            var attr = File.GetAttributes(path);
            List<string> files;

            if (attr.HasFlag(FileAttributes.Directory))
            {
                files = Directory.GetFiles(path, "*.sprite", SearchOption.TopDirectoryOnly).ToList();
            }
            else
            {
                files = new List<string>() { path };
            }

            string fileName;
            string directory;

            foreach (var filePath in files)
            {
                fileName = Path.GetFileNameWithoutExtension(filePath);
                directory = $"{Directory.GetCurrentDirectory()}\\{fileName}";

                if (!Directory.Exists($"{directory}"))
                {
                    Directory.CreateDirectory(directory);
                }

                DecodeImage(filePath, fileName);
            }

            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        private static void DecodeImage(string filePath, string directory)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(stream);

            long fileLength = reader.BaseStream.Length;
            long position = 0;
            ImgRaw imageRaw;
            byte[] uncompressedData;
            Image image;
            string imageFile;

            while (position < fileLength)
            {
                imageRaw = ImgRaw.FromFile(reader);

                if (imageRaw != null)
                {
                    uncompressedData = LZ4Codec.Decode(imageRaw.CompressedData, 0, imageRaw.CompressedData.Length, imageRaw.Width * imageRaw.Height * 4);

                    image = Image.LoadPixelData<Rgba32>(uncompressedData, imageRaw.Width, imageRaw.Height);

                    imageFile = $"{imageRaw.DataName}_{position:x8}.png";
                    image.Save($"{directory}\\{imageFile}", new PngEncoder());

                    Console.WriteLine($"Saved image in: {directory}\\{imageFile}");
                }
                else
                {
                    reader.BaseStream.Position = position;
                    position += 1;
                }
            }

        }
    }
}
