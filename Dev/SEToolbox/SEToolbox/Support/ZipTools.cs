namespace SEToolbox.Support
{
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;

    public static class ZipTools
    {
        public static void MakeClearDirectory(string folder)
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);
        }

        public static void GZipUncompress(string sourceFilename, string destinationFilename)
        {
            // Low memory, fast extract.
            using (var compressedByteStream = new FileStream(sourceFilename, FileMode.Open))
            {
                if (File.Exists(destinationFilename))
                    File.Delete(destinationFilename);

                using (var outStream = new FileStream(destinationFilename, FileMode.CreateNew))
                {
                    // GZipStream requires using. Do not optimize the stream.
                    using (var zip = new GZipStream(compressedByteStream, CompressionMode.Decompress))
                    {
                        zip.CopyTo(outStream);
                        Debug.WriteLine("Decompressed from {0:#,###0} bytes to {1:#,###0} bytes.", compressedByteStream.Length, outStream.Length);
                    }
                }
            }
        }

        /// <summary>
        /// Reads only the specified number of bytes to an array.
        /// </summary>
        /// <param name="sourceFilename"></param>
        /// <param name="numberBytes"></param>
        /// <returns></returns>
        public static byte[] GZipUncompress(string sourceFilename, int numberBytes)
        {
            using (var compressedByteStream = new FileStream(sourceFilename, FileMode.Open))
            {
                // GZipStream requires using. Do not optimize the stream.
                using (var zip = new GZipStream(compressedByteStream, CompressionMode.Decompress))
                {
                    var arr = new byte[numberBytes];
                    zip.Read(arr, 0, numberBytes);
                    return arr;
                }
            }
        }

        public static void GZipCompress(string sourceFilename, string destinationFilename)
        {
            // Low memory, fast compress.
            using (var originalByteStream = new FileStream(sourceFilename, FileMode.Open))
            {
                if (File.Exists(destinationFilename))
                    File.Delete(destinationFilename);

                using (var compressedByteStream = new FileStream(destinationFilename, FileMode.CreateNew))
                {
                    // GZipStream requires using. Do not optimize the stream.
                    using (var compressionStream = new GZipStream(compressedByteStream, CompressionMode.Compress, true))
                    {
                        originalByteStream.CopyTo(compressionStream);
                    }
                    Debug.WriteLine("Compressed from {0:#,###0} bytes to {1:#,###0} bytes.", originalByteStream.Length, compressedByteStream.Length);
                }
            }
        }

        /// <summary>
        /// check for Magic Number: 1f 8b
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool IsGzipedFile(string filename)
        {
            try
            {
                using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var b1 = stream.ReadByte();
                    var b2 = stream.ReadByte();
                    return (b1 == 0x1f && b2 == 0x8b);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
