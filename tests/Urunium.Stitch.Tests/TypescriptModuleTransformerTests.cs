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
    public class TypescriptModuleTransformerTests
    {
        [Test]
        public void TransFormTest()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.tsx", new MockFileData("export class Component extends React.Component<TProp, TState> { render() { return <div>Hello world</div>;}}") },
                { @"c:\App\font.ttf", new MockFileData("ttffilehere") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IModuleTransformer[]
                            {
                                new TypescriptModuleTransformer(fileSystem)
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

            var transpiled = @"""use strict"";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var Component = (function (_super) {
    __extends(Component, _super);
    function Component() {
        return _super.apply(this, arguments) || this;
    }
    Component.prototype.render = function () { return React.createElement(""div"", null, ""Hello world""); };
    return Component;
}(React.Component));
exports.Component = Component;".Replace("\r\n", "");
            Assert.AreEqual(transpiled, package.Modules[2].TransformedContent.Replace("\r\n", ""));
        }
    }
}
