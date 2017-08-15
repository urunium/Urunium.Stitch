using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urunium.Stitch.ModuleTransformers;

namespace Urunium.Stitch.Tests
{
    [TestFixture]
    public class StitcherTests
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
                .From((source) =>
                {
                    source.RootPath = @"c:\App";
                    source.EntryPoints = new[] { "./App" };
                    source.CopyFiles = new[] { "./font.ttf" };
                }).Into((destination) =>
                {
                    destination.Directory = @"c:\Bundle";
                    destination.BundleFileName = "app.bundle.js";
                })
                .WithFileSystem(fileSystem)
                .AddTransformer<BabelModuleTransformer>()
                .AddTransformer<LessModuleTransformer>()
                .AddTransformer<SassModuleTransformer>()
                .AddTransformer<Base64ModuleTransformer>()
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
                .From((source) =>
                {
                    source.RootPath = @"c:\App";
                    source.EntryPoints = new[] { "./App" };
                    source.CopyFiles = new[] { "./font.ttf" };
                }).Into((destination) =>
                {
                    destination.Directory = @"c:\Bundle";
                    destination.BundleFileName = "app.bundle.js";
                })
                .WithFileSystem(fileSystem)
                .AddTransformer<BabelModuleTransformer>()
                .AddTransformer<LessModuleTransformer>()
                .AddTransformer<SassModuleTransformer>()
                .AddTransformer<Base64ModuleTransformer>()
                .AddTransformer<TestModuleTransformer>()// Has dependency to Base64ModuleTransformer
                .Sew();

