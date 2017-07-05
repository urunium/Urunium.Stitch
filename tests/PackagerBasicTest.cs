using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urunium.Stitch.FileHandlers;

namespace Urunium.Stitch.Tests
{
    [TestFixture]
    public class PackagerBasicTest
    {
        [Test]
        public void PackagerPackage()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("exports.default = ()=>'Hello';") },
                { @"c:\App\font.ttf", new MockFileData("ttffilehere") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler()
                            });
            Package package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                Globals = new GlobalsConfig
                {
                    ["react"] = "React",
                    ["react-dom"] = "ReactDOM"
                },
                CopyFiles = new[] { "./font.ttf" }
            });
            Assert.AreEqual(3, package.Modules.Count);
            Assert.AreEqual(1, package.Files.Count);
        }

        [Test]
        public void PackagerTranspilesJs()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("export default ()=>'Hello';") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler()
                            });
            Package package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                Globals = new GlobalsConfig
                {
                    ["react"] = "React",
                    ["react-dom"] = "ReactDOM"
                }
            });
            Assert.AreEqual(3, package.Modules.Count);
            Assert.AreEqual(@"'use strict';Object.defineProperty(exports, ""__esModule"", {  value: true});exports.default = function () {  return 'Hello';};", package.Modules[2].TransformedContent.Replace("\r\n", "").Replace("\n", ""));
        }

        [Test]
        public void PackagerCalculatesModuleId()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("export default ()=>'Hello';") },
                { @"c:\App\Module1\index.js", new MockFileData("export default ()=>'Hello';") },
                { @"c:\App\Module1\dir\index.js", new MockFileData("export default ()=>'Hello';") },
                { @"c:\App\Module1\dir\module.js", new MockFileData("export default ()=>'Hello';") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler()
                            });
            Package package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                Globals = new GlobalsConfig
                {
                    ["react"] = "React",
                    ["react-dom"] = "ReactDOM"
                },
            });
            Assert.AreEqual(3, package.Modules.Count);
            Assert.AreEqual("react", package.Modules[0].ModuleId);
            Assert.AreEqual("react-dom", package.Modules[1].ModuleId);
            Assert.AreEqual("App", package.Modules[2].ModuleId);

            package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./Module1" },
                Globals = new GlobalsConfig
                {
                    ["react"] = "React",
                    ["react-dom"] = "ReactDOM"
                }
            });
            Assert.AreEqual(3, package.Modules.Count);
            Assert.AreEqual("react", package.Modules[0].ModuleId);
            Assert.AreEqual("react-dom", package.Modules[1].ModuleId);
            Assert.AreEqual("Module1/", package.Modules[2].ModuleId);

            package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./Module1/dir" },
                Globals = new GlobalsConfig
                {
                    ["react"] = "React",
                    ["react-dom"] = "ReactDOM"
                }
            });
            Assert.AreEqual(3, package.Modules.Count);
            Assert.AreEqual("react", package.Modules[0].ModuleId);
            Assert.AreEqual("react-dom", package.Modules[1].ModuleId);
            Assert.AreEqual("Module1/dir/", package.Modules[2].ModuleId);

            package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./Module1/dir/module" },
                Globals = new GlobalsConfig
                {
                    ["react"] = "React",
                    ["react-dom"] = "ReactDOM"
                }
            });
            Assert.AreEqual(3, package.Modules.Count);
            Assert.AreEqual("react", package.Modules[0].ModuleId);
            Assert.AreEqual("react-dom", package.Modules[1].ModuleId);
            Assert.AreEqual("Module1/dir/module", package.Modules[2].ModuleId);

        }
    }
}
