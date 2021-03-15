using NUnit.Framework;
using System;
using System.IO;
using System.IO.Compression;
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
                ImportFromFolder.Import(di).GetAwaiter().GetResult();
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
            ZipFile.ExtractToDirectory(zipPath, workingDataDirectory);

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
