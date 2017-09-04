namespace SEToolbox.Support
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;

    using ICSharpCode.SharpZipLib.Core;
    using ICSharpCode.SharpZipLib.Zip;
    using Res = SEToolbox.Properties.Resources;

    /// <summary>
    /// Sourced from: https://github.com/icsharpcode/SharpZipLib/wiki/Zip-Samples
    /// </summary>
    public static class ZipTools
    {
        public static void MakeClearDirectory(string folder)
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);
        }

        public static void ExtractZipFileToDirectory(string archiveFilenameIn, string password, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                var fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password;     // AES encrypted entries are handled automatically
                }

                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    var entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    var buffer = new byte[4096];     // 4K is optimum
                    var zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    var fullZipToPath = Path.Combine(outFolder, entryFileName);
                    var directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (!string.IsNullOrEmpty(directoryName))
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (var streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }

        public static void ExtractZipFileToFile(string zipArchiveFilenameIn, string password, string archivedFile, string outFile)
        {
            ZipFile zf = null;
            try
            {
                var fs = File.OpenRead(zipArchiveFilenameIn);
                zf = new ZipFile(fs);
                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password;     // AES encrypted entries are handled automatically
                }

                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }

                    var seperator = Path.DirectorySeparatorChar.ToString(CultureInfo.CurrentUICulture);

                    if (zipEntry.Name.Replace("/", seperator).Equals(archivedFile, StringComparison.OrdinalIgnoreCase))
                    {
                        var buffer = new byte[4096]; // 4K is optimum
                        var zipStream = zf.GetInputStream(zipEntry);

                        // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                        // of the file, but does not waste memory.
                        // The "using" will close the stream even if an exception occurs.
                        using (var streamWriter = File.Create(outFile))
                        {
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        }
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }

        public static Stream ExtractZipFileToSteam(string zipArchiveFilenameIn, string password, string archivedFile)
        {
            ZipFile zf = null;
            try
            {
                var fs = File.OpenRead(zipArchiveFilenameIn);
                zf = new ZipFile(fs);
                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password;     // AES encrypted entries are handled automatically
                }

                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }

                    var seperator = Path.DirectorySeparatorChar.ToString(CultureInfo.CurrentUICulture);

                    if (zipEntry.Name.Replace("/", seperator).Equals(archivedFile, StringComparison.OrdinalIgnoreCase))
                    {
                        var buffer = new byte[4096]; // 4K is optimum
                        var zipStream = zf.GetInputStream(zipEntry);

                        // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                        // of the file, but does not waste memory.
                        // The "using" will close the stream even if an exception occurs.

                        var stream = new MemoryStream();
                        StreamUtils.Copy(zipStream, stream, buffer);
                        stream.Position = 0;
                        return stream;
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
            return null;
        }

        public static string[] ExtractZipContentList(string archiveFilenameIn, string password)
        {
            var fileList = new List<string>();

            if (!File.Exists(archiveFilenameIn))
                return fileList.ToArray();

            ZipFile zf = null;
            try
            {
                var fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password; // AES encrypted entries are handled automatically
                }

                fileList.AddRange(from ZipEntry zipEntry in zf where zipEntry.IsFile select zipEntry.Name.Replace('/', Path.DirectorySeparatorChar));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(String.Format(Res.Exception_CorruptZipFile, archiveFilenameIn), ex);
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
            return fileList.ToArray();
        }

        public static void ZipFolder(string sourceFolderName, string password, string outPathname)
        {
            var fsOut = File.Create(outPathname);
            var zipStream = new ZipOutputStream(fsOut);

            zipStream.SetLevel(9); //0-9, 9 being the highest level of compression

            zipStream.Password = password;  // optional. Null is the same as not setting. Required if using AES.

            // This setting will strip the leading part of the folder path in the entries, to
            // make the entries relative to the starting folder.
            // To include the full path for each entry up to the drive root, assign folderOffset = 0.
            var folderOffset = sourceFolderName.Length + (sourceFolderName.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.CurrentUICulture)) ? 0 : 1);

            CompressFolder(sourceFolderName, zipStream, folderOffset);

            zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
            zipStream.Close();
        }

        private static void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {
            var files = Directory.GetFiles(path);

            foreach (var filename in files)
            {
                var fi = new FileInfo(filename);

                var entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                var newEntry = new ZipEntry(entryName)
                {
                    DateTime = fi.LastWriteTime,    // Note the zip format stores 2 second granularity
                    Size = fi.Length
                };

                // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                // A password on the ZipOutputStream is required if using AES.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                // but the zip will be in Zip64 format which not all utilities can understand.
                //   zipStream.UseZip64 = UseZip64.Off;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                var buffer = new byte[4096];
                using (var streamReader = File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }

            var folders = Directory.GetDirectories(path);

            foreach (var folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }
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
                using (var stream = File.OpenRead(filename))
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
