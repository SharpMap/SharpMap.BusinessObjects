using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using SharpMap.Data.Providers.Business.MongoDB.Gtfs.Import;

namespace SharpMap.Business.Tests.MongoDB.Gtfs
{
    public class GtfsImportTest
    {
        //[TestCase("C:\\Downloads\\gtfs-nl")]
        [TestCase("TestData\\15619.zip")]
        public void TestImport(string path)
        {
            /*
            if (!File.Exists(path))
                throw new IgnoreException("File not found", new FileNotFoundException(path));
             */

            if (!Path.IsPathRooted(path))
                CopyToWorkingDirectory(path);
            else if (!File.Exists(path))
                throw new IgnoreException("File not found", new FileNotFoundException(path));

            var ext = Path.GetExtension(path);
            if (!string.IsNullOrEmpty(ext) && ext == ".zip")
                path = ExtractZip(path);

            try
            {
                var di = new DirectoryInfo(path);
                ImportFromFolder.Import(di);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new IgnoreException("Exception thrown", e);
            }
        }

        private string ExtractZip(string zipPath)
        {
            var workingZipPath = Path.IsPathRooted(zipPath)
                ? zipPath
                : Path.Combine(Directory.GetCurrentDirectory(), zipPath);

            var workingDataDirectory = Path.Combine(
                Path.GetDirectoryName(workingZipPath),
                Path.GetFileNameWithoutExtension(zipPath));

            if (Directory.Exists(workingDataDirectory))
                Directory.Delete(workingDataDirectory, true);

            Directory.CreateDirectory(workingDataDirectory);

            ZipFile zf = null;
            try
            {
                var fs = File.OpenRead(workingZipPath);
                zf = new ZipFile(fs);

                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    var buffer = new byte[4096];     // 4K is optimum
                    var zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    var fullZipToPath = Path.Combine(workingDataDirectory, entryFileName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (var streamWriter = File.Create(fullZipToPath))
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
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
            return workingDataDirectory;
        }

        private void CopyToWorkingDirectory(string path)
        {
            var dst = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (!(Directory.Exists(dst) ||File.Exists(dst)))
            {
                var src = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                File.Copy(src, dst, true);
            }
        }
    }
}