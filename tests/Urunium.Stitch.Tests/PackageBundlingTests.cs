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
    public class PackageBundlingTests
    {
        [Test]
        public void BundlingPackageShouldCreateDirectoryAndCopyFiles()
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
            Package package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                CopyFiles = new[] { "./Fonts/font.ttf" }
            });

            PackageBundler bundler = new PackageBundler(fileSystem);
            bundler.CreateBundle(package, destinationDirectory: @"c:\Bundle");

            Assert.True(fileSystem.FileExists(@"c:\Bundle\Fonts\font.ttf"));
        }

        [Test]
        public void BundlingPackageShouldCopyFilesToExistingDir()
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
            Package package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                CopyFiles = new[] { "./Fonts/font.ttf" }
            });

            PackageBundler bundler = new PackageBundler(fileSystem);
            bundler.CreateBundle(package, destinationDirectory: @"c:\Bundle");

            Assert.True(fileSystem.FileExists(@"c:\Bundle\Fonts\font.ttf"));
        }

        [Test]
        public void BundlingPackageShouldOverwriteExistingFiles()
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
            Package package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                CopyFiles = new[] { "./Fonts/font.ttf" }
            });

            PackageBundler bundler = new PackageBundler(fileSystem);
            bundler.CreateBundle(package, destinationDirectory: @"c:\Bundle");

            Assert.True(fileSystem.FileExists(@"c:\Bundle\Fonts\font.ttf"));
            Assert.AreEqual("ttffilehere", fileSystem.File.ReadAllText(@"c:\Bundle\Fonts\font.ttf"));
        }

        [Test]
        public void BundlingPackageShouldBundleJs()
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
            Package package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./Views/App" },
                CopyFiles = new[] { "./Views/Fonts/font.ttf" }
            });

            PackageBundler bundler = new PackageBundler(fileSystem);
            bundler.CreateBundle(package, destinationDirectory: @"c:\Bundle");

            Assert.True(fileSystem.FileExists(@"c:\Bundle\Views\bundle.js"));
            Assert.True(fileSystem.FileExists(@"c:\Bundle\Views\Fonts\font.ttf"));
        }

        [Test]
        public void BundlingPackageShouldBundleJs_FlatDirectory()
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
            Package package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                CopyFiles = new[] { "./Fonts/font.ttf" }
            });

            PackageBundler bundler = new PackageBundler(fileSystem);
            bundler.CreateBundle(package, destinationDirectory: @"c:\Bundle");

            Assert.True(fileSystem.FileExists(@"c:\Bundle\bundle.js"));
            Assert.True(fileSystem.FileExists(@"c:\Bundle\Fonts\font.ttf"));
        }

        [Test]
        public void BundlingPackageBundleAndCopyImage()
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
            Package package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                CopyFiles = new[] { "./Fonts/font.ttf", "./Images/image.gif" }
            });

            PackageBundler bundler = new PackageBundler(fileSystem);
            bundler.CreateBundle(package, destinationDirectory: @"c:\Bundle");

            Assert.AreEqual("Images/image.gif", package.Modules[2].ModuleId);

            Assert.True(fileSystem.FileExists(@"c:\Bundle\bundle.js"));
            Assert.True(fileSystem.FileExists(@"c:\Bundle\Fonts\font.ttf"));
            Assert.True(fileSystem.FileExists(@"c:\Bundle\Images\image.gif"));
        }
    }
}
