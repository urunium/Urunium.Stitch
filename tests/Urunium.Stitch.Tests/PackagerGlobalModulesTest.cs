﻿using NUnit.Framework;
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
    public class PackagerGlobalModulesTest
    {
        [Test]
        public void PackagerCanWrapGlobalModules()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("exports.default = ()=>'Hello';") },
                { @"c:\App\font.ttf", new MockFileData("ttffilehere") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            transformers: new IModuleTransformer[]
                            {
                                new BabelModuleTransformer(fileSystem)
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
        public void PackagerWrapsGlobalModules()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("exports.default = ()=>'Hello';") },
                { @"c:\App\font.ttf", new MockFileData("ttffilehere") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            transformers: new IModuleTransformer[]
                            {
                                new BabelModuleTransformer(fileSystem)
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

            Assert.AreEqual("Object.defineProperty(exports, '__esModule', {value: true}); exports.default = React", package.Modules[0].TransformedContent);
        }
    }
}