            Assert.True(fileSystem.File.Exists(@"c:\Bundle\app.bundle.js"));
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\font.ttf"));
            Assert.True(fileSystem.File.ReadAllText(@"c:\Bundle\app.bundle.js").Contains("'font.ttf' : function(require, exports, module) "));
        }

        [Test]
        public void BuildCustomPackageBundlerTest()
        {
            IFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\App\App.js", new MockFileData("import './font.ttf'; exports.default = ()=>'Hello';") },
                { @"c:\App\font.ttf", new MockFileData("ttffilehere") }
            });
            Stitcher.Stitch
                .From((source) =>
                {
                    source.RootPath = @"c:\App";
                    source.EntryPoints = new[] { "./App" };
                    source.CopyFiles = new[] { "./font.ttf" };
                }).Into((destination) =>
                {
                    destination.Directory = @"c:\Bundle";
                    destination.BundleFileName = "app.bundle.js";
                })
                .WithFileSystem(fileSystem)
                .AddTransformer<BabelModuleTransformer>()
                .AddTransformer<LessModuleTransformer>()
                .AddTransformer<SassModuleTransformer>()
                .AddTransformer<Base64ModuleTransformer>()
                .WithPackageBundler<MyPackageBundler>() // custom PackageBundler
                .Register<ICommentWriter, CommentWriter>() // MyPackageBundler dependency: If any extra dependency then we must register them
                .Sew();

            Assert.True(fileSystem.File.Exists(@"c:\Bundle\app.bundle.js"));
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\font.ttf"));
            Assert.True(fileSystem.File.ReadAllText(@"c:\Bundle\app.bundle.js").Contains("'font.ttf' : function(require, exports, module) "));
            Assert.True(fileSystem.File.ReadAllText(@"c:\Bundle\app.bundle.js").Contains("// Comment added from MyPackageBundler"));
        }

        [Test]
        public void BuildUsingConfigTest()
        {
            var container = new Urunium.Stitch.TinyIoC.TinyIoCContainer();
            Stitcher.Stitch
                .UsingContainer(container)
                .UsingConfig(new StitchConfig
                {
                    From = new SourceConfig
                    {
                        RootPath = @"c:\App",
                        EntryPoints = new[] { "./App" },
                        CopyFiles = new[] { "./font.ttf" }
                    },
                    Into = new DestinationConfig
                    {
                        Directory = @"c:\Bundle",
                        BundleFileName = "app.bundle.js"
                    },
                    Extendibility = new PackagerExtendibilityConfig
                    {
                        DI = new Dictionary<string, string>
                        {
                            { "System.IO.Abstractions.IFileSystem, System.IO.Abstractions", "Urunium.Stitch.Tests.StitcherTests+MyMockFileSystem, Urunium.Stitch.Tests" },
                            { "Urunium.Stitch.PackageBundler, Urunium.Stitch", "Urunium.Stitch.Tests.StitcherTests+MyPackageBundler, Urunium.Stitch.Tests" },
                            { "Urunium.Stitch.Tests.StitcherTests+ICommentWriter, Urunium.Stitch.Tests", "Urunium.Stitch.Tests.StitcherTests+CommentWriter, Urunium.Stitch.Tests" }
                        },
                        Transformers = new List<string>
                        {
                            // default transformers
                            "Urunium.Stitch.ModuleTransformers.BabelModuleTransformer",
                            "Urunium.Stitch.ModuleTransformers.LessModuleTransformer",
                            "Urunium.Stitch.ModuleTransformers.SassModuleTransformer",
                            "Urunium.Stitch.ModuleTransformers.Base64ModuleTransformer",
                            // extra transformers
                            "Urunium.Stitch.Tests.StitcherTests+TestModuleTransformer, Urunium.Stitch.Tests",
                        }
                    }
                }).Sew();

            var fileSystem = container.Resolve<IFileSystem>();
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\app.bundle.js"));
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\font.ttf"));
            Assert.True(fileSystem.File.ReadAllText(@"c:\Bundle\app.bundle.js").Contains("'font.ttf' : function(require, exports, module) "));
            Assert.True(fileSystem.File.ReadAllText(@"c:\Bundle\app.bundle.js").Contains("// Comment added from MyPackageBundler"));
        }

        [Test]
        public void BuildUsingJsonConfigTest()
        {
            var container = new Urunium.Stitch.TinyIoC.TinyIoCContainer();
            var config = @"{
                'From': {
                    'RootPath': 'c:\\App',
                    'EntryPoints': ['./App'],
                    'CopyFiles': ['./font.ttf']
                },
                'Into': {
                    'Directory': 'c:\\Bundle',
                    'BundleFileName': 'app.bundle.js'
                },
                'Extendibility': {
                    'DI': {
                      'System.IO.Abstractions.IFileSystem, System.IO.Abstractions': 'Urunium.Stitch.Tests.StitcherTests+MyMockFileSystem, Urunium.Stitch.Tests',
                      'Urunium.Stitch.PackageBundler, Urunium.Stitch': 'Urunium.Stitch.Tests.StitcherTests+MyPackageBundler, Urunium.Stitch.Tests',
                      'Urunium.Stitch.Tests.StitcherTests+ICommentWriter, Urunium.Stitch.Tests': 'Urunium.Stitch.Tests.StitcherTests+CommentWriter, Urunium.Stitch.Tests'
                    },
                    'Transformers': [
                            'Urunium.Stitch.ModuleTransformers.BabelModuleTransformer',
                            'Urunium.Stitch.ModuleTransformers.LessModuleTransformer',
                            'Urunium.Stitch.ModuleTransformers.SassModuleTransformer',
                            'Urunium.Stitch.ModuleTransformers.Base64ModuleTransformer',
                            'Urunium.Stitch.Tests.StitcherTests+TestModuleTransformer, Urunium.Stitch.Tests'
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
            Assert.True(fileSystem.File.ReadAllText(@"c:\Bundle\app.bundle.js").Contains("// Comment added from MyPackageBundler"));
        }

        [Test]
        public void BuildUsingJsonConfigWithoutSpecifyingContainer()
        {
            var json = @"{
  'From': {
    'CopyFiles': [],
    'EntryPoints': [ './App' ],
    'Globals': {
      'react': 'React',
      'react-dom': 'ReactDOM'
    },
    'RootPath': '$(ProjectDir)'
  },
  'Into': {
    'BundleFileName': 'app.js',
    'Directory': '$(OutDir)Views'
  },
  'Extendibility': {
    'DI': {
    },
    'Transformers': [

    ]
  }
}";
            json = json.Replace("$(ProjectDir)", "c:\\\\App").Replace("$(OutDir)", "c:\\\\Bundle\\\\");
            MyMockFileSystem fileSystem = new MyMockFileSystem();
            Stitcher.Stitch.UsingJsonConfig(json).WithFileSystem(fileSystem).Sew();
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\Views\app.js"));
        }

        [Test]
        public void WithFileSystemTest()
        {
            var container = new Urunium.Stitch.TinyIoC.TinyIoCContainer();
            Stitcher.Stitch
                .UsingContainer(container)
                .From((source) =>
                {
                    source.RootPath = @"c:\App";
                    source.EntryPoints = new[] { "./App" };
                    source.CopyFiles = new[] { "./font.ttf" };
                }).Into((destination) =>
                {
                    destination.Directory = @"c:\Bundle";
                    destination.BundleFileName = "app.bundle.js";
                })
                .WithFileSystem<MyMockFileSystem>()
                .UsingDefaultTransformers()
                .Sew();

            var fileSystem = container.Resolve<IFileSystem>();
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\app.bundle.js"));
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\font.ttf"));
        }

        [Test]
        public void GlobalModulesTest()
        {
            var container = new Urunium.Stitch.TinyIoC.TinyIoCContainer();
            Stitcher.Stitch
                .UsingContainer(container)
                .From((source) =>
                {
                    source.RootPath = @"c:\App";
                    source.EntryPoints = new[] { "./App" };
                    source.CopyFiles = new[] { "./font.ttf" };
                    source.Globals = new GlobalsConfig
                    {
                        ["react"] = "React",
                        ["react-dom"] = "ReactDOM"
                    };
                }).Into((destination) =>
                {
                    destination.Directory = @"c:\Bundle";
                    destination.BundleFileName = "app.bundle.js";
                })
                .WithFileSystem<MyMockFileSystem>()
                .UsingDefaultTransformers()
                .Sew();

            var fileSystem = container.Resolve<IFileSystem>();
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\app.bundle.js"));
            Assert.True(fileSystem.File.Exists(@"c:\Bundle\font.ttf"));
        }

        public class TestModuleTransformer : IModuleTransformer
        {
            Base64ModuleTransformer _base64Transformer;
            public TestModuleTransformer(Base64ModuleTransformer base64Transformer)
            {
                _base64Transformer = base64Transformer;
            }

            public IEnumerable<string> Extensions => new[] { "ttf" };
            
            public Module Transform(Module module)
            {
                _base64Transformer.Transform(module);
                return module;
            }
        }

        public class MyPackageBundler : PackageBundler
        {
            ICommentWriter _commentWriter;
            public MyPackageBundler(IFileSystem filesystem, ICommentWriter commentWriter) : base(filesystem)
            {
                _commentWriter = commentWriter;
            }

            public override void CreateBundle(Package package, string destinationDirectory, string bundleFileName = "bundle.js")
            {
                base.CreateBundle(package, destinationDirectory, bundleFileName);
                string bundleJs = FileSystem.Path.GetFullPath(FileSystem.Path.Combine(destinationDirectory, bundleFileName));
                _commentWriter.WriteSingleLineComment(bundleJs, "Comment added from MyPackageBundler");
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
