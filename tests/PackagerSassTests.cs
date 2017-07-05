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
    public class PackagerSassTests
    {
        [Test]
        public void PackagerSupportsSass()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.sass", new MockFileData("body {background: red;}") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new SassFileHandler(fileSystem)
                            });
            Package package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                
            });
            Assert.AreEqual(1, package.Modules.Count);
            Assert.AreEqual("var styleNode = document.querySelector('style[data-module=\"App\"]'); if(!styleNode){ styleNode = document.createElement('STYLE'); styleNode.type = 'text/css'; styleNode.dataset.module = 'App'; document.head.appendChild(styleNode); }  styleNode.innerHtml = '';  var styleText = document.createTextNode(\"body{background:red}\\n\"); styleNode.appendChild(styleText);", package.Modules[0].TransformedContent);
        }

        [Test]
        public void PackagerSupportsSassImport()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\import.sass", new MockFileData("$color:black;body {border: $color;}") },
                { @"c:\App\App.sass", new MockFileData("@import './import.sass';$backColor:red;body {background: $backColor;}") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new SassFileHandler(fileSystem)
                            });
            Package package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                
            });
            Assert.AreEqual(1, package.Modules.Count);
            Assert.AreEqual("var styleNode = document.querySelector('style[data-module=\"App\"]'); if(!styleNode){ styleNode = document.createElement('STYLE'); styleNode.type = 'text/css'; styleNode.dataset.module = 'App'; document.head.appendChild(styleNode); }  styleNode.innerHtml = '';  var styleText = document.createTextNode(\"body{border:#000}body{background:red}\\n\"); styleNode.appendChild(styleText);", package.Modules[0].TransformedContent);
        }

        [Test]
        public void PackagerSupportsMultipleSassImport()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\import1.sass", new MockFileData("$color:black;body {border: $color;}") },
                { @"c:\App\import2.sass", new MockFileData("$color:black;body {border-right: $color;}") },
                { @"c:\App\App.sass", new MockFileData("@import './import1.sass';@import './import2.sass';$backColor:red;body {background: $backColor;}") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new SassFileHandler(fileSystem)
                            });
            Package package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                
            });
            Assert.AreEqual(1, package.Modules.Count);
            Assert.AreEqual("var styleNode = document.querySelector('style[data-module=\"App\"]'); if(!styleNode){ styleNode = document.createElement('STYLE'); styleNode.type = 'text/css'; styleNode.dataset.module = 'App'; document.head.appendChild(styleNode); }  styleNode.innerHtml = '';  var styleText = document.createTextNode(\"body{border:#000}body{border-right:#000}body{background:red}\\n\"); styleNode.appendChild(styleText);", package.Modules[0].TransformedContent);
        }
    }
}
