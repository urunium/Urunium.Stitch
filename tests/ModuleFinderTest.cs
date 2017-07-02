using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch.Tests
{
    [TestFixture]
    public class ModuleFinderTest
    {
        [Test]
        public void FindLocalModule()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("exports.default = ()=>'Hello';") },
                { @"c:\App\font.ttf", new MockFileData("ttffilehere") }
            });
            var finder = new ModuleFinder(fileSystem, new[] { "js" });
            var modulePath = finder.FindModulePath("./App", @"c:\App", null);
            Assert.AreEqual(@"c:\App\App.js", modulePath);
        }

        [Test]
        public void FindAsIndexModule()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\MyModule\index.js", new MockFileData("exports.default = ()=>'Hello';") },
                { @"c:\App\font.ttf", new MockFileData("ttffilehere") }
            });
            var finder = new ModuleFinder(fileSystem, new[] { "js" });
            var modulePath = finder.FindModulePath("./MyModule", @"c:\App", null);
            Assert.AreEqual(@"c:\App\MyModule\index.js", modulePath);
        }

        [Test]
        public void FindAsPackageModule()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\MyModule\a.js", new MockFileData("exports.default = ()=>'Hello';") },
                { @"c:\App\MyModule\package.json", new MockFileData("{'name':'mypackae','scripts':[],'main':'a.js'}") },
                { @"c:\App\font.ttf", new MockFileData("ttffilehere") }
            });
            var finder = new ModuleFinder(fileSystem, new[] { "js" });
            var modulePath = finder.FindModulePath("./MyModule", @"c:\App", null);
            Assert.AreEqual(@"c:\App\MyModule\a.js", modulePath);
        }

        [Test]
        public void FindAsPackageModule_PackageGetsPriority()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\MyModule\a.js", new MockFileData("exports.default = ()=>'Hello';") },
                { @"c:\App\MyModule\index.js", new MockFileData("exports.default = ()=>'Hello from index';") },
                { @"c:\App\MyModule\package.json", new MockFileData("{'name':'mypackae','scripts':[],'main':'a.js'}") },
                { @"c:\App\font.ttf", new MockFileData("ttffilehere") }
            });
            var finder = new ModuleFinder(fileSystem, new[] { "js" });
            var modulePath = finder.FindModulePath("./MyModule", @"c:\App", null);
            Assert.AreEqual(@"c:\App\MyModule\a.js", modulePath);
        }

        [Test]
        public void FindAsNodeModule()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\node_modules\MyModule\index.js", new MockFileData("exports.default = ()=>'Hello';") },
                { @"c:\App\font.ttf", new MockFileData("ttffilehere") }
            });
            var finder = new ModuleFinder(fileSystem, new[] { "js" });
            var modulePath = finder.FindModulePath("MyModule", @"c:\App", null);
            Assert.AreEqual(@"c:\App\node_modules\MyModule\index.js", modulePath);
        }
    }
}
