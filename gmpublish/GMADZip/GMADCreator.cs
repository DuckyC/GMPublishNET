using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GMPublish.GMAD
{
    public static class GMADCreator
    {
        private static string formatFilename(string baseFolder, string before)
        {
            return before.TrimStart(baseFolder).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).TrimStart(Path.AltDirectorySeparatorChar).ToLowerInvariant();
        }

        private static List<string> GetFilesRecursively(string baseFolder)
        {
            var paths = new List<string>();

            foreach (var file in Directory.GetFiles(baseFolder))
            {
                    paths.Add(file);
            }

            foreach (var dir in Directory.GetDirectories(baseFolder))
            {
                var subPaths = GetFilesRecursively(dir);
                paths = paths.Concat(subPaths).ToList();

            }
            
            return paths;
        }

        public static void Create(string baseFolder, AddonJSON addon, Stream outputStream)
        {
            
            var description = addon.BuildDescription();

            var crcStream = new CrcStream(outputStream);
            BinaryWriter writer = new BinaryWriter(crcStream);
            writer.BaseStream.Seek(0, SeekOrigin.Begin);
            writer.BaseStream.SetLength(0);

            writer.Write(("GMAD").ToCharArray());
            writer.Write((char)3);
            writer.Write((ulong)0);
            writer.Write(DateTime.Now.ToUnixTime());
            writer.Write((char)0);
            writer.WriteNullTerminatedString(addon.Title);
            writer.WriteNullTerminatedString(description);
            writer.WriteNullTerminatedString("Author Name");
            writer.Write((int)1);

            var flatList = new List<string>();
            foreach (var file in GetFilesRecursively(baseFolder))
            {
                var fileName = formatFilename(baseFolder, file);
                if (!Whitelist.Check(fileName)) { continue; }
                if (fileName == addon.Icon) { continue; }
                if (fileName == "addon.json") { continue; }
                flatList.Add(file);
            }

            uint fileNum = 0;
            foreach (var file in flatList)
            {
                fileNum++;
                var fileName = formatFilename(baseFolder, file);
                uint fileCrc = 0;
                using (var stream = new FileStream(file, FileMode.Open))
                {
                    fileCrc = CRC32.ComputeChecksum(stream);
                }

                var fileInfo = new FileInfo(file);

                writer.Write(fileNum);
                writer.WriteNullTerminatedString(fileName);
                writer.Write(fileInfo.Length);
                writer.Write(fileCrc);
                Console.WriteLine($"{fileName} is #{fileNum} and is {fileInfo.Length} bytes with crc: {fileCrc} ");
            }

            writer.Flush();
            writer.Write((uint)0);


            foreach (var file in flatList)
            {
                using (var stream = new FileStream(file, FileMode.Open))
                {
                    byte[] buff = new byte[stream.Length];
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Read(buff, 0, buff.Length);
                    writer.Write(buff);
                    writer.Flush();

                }
            }

            writer.Write(crcStream.WriteCrc);
            writer.Flush();

            writer.BaseStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
