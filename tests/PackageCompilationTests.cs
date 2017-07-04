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
    public class PackageCompilationTests
    {
        [Test]
        public void CompilingPackageShouldCreateDirectoryAndCopyFiles()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\Fonts\font.ttf", new MockFileData("ttffilehere") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler()
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                CopyFiles = new[] { "./Fonts/font.ttf" }
            });

            PackageCompiler compiler = new PackageCompiler(fileSystem);
            compiler.Compile(package, destinationDirectory: @"c:\Bundle");

            Assert.True(fileSystem.FileExists(@"c:\Bundle\Fonts\font.ttf"));
        }

        [Test]
        public void CompilingPackageShouldCopyFilesToExistingDir()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\Fonts\font.ttf", new MockFileData("ttffilehere") }
            });
            fileSystem.Directory.CreateDirectory(@"c:\Bundle");

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler()
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                CopyFiles = new[] { "./Fonts/font.ttf" }
            });

            PackageCompiler compiler = new PackageCompiler(fileSystem);
            compiler.Compile(package, destinationDirectory: @"c:\Bundle");

            Assert.True(fileSystem.FileExists(@"c:\Bundle\Fonts\font.ttf"));
        }

        [Test]
        public void CompilingPackageShouldOverwriteExistingFiles()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\Fonts\font.ttf", new MockFileData("ttffilehere") },
                { @"c:\Bundle\Fonts\font.ttf", new MockFileData("oldttffilehere") }
            });
            
            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler()
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                CopyFiles = new[] { "./Fonts/font.ttf" }
            });

            PackageCompiler compiler = new PackageCompiler(fileSystem);
            compiler.Compile(package, destinationDirectory: @"c:\Bundle");

            Assert.True(fileSystem.FileExists(@"c:\Bundle\Fonts\font.ttf"));
            Assert.AreEqual("ttffilehere", fileSystem.File.ReadAllText(@"c:\Bundle\Fonts\font.ttf"));
        }

        [Test]
        public void CompilingPackageShouldBundleJs()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\Views\App.js", new MockFileData("import m from './module';export default ()=>`Hello ${m()}`;") },
                { @"c:\App\Views\module.js", new MockFileData("export default () => 'world';") },
                { @"c:\App\Views\Fonts\font.ttf", new MockFileData("ttffilehere") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler()
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./Views/App" },
                CopyFiles = new[] { "./Views/Fonts/font.ttf" }
            });

            PackageCompiler compiler = new PackageCompiler(fileSystem);
            compiler.Compile(package, destinationDirectory: @"c:\Bundle");

            Assert.True(fileSystem.FileExists(@"c:\Bundle\Views\bundle.js"));
            Assert.True(fileSystem.FileExists(@"c:\Bundle\Views\Fonts\font.ttf"));
        }

        [Test]
        public void CompilingPackageShouldBundleJs_FlatDirectory()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import m from './module';export default ()=>`Hello ${m()}`;") },
                { @"c:\App\module.js", new MockFileData("export default () => 'world';") },
                { @"c:\App\Fonts\font.ttf", new MockFileData("ttffilehere") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler()
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                CopyFiles = new[] { "./Fonts/font.ttf" }
            });

            PackageCompiler compiler = new PackageCompiler(fileSystem);
            compiler.Compile(package, destinationDirectory: @"c:\Bundle");

            Assert.True(fileSystem.FileExists(@"c:\Bundle\bundle.js"));
            Assert.True(fileSystem.FileExists(@"c:\Bundle\Fonts\font.ttf"));
        }

        [Test]
        public void CompilingPackageBundleAndCopyImage()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import m from './module';export default ()=>`Hello ${m()}`;") },
                { @"c:\App\module.js", new MockFileData("import './Images/image.gif'; export default () => 'world';") },
                { @"c:\App\Images\image.gif", new MockFileData("gif file") },
                { @"c:\App\Fonts\font.ttf", new MockFileData("ttffilehere") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler()
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                CopyFiles = new[] { "./Fonts/font.ttf", "./Images/image.gif" }
            });

            PackageCompiler compiler = new PackageCompiler(fileSystem);
            compiler.Compile(package, destinationDirectory: @"c:\Bundle");

            Assert.AreEqual("Images/image.gif", package.Modules[2].ModuleId);

            Assert.True(fileSystem.FileExists(@"c:\Bundle\bundle.js"));
            Assert.True(fileSystem.FileExists(@"c:\Bundle\Fonts\font.ttf"));
            Assert.True(fileSystem.FileExists(@"c:\Bundle\Images\image.gif"));
        }
    }
}
