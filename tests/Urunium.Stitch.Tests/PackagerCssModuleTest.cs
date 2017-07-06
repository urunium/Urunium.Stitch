using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urunium.Stitch.ModuleTransformers;

namespace Urunium.Stitch.Tests
{
    [TestFixture]
    public class PackagerCssModuleTest
    {
        [Test]
        public void PackagerSupportsCSS()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.css", new MockFileData("body {background: red;}") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IModuleTransformer[]
                            {
                                new LessModuleTransformer(fileSystem)
                            });
            Package package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                
            });
            Assert.AreEqual(1, package.Modules.Count);
            Assert.AreEqual("var styleNode = document.querySelector('style[data-module=\"App\"]'); if(!styleNode){ styleNode = document.createElement('STYLE'); styleNode.type = 'text/css'; styleNode.dataset.module = 'App'; document.head.appendChild(styleNode); }  styleNode.innerHtml = '';  var styleText = document.createTextNode(\"body{background:red}\"); styleNode.appendChild(styleText);", package.Modules[0].TransformedContent);
        }

        [Test]
        public void PackagerSupportsLess()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.css", new MockFileData("@color:red;body {background: @color;}") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IModuleTransformer[]
                            {
                                new LessModuleTransformer(fileSystem)
                            });
            Package package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                
            });
            Assert.AreEqual(1, package.Modules.Count);
            Assert.AreEqual("var styleNode = document.querySelector('style[data-module=\"App\"]'); if(!styleNode){ styleNode = document.createElement('STYLE'); styleNode.type = 'text/css'; styleNode.dataset.module = 'App'; document.head.appendChild(styleNode); }  styleNode.innerHtml = '';  var styleText = document.createTextNode(\"body{background:red}\"); styleNode.appendChild(styleText);", package.Modules[0].TransformedContent);
        }

        [Test]
        public void PackagerHandlesCssImport()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\import.css", new MockFileData("@borderColor:green;body {border: 1px solid @borderColor;}") },
                { @"c:\App\App.css", new MockFileData("@import './import.css';@color:red;body {background: @color;}") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IModuleTransformer[]
                            {
                                new LessModuleTransformer(fileSystem)
                            });
            Package package = packager.Package(new SourceConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                
            });
            Assert.AreEqual(1, package.Modules.Count);
            Assert.AreEqual("var styleNode = document.querySelector('style[data-module=\"App\"]'); if(!styleNode){ styleNode = document.createElement('STYLE'); styleNode.type = 'text/css'; styleNode.dataset.module = 'App'; document.head.appendChild(styleNode); }  styleNode.innerHtml = '';  var styleText = document.createTextNode(\"body{border:1px solid green}body{background:red}\"); styleNode.appendChild(styleText);", package.Modules[0].TransformedContent);
        }
    }
}
