using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch.Tests
{
    [TestFixture]
    public class RealFileSystemTest
    {
        [Test]
        public void BuildWithRealFilesTest()
        {
            string location = System.IO.Path.GetFullPath(new Uri(System.IO.Path.GetDirectoryName(typeof(Stitcher).Assembly.CodeBase)).AbsolutePath);
            string rootPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(location));
            Stitcher.Stitch
                .From(source =>
                {
                    source.RootPath = rootPath;
                    source.EntryPoints = new[] { "./Views/App" };
                })
                .Into((dest) =>
                {
                    dest.Directory = location;
                    dest.BundleFileName = "app.bundle.js";
                })
                .UsingDefaultFileSystem()
                .UsingDefaultFileHandlers()
                .Sew();

            Assert.True(System.IO.File.Exists(System.IO.Path.Combine(location, "Views\\app.bundle.js")));
        }
    }
}