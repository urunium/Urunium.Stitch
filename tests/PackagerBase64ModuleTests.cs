using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urunium.Stitch.FileHandlers;

namespace Urunium.Stitch.Tests
{
    [TestFixture]
    public class PackagerBase64ModuleTests
    {
        [Test]
        public void PackagerSupportsGif()
        {
            var imageData = Convert.FromBase64String("R0lGODlhAQABAAAAACw=");
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\img.gif", new MockFileData(imageData) }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new Base64FileHandler()
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./img.gif" },
                Globals = new Dictionary<string, string> { { "react", "React" }, { "react-dom", "ReactDOM" } }
            });
            Assert.AreEqual(1, package.Modules.Count);
            Assert.AreEqual("Object.defineProperty(exports, '__esModule', {value: true}); exports.default = \"data:image/gif;base64,R0lGODlhAQABAAAAACw=\"", package.Modules[0].TransformedContent);
        }

        [Test]
        public void PackagerSupportsPng()
        {
            byte[] imageData;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = "Urunium.Stitch.Tests.img.png";

            using (System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                stream.CopyTo(ms);
                imageData = ms.ToArray();
            }
            var base64 = System.Convert.ToBase64String(imageData);

            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\img.png", new MockFileData(imageData) }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new Base64FileHandler()
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./img.png" },
                Globals = new Dictionary<string, string> { { "react", "React" }, { "react-dom", "ReactDOM" } }
            });
            Assert.AreEqual(1, package.Modules.Count);
            Assert.AreEqual("Object.defineProperty(exports, '__esModule', {value: true}); exports.default = \"data:image/png;base64," + base64 + "\"", package.Modules[0].TransformedContent);
        }


        [Test]
        public void PackagerSupportsJpeg()
        {
            byte[] imageData;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = "Urunium.Stitch.Tests.img.jpg";

            using (System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                stream.CopyTo(ms);
                imageData = ms.ToArray();
            }
            var base64 = System.Convert.ToBase64String(imageData);

            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\img.jpg", new MockFileData(imageData) }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new Base64FileHandler()
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./img.jpg" },
                Globals = new Dictionary<string, string> { { "react", "React" }, { "react-dom", "ReactDOM" } }
            });
            Assert.AreEqual(1, package.Modules.Count);
            Assert.AreEqual("Object.defineProperty(exports, '__esModule', {value: true}); exports.default = \"data:image/jpeg;base64," + base64 + "\"", package.Modules[0].TransformedContent);
        }
    }
}
