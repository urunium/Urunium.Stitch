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
    public class PackagerRequireTests
    {
        [Test]
        public void PackagerCanProcessJsRequire()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import m from './module';export default ()=>`Hello ${m()}`;") },
                { @"c:\App\module.js", new MockFileData("export default () => 'world';") }
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
                Globals = new Dictionary<string, string> { { "react", "React" }, { "react-dom", "ReactDOM" } }
            });
            Assert.AreEqual(2, package.Modules.Count);

            Assert.AreEqual("App", package.Modules[0].ModuleId);
            Assert.AreEqual("module", package.Modules[1].ModuleId);
        }

        [Test]
        public void PackagerCanProcessNestedRequire()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import m from './module';export default ()=>`Hello ${m()}`;") },
                { @"c:\App\module.js", new MockFileData("import world from './world';export default () => world;") },
                { @"c:\App\world.js", new MockFileData("export default 'world';") }
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
                Globals = new Dictionary<string, string> { { "react", "React" }, { "react-dom", "ReactDOM" } }
            });
            Assert.AreEqual(3, package.Modules.Count);

            Assert.AreEqual("App", package.Modules[0].ModuleId);
            Assert.AreEqual("module", package.Modules[1].ModuleId);
            Assert.AreEqual("world", package.Modules[2].ModuleId);
        }

        [Test]
        public void PackagerCanProcessDirectoryModuleRequire()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import m from './module';export default ()=>`Hello ${m()}`;") },
                { @"c:\App\module.js", new MockFileData("import world from './world';export default () => world;") },
                { @"c:\App\world\index.js", new MockFileData("export default 'world';") }
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
                Globals = new Dictionary<string, string> { { "react", "React" }, { "react-dom", "ReactDOM" } }
            });
            Assert.AreEqual(3, package.Modules.Count);

            Assert.AreEqual("App", package.Modules[0].ModuleId);
            Assert.AreEqual("module", package.Modules[1].ModuleId);
            Assert.AreEqual("world/", package.Modules[2].ModuleId);
        }

        [Test]
        public void PackagerCanProcessDirectoryModuleWithPackageJsonRequire()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import m from './module';export default ()=>`Hello ${m()}`;") },
                { @"c:\App\module.js", new MockFileData("import world from './world';export default () => world;") },
                { @"c:\App\world\module.js", new MockFileData("export default 'world';") },
                { @"c:\App\world\package.json", new MockFileData("{'main':'module.js'}") }
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
                Globals = new Dictionary<string, string> { { "react", "React" }, { "react-dom", "ReactDOM" } }
            });
            Assert.AreEqual(3, package.Modules.Count);

            Assert.AreEqual("App", package.Modules[0].ModuleId);
            Assert.AreEqual("module", package.Modules[1].ModuleId);
            Assert.AreEqual("world/", package.Modules[2].ModuleId);
        }

        [Test]
        public void PackagerCanProcessCssRequire()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import './app.css';export default ()=>`Hello`;") },
                { @"c:\App\app.css", new MockFileData("body{background:#FF0000}") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler(),
                                new LessFileHandler(fileSystem)
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                Globals = new Dictionary<string, string> { { "react", "React" }, { "react-dom", "ReactDOM" } }
            });
            Assert.AreEqual(2, package.Modules.Count);

            Assert.AreEqual("App", package.Modules[0].ModuleId);
            Assert.AreEqual("app.css", package.Modules[1].ModuleId);
        }

        [Test]
        public void PackagerCanProcessLessRequire()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import './app.less';export default ()=>`Hello`;") },
                { @"c:\App\app.less", new MockFileData("body{background:#FF0000}") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler(),
                                new LessFileHandler(fileSystem)
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                Globals = new Dictionary<string, string> { { "react", "React" }, { "react-dom", "ReactDOM" } }
            });
            Assert.AreEqual(2, package.Modules.Count);

            Assert.AreEqual("App", package.Modules[0].ModuleId);
            Assert.AreEqual("app.less", package.Modules[1].ModuleId);
        }

        [Test]
        public void PackagerCanProcessSassRequire()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import './app.sass';export default ()=>`Hello`;") },
                { @"c:\App\app.sass", new MockFileData("body{background:#FF0000}") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler(),
                                new LessFileHandler(fileSystem)
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                Globals = new Dictionary<string, string> { { "react", "React" }, { "react-dom", "ReactDOM" } }
            });
            Assert.AreEqual(2, package.Modules.Count);

            Assert.AreEqual("App", package.Modules[0].ModuleId);
            Assert.AreEqual("app.sass", package.Modules[1].ModuleId);
        }

        [Test]
        public void PackagerCanProcessScssRequire()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import './app.scss';export default ()=>`Hello`;") },
                { @"c:\App\app.scss", new MockFileData("body{background:#FF0000}") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler(),
                                new LessFileHandler(fileSystem)
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                Globals = new Dictionary<string, string> { { "react", "React" }, { "react-dom", "ReactDOM" } }
            });
            Assert.AreEqual(2, package.Modules.Count);

            Assert.AreEqual("App", package.Modules[0].ModuleId);
            Assert.AreEqual("app.scss", package.Modules[1].ModuleId);
        }

        [Test]
        public void PackagerCanProcessJsxRequire()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import component from './component';export default ()=>`Hello`;") },
                { @"c:\App\component.jsx", new MockFileData("class Com extends React.Component{render(){return <div></div>}}") }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler(),
                                new LessFileHandler(fileSystem)
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                Globals = new Dictionary<string, string> { { "react", "React" }, { "react-dom", "ReactDOM" } }
            });
            Assert.AreEqual(2, package.Modules.Count);

            Assert.AreEqual("App", package.Modules[0].ModuleId);
            Assert.AreEqual("component", package.Modules[1].ModuleId);
        }

        [Test]
        public void PackagerCanProcessMultipleRequires()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData(@"import dep from './dep1';import './Styles/app.css';import './Styles/app.less';import './Styles/app.scss';export default ()=>`Hello`;") },
                { @"c:\App\dep1.jsx", new MockFileData("var dep11 = require('./dep1-1');class Com extends React.Component{render(){return <div></div>}}") },
                { @"c:\App\dep1-1.js", new MockFileData(@"require('./modules/filemodule1');exports.default = {hello: function() {return 'world'}}") },
                { @"c:\App\modules\filemodule1.js", new MockFileData("require('./dirmodule');") },
                { @"c:\App\modules\dirmodule\index.js", new MockFileData("require('./dirapp');") },
                { @"c:\App\modules\dirmodule\dirapp.js", new MockFileData("") },
                { @"c:\App\Styles\app.css", new MockFileData("body{}") },
                { @"c:\App\Styles\app.less", new MockFileData("body{}") },
                { @"c:\App\Styles\app.scss", new MockFileData("body{}") },
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler(),
                                new LessFileHandler(fileSystem)
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                Globals = new Dictionary<string, string> { { "react", "React" }, { "react-dom", "ReactDOM" } }
            });
            Assert.AreEqual(9, package.Modules.Count);

            Assert.AreEqual("App", package.Modules[0].ModuleId);
            Assert.AreEqual("dep1", package.Modules[1].ModuleId);
            Assert.AreEqual("dep1-1", package.Modules[2].ModuleId);
            Assert.AreEqual("modules/filemodule1", package.Modules[3].ModuleId);
            Assert.AreEqual("modules/dirmodule/", package.Modules[4].ModuleId);
            Assert.AreEqual("modules/dirmodule/dirapp", package.Modules[5].ModuleId);
            Assert.AreEqual("Styles/app.css", package.Modules[6].ModuleId);
            Assert.AreEqual("Styles/app.less", package.Modules[7].ModuleId);
            Assert.AreEqual("Styles/app.scss", package.Modules[8].ModuleId);
        }

        [Test]
        public void PackagerCanProcessPngRequire()
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

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import png from './img.png';export default ()=>`Hello`;") },
                { @"c:\App\img.png", new MockFileData(imageData) }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler(),
                                new Base64FileHandler()
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                Globals = new Dictionary<string, string> { { "react", "React" }, { "react-dom", "ReactDOM" } }
            });
            Assert.AreEqual(2, package.Modules.Count);

            Assert.AreEqual("App", package.Modules[0].ModuleId);
            Assert.AreEqual("img.png", package.Modules[1].ModuleId);
        }

        [Test]
        public void PackagerCanProcessJpgRequire()
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

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import png from './img.jpg';export default ()=>`Hello`;") },
                { @"c:\App\img.jpg", new MockFileData(imageData) }
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler(),
                                new Base64FileHandler()
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                Globals = new Dictionary<string, string> { { "react", "React" }, { "react-dom", "ReactDOM" } }
            });
            Assert.AreEqual(2, package.Modules.Count);

            Assert.AreEqual("App", package.Modules[0].ModuleId);
            Assert.AreEqual("img.jpg", package.Modules[1].ModuleId);
        }

        [Test]
        public void PackagerCanProcessMultipleImageRequire()
        {
            byte[] imageDataJpg;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = "Urunium.Stitch.Tests.img.jpg";

            using (System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                stream.CopyTo(ms);
                imageDataJpg = ms.ToArray();
            }

            byte[] imageDataPng;
            var resourceNamePng = "Urunium.Stitch.Tests.img.png";

            using (System.IO.Stream stream = assembly.GetManifestResourceStream(resourceNamePng))
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                stream.CopyTo(ms);
                imageDataPng = ms.ToArray();
            }

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import jpg from './img.jpg';import png from './img.png';export default ()=>`Hello`;") },
                { @"c:\App\img.jpg", new MockFileData(imageDataJpg) },
                { @"c:\App\img.png", new MockFileData(imageDataPng) },
            });

            var packager = new Packager(
                            fileSystem: fileSystem,
                            handlers: new IFileHandler[]
                            {
                                new BabelFilehandler(),
                                new Base64FileHandler()
                            });
            Package package = packager.Package(new PackagerConfig
            {
                RootPath = @"c:\App",
                EntryPoints = new[] { "./App" },
                Globals = new Dictionary<string, string> { { "react", "React" }, { "react-dom", "ReactDOM" } }
            });
            Assert.AreEqual(3, package.Modules.Count);

            Assert.AreEqual("App", package.Modules[0].ModuleId);
            Assert.AreEqual("img.jpg", package.Modules[1].ModuleId);
            Assert.AreEqual("img.png", package.Modules[2].ModuleId);
        }
    }
}
