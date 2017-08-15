# Urunium.Stitch
Stitch together: js modules, css, images etc. in commonjs. Inspired by https://github.com/davidpadbury/StitchIt

# Goal
Goal of this project is to provide [webpack](https://webpack.github.io/) like capability, to bundle various resources into single commonjs resource.

## Getting started

In near future (nuget package is not available yet!):
```
Install-Package Urunium-Stitch
```
Or,
```
Install-Package Urunium-Stitch.Msbuild
```

Most likely all you'd need `Urunium-Stitch.Msbuild` package which integrates with the msbuild process and can be configured using [Json based](#usingjsonconfig) config. For more flexibility or extending capabalities use `Urunium-Stitch` package. This provides code based approach which needs to be written into appropriate place, as per project's need and architecture.

## Brief look at Usage:

```c#
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
```
Outermost API is the `Stitcher` class that provides fluent-api to configure and run the urunium-stitch process. Following are the parts configured in above example:

* From : In above example we are configuring the `RootPath`, `EntryPoints` and `CopyFiles` properties of `SourceConfig`. 
  - RootPath as name suggests is the location from where files search need to be based on.
  - EntryPoints configure the items which needs to be bundled. Bundling happens recursively for each entry-point item. But all the modules will remain in single file, irrespective of number of entry points.
  - CopyFiles denote the files that needs to be copied to the output. Generally these are the resources that cannot be bundled into CommonJs modules but are part of final product.
  
* Into : Configures the destination. It has two properties: `Directory`, which is the location where bundled file must be created, and `BundleFileName`

* AddTransformer : Adds an `IModuleTransformer` into the pipeline. Following transformers are available:
  - BabelModuleTransformer
  - Base64ModuleTransformer
  - DefaultModuleTransformer
  - LessModuleTransformer
  - SassModuleTransformer
  - TypescriptModuleTransformer
  
The sequence in which transformers are added may have some impact on the output. The default sequence is as follows:
  - BabelModuleTransformer
  - TypescriptModuleTransformer
  - LessModuleTransformer
  - SassModuleTransformer
  - Base64ModuleTransformer
  
Instead of adding these transformers we can simple call `UsingDefaultTransformers`

* Sew : Finally `Sew` method actually starts the bundling.

## Global modules

```c#
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
```
While configuring the source, we can specify global modules. Global modules will not be bundled into the final bundle, but will be assumed to be available in global context.

## UsingConfig

```c#
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
```
As shown above instead of configuring each parts individually, we can also configure in one scoop using `StitchConfig` and `UsingConfig` method.

## UsingJsonConfig

Json config is also supported, as follows:

```c#
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
```

# Interested in contributing?
Fork the project from github and send PR. Only tested using Visual studio 2017, but 2015 may work as well.
Execute `build.bat` or `build.ps1`. Final nuget packages are created at `artifact` folder.
