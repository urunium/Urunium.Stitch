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
    public class PackageBuilderTest
    {
        [Test]
        public void BuildTest()
        {
            IFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("exports.default = ()=>'Hello';") },
                { @"c:\App\font.ttf", new MockFileData("ttffilehere") }
            });
            Stitcher.Stitch
                .Modules((modules) =>
                {
                    modules.RootedAt(@"c:\App");
                    modules.EntryPoints(new[] { "./App" });
                    modules.CopyFiles(new[] { "./font.ttf" });
                }).Into((modules) =>
                {
                    modules.BundleAt(@"c:\Bundle");
                    modules.BundleInto("app.bundle.js");
                })
                .WithFileSystem(fileSystem)
                .AddFileHandler<BabelFilehandler>()
                .AddFileHandler<LessFileHandler>()
                .AddFileHandler<SassFileHandler>()
                .AddFileHandler<Base64FileHandler>()
                .Sew();

            Assert.True(fileSystem.File.Exists(@"c:\Bundle\app.bundle.js"));
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\font.ttf"));
        }

        [Test]
        public void BuildDiTest()
        {
            IFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import './font.ttf'; exports.default = ()=>'Hello';") },
                { @"c:\App\font.ttf", new MockFileData("ttffilehere") }
            });
            Stitcher.Stitch
                .Modules((modules) =>
                {
                    modules.RootedAt(@"c:\App");
                    modules.EntryPoints(new[] { "./App" });
                    modules.CopyFiles(new[] { "./font.ttf" });
                }).Into((modules) =>
                {
                    modules.BundleAt(@"c:\Bundle");
                    modules.BundleInto("app.bundle.js");
                })
                .WithFileSystem(fileSystem)
                .AddFileHandler<BabelFilehandler>()
                .AddFileHandler<LessFileHandler>()
                .AddFileHandler<SassFileHandler>()
                .AddFileHandler<Base64FileHandler>()
                .AddFileHandler<TestFileHandler>()// Has dependency to Base64FileHandler
                .Sew();

            Assert.True(fileSystem.File.Exists(@"c:\Bundle\app.bundle.js"));
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\font.ttf"));
            Assert.True(fileSystem.File.ReadAllText(@"c:\Bundle\app.bundle.js").Contains("'font.ttf' : function(require, exports, module) "));
        }

        [Test]
        public void BuildCustomPackageCompilerTest()
        {
            IFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import './font.ttf'; exports.default = ()=>'Hello';") },
                { @"c:\App\font.ttf", new MockFileData("ttffilehere") }
            });
            Stitcher.Stitch
                .Modules((modules) =>
                {
                    modules.RootedAt(@"c:\App");
                    modules.EntryPoints(new[] { "./App" });
                    modules.CopyFiles(new[] { "./font.ttf" });
                }).Into((modules) =>
                {
                    modules.BundleAt(@"c:\Bundle");
                    modules.BundleInto("app.bundle.js");
                })
                .WithFileSystem(fileSystem)
                .AddFileHandler<BabelFilehandler>()
                .AddFileHandler<LessFileHandler>()
                .AddFileHandler<SassFileHandler>()
                .AddFileHandler<Base64FileHandler>()
                .WithPackageCompiler<MyPackageCompiler>() // custom PackageCompiler
                .Register<ICommentWriter, CommentWriter>() // MyPackageCompiler dependency: If any extra dependency then we must register them
                .Sew();

            Assert.True(fileSystem.File.Exists(@"c:\Bundle\app.bundle.js"));
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\font.ttf"));
            Assert.True(fileSystem.File.ReadAllText(@"c:\Bundle\app.bundle.js").Contains("'font.ttf' : function(require, exports, module) "));
            Assert.True(fileSystem.File.ReadAllText(@"c:\Bundle\app.bundle.js").Contains("// Comment added from MyPackageCompiler"));
        }

        [Test]
        public void BuildUsingConfigTest()
        {
            var container = new TinyIoC.TinyIoCContainer();
            Stitcher.Stitch
                .UsingContainer(container)
                .UsingConfig(new StitchConfig
                {
                    Packager = new PackagerConfig
                    {
                        RootPath = @"c:\App",
                        EntryPoints = new[] { "./App" },
                        CopyFiles = new[] { "./font.ttf" }
                    },
                    Compiler = new PackageCompilerConfig
                    {
                        DestinationDirectory = @"c:\Bundle",
                        BundleFileName = "app.bundle.js"
                    },
                    Extendibility = new PackagerExtendibilityConfig
                    {
                        DI = new Dictionary<string, string>
                        {
                            { "System.IO.Abstractions.IFileSystem, System.IO.Abstractions", "Urunium.Stitch.Tests.PackageBuilderTest+MyMockFileSystem, Urunium.Stitch.Tests" },
                            { "Urunium.Stitch.PackageCompiler, Urunium.Stitch", "Urunium.Stitch.Tests.PackageBuilderTest+MyPackageCompiler, Urunium.Stitch.Tests" },
                            { "Urunium.Stitch.Tests.PackageBuilderTest+ICommentWriter, Urunium.Stitch.Tests", "Urunium.Stitch.Tests.PackageBuilderTest+CommentWriter, Urunium.Stitch.Tests" }
                        },
                        FileHandlers = new List<string>
                        {
                            // default handlers
                            "Urunium.Stitch.FileHandlers.BabelFilehandler",
                            "Urunium.Stitch.FileHandlers.LessFileHandler",
                            "Urunium.Stitch.FileHandlers.SassFileHandler",
                            "Urunium.Stitch.FileHandlers.Base64FileHandler",
                            // extra handler
                            "Urunium.Stitch.Tests.PackageBuilderTest+TestFileHandler, Urunium.Stitch.Tests",
                        }
                    }
                }).Sew();

            var fileSystem = container.Resolve<IFileSystem>();
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\app.bundle.js"));
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\font.ttf"));
            Assert.True(fileSystem.File.ReadAllText(@"c:\Bundle\app.bundle.js").Contains("'font.ttf' : function(require, exports, module) "));
            Assert.True(fileSystem.File.ReadAllText(@"c:\Bundle\app.bundle.js").Contains("// Comment added from MyPackageCompiler"));
        }

        [Test]
        public void BuildUsingJsonConfigTest()
        {
            var container = new TinyIoC.TinyIoCContainer();
            var config = @"{
                'Packager': {
                    'RootPath': 'c:\\App',
                    'EntryPoints': ['./App'],
                    'CopyFiles': ['./font.ttf']
                },
                'Compiler': {
                    'DestinationDirectory': 'c:\\Bundle',
                    'BundleFileName': 'app.bundle.js'
                },
                'Extendibility': {
                    'DI': {
                      'System.IO.Abstractions.IFileSystem, System.IO.Abstractions': 'Urunium.Stitch.Tests.PackageBuilderTest+MyMockFileSystem, Urunium.Stitch.Tests',
                      'Urunium.Stitch.PackageCompiler, Urunium.Stitch': 'Urunium.Stitch.Tests.PackageBuilderTest+MyPackageCompiler, Urunium.Stitch.Tests',
                      'Urunium.Stitch.Tests.PackageBuilderTest+ICommentWriter, Urunium.Stitch.Tests': 'Urunium.Stitch.Tests.PackageBuilderTest+CommentWriter, Urunium.Stitch.Tests'
                    },
                    'FileHandlers': [
                            'Urunium.Stitch.FileHandlers.BabelFilehandler',
                            'Urunium.Stitch.FileHandlers.LessFileHandler',
                            'Urunium.Stitch.FileHandlers.SassFileHandler',
                            'Urunium.Stitch.FileHandlers.Base64FileHandler',
                            'Urunium.Stitch.Tests.PackageBuilderTest+TestFileHandler, Urunium.Stitch.Tests'
                    ]
                }
            }";
            Stitcher.Stitch
                .UsingContainer(container)
                .UsingJsonConfig(config).Sew();

            var fileSystem = container.Resolve<IFileSystem>();
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\app.bundle.js"));
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\font.ttf"));
            Assert.True(fileSystem.File.ReadAllText(@"c:\Bundle\app.bundle.js").Contains("'font.ttf' : function(require, exports, module) "));
            Assert.True(fileSystem.File.ReadAllText(@"c:\Bundle\app.bundle.js").Contains("// Comment added from MyPackageCompiler"));
        }

        [Test]
        public void WithFileSystemTest()
        {
            var container = new TinyIoC.TinyIoCContainer();
            Stitcher.Stitch
                .UsingContainer(container)
                .Modules((modules) =>
                {
                    modules.RootedAt(@"c:\App");
                    modules.EntryPoints(new[] { "./App" });
                    modules.CopyFiles(new[] { "./font.ttf" });
                }).Into((modules) =>
                {
                    modules.BundleAt(@"c:\Bundle");
                    modules.BundleInto("app.bundle.js");
                })
                .WithFileSystem<MyMockFileSystem>()
                .UseDefaultFileHandlers()
                .Sew();

            var fileSystem = container.Resolve<IFileSystem>();
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\app.bundle.js"));
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\font.ttf"));
        }

        public class TestFileHandler : IFileHandler
        {
            Base64FileHandler _base64Handler;
            public TestFileHandler(Base64FileHandler base64Handler)
            {
                _base64Handler = base64Handler;
            }

            public IEnumerable<string> Extensions => new[] { "ttf" };

            public string Build(string content, string fullModulePath, string moduleId)
            {
                return _base64Handler.Build(content, fullModulePath, moduleId);
            }
        }

        public class MyPackageCompiler : PackageCompiler
        {
            ICommentWriter _commentWriter;
            public MyPackageCompiler(IFileSystem filesystem, ICommentWriter commentWriter) : base(filesystem)
            {
                _commentWriter = commentWriter;
            }

            public override void Compile(Package package, string destinationDirectory, string bundleFileName = "bundle.js")
            {
                base.Compile(package, destinationDirectory, bundleFileName);
                string bundleJs = FileSystem.Path.GetFullPath(FileSystem.Path.Combine(destinationDirectory, bundleFileName));
                _commentWriter.WriteSingleLineComment(bundleJs, "Comment added from MyPackageCompiler");
            }
        }

        public interface ICommentWriter
        {
            void WriteSingleLineComment(string path, string comment);
        }

        public class CommentWriter : ICommentWriter
        {
            IFileSystem _filesystem;
            public CommentWriter(IFileSystem filesystem)
            {
                _filesystem = filesystem;
            }
            public void WriteSingleLineComment(string path, string comment)
            {
                _filesystem.File.AppendAllText(path, $"\n// {comment}");
            }
        }

        public class MyMockFileSystem : MockFileSystem
        {
            public MyMockFileSystem() : base(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import './font.ttf'; exports.default = ()=>'Hello';") },
                { @"c:\App\font.ttf", new MockFileData("ttffilehere") }
            })
            {

            }
        }
    }
}
