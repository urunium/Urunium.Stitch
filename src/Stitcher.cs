using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urunium.Stitch.FileHandlers;

namespace Urunium.Stitch
{
    public class Stitcher
    {
        private TinyIoC.TinyIoCContainer _container;
        private PackageCompilerConfig _packageCompilerConfig;
        private PackagerConfig _packagerConfig;
        private List<Type> _fileHandlerTypes = new List<Type>();
        private bool _useDefaultFileHandlers = true;

        private Stitcher()
        {
        }

        public static ModuleConfigurator Stitch => new ModuleConfigurator(new Stitcher());
        
        public Stitcher WithFileSystem(IFileSystem fileSystem)
        {
            _container.Register(fileSystem);
            return this;
        }

        public Stitcher WithFileSystem<TFileSystem>() where TFileSystem : class, IFileSystem
        {
            _container.Register<IFileSystem, TFileSystem>();
            return this;
        }

        public Stitcher WithPackageCompiler<TPackageCompiler>() where TPackageCompiler : PackageCompiler
        {
            _container.Register<PackageCompiler, TPackageCompiler>();
            return this;
        }

        public Stitcher Register<RegisterType, RegisterImplementation>()
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            _container.Register<RegisterType, RegisterImplementation>();
            return this;
        }

        public Stitcher UseDefaultFileHandlers()
        {
            _useDefaultFileHandlers = true;
            return this;
        }

        public Stitcher UsingDefaultFileSystem()
        {
            _container.Register<IFileSystem, FileSystem>().AsSingleton();
            return this;
        }

        private Stitcher UsingDefaultPackageCompiler()
        {
            return WithPackageCompiler<PackageCompiler>();
        }

        public Stitcher AddFileHandler<THandler>() where THandler : class, IFileHandler

        {
            _useDefaultFileHandlers = false;
            _container.Register<THandler, THandler>();
            _fileHandlerTypes.Add(typeof(THandler));
            return this;
        }
        
        public void Sew()
        {
            IFileSystem fileSystem;
            if (!_container.CanResolve<IFileSystem>())
            {
                UsingDefaultFileSystem();
            }

            if (!_container.CanResolve<PackageCompiler>())
            {
                UsingDefaultPackageCompiler();
            }

            if (_useDefaultFileHandlers)
            {
                _fileHandlerTypes.Clear();
                AddFileHandler<BabelFilehandler>()
                    .AddFileHandler<LessFileHandler>()
                    .AddFileHandler<SassFileHandler>()
                    .AddFileHandler<Base64FileHandler>();
            }

            fileSystem = _container.Resolve<IFileSystem>();

            var packager = new Packager(fileSystem, _fileHandlerTypes.Select(x => _container.Resolve(x)).Cast<IFileHandler>().ToArray());

            var package = packager.Package(_packagerConfig);

            PackageCompiler compiler = _container.Resolve<PackageCompiler>();
            compiler.Compile(package, _packageCompilerConfig.DestinationDirectory, _packageCompilerConfig.BundleFileName);
        }

        private Stitcher UsingPackagerConfig(PackagerConfig packagerConfig)
        {
            _packagerConfig = packagerConfig;
            return this;
        }

        private Stitcher UsingPackageCompilerConfig(PackageCompilerConfig packageCompilerConfig)
        {
            _packageCompilerConfig = packageCompilerConfig;
            return this;
        }

        private Stitcher UsingJsonConfig(string json)
        {
            return UsingConfig(Newtonsoft.Json.JsonConvert.DeserializeObject<StitchConfig>(json));
        }

        private Stitcher UsingConfig(StitchConfig config)
        {
            if (config.Compiler == null)
            {
                throw new Exception("Compiler config is required");
            }

            if (string.IsNullOrWhiteSpace(config.Compiler.DestinationDirectory))
            {
                throw new Exception("DestinationDirectory is required");
            }

            if (config.Packager == null)
            {
                throw new Exception("Packager config is required");
            }

            if (string.IsNullOrWhiteSpace(config.Packager.RootPath))
            {
                throw new Exception("RootPath is required");
            }

            if (((config.Packager.EntryPoints?.Length ?? 0) == 0) && (config.Packager.CopyFiles?.Length ?? 0) == 0)
            {
                throw new Exception("EntryPoints or CopyFiles is required");
            }

            _packagerConfig = config.Packager;
            _packageCompilerConfig = config.Compiler;

            if (config.Extendibility != null)
            {
                if (config.Extendibility.DI != null)
                {
                    foreach (var diEntry in config.Extendibility.DI)
                    {
                        Type registerType = Type.GetType(diEntry.Key, true);
                        Type implementationType = Type.GetType(diEntry.Value, true);
                        _container.Register(registerType, implementationType);
                    }
                }
                if (config.Extendibility.FileHandlers?.Count > 0)
                {
                    _useDefaultFileHandlers = false;
                    _fileHandlerTypes.Clear();
                    foreach (var fileHandlerTypeName in config.Extendibility.FileHandlers)
                    {
                        Type fileHandlerType = Type.GetType(fileHandlerTypeName, true);
                        _fileHandlerTypes.Add(fileHandlerType);
                    }
                }
            }

            return this;
        }

        public class ModuleConfigurator
        {
            Stitcher _stitcher;
            internal ModuleConfigurator(Stitcher stitcher)
            {
                _stitcher = stitcher;
            }

            public ModuleConfigurator UsingContainer(TinyIoC.TinyIoCContainer container)
            {
                _stitcher._container = container;
                return this;
            }

            public DestinationConfigurator Modules(Action<ModuleConfigurationBuilder> configuratorCallback)
            {
                if (_stitcher._container == null)
                {
                    _stitcher._container = new TinyIoC.TinyIoCContainer();
                }

                var configBuilder = new ModuleConfigurationBuilder();
                configuratorCallback(configBuilder);
                _stitcher.UsingPackagerConfig(configBuilder.PackagerConfig);
                return new DestinationConfigurator(_stitcher);
            }

            public Stitcher UsingConfig(StitchConfig stitchConfig)
            {
                return _stitcher.UsingConfig(stitchConfig);
            }

            public Stitcher UsingJsonConfig(string config)
            {
                return _stitcher.UsingJsonConfig(config);
            }
        }

        public class ModuleConfigurationBuilder
        {
            public PackagerConfig PackagerConfig { get; set; } = new PackagerConfig();

            public void RootedAt(string rootPath)
            {
                PackagerConfig.RootPath = rootPath;
            }

            public void EntryPoints(string[] entryPoints)
            {
                PackagerConfig.EntryPoints = entryPoints;
            }

            public void CopyFiles(string[] copyFiles)
            {
                PackagerConfig.CopyFiles = copyFiles;
            }

            public void WithGlobalModules(List<ValueTuple<string, string>> globals)
            {
                PackagerConfig.Globals = globals.ToDictionary(t => t.Item1, t => t.Item2);
            }
        }

        public class DestinationConfigurator
        {
            Stitcher _stitcher;
            internal DestinationConfigurator(Stitcher stitcher)
            {
                _stitcher = stitcher;
            }

            public Stitcher Into(Action<DestinationConfigurationBuilder> configuratorCallback)
            {
                var configBuilder = new DestinationConfigurationBuilder();
                configuratorCallback(configBuilder);
                _stitcher.UsingPackageCompilerConfig(configBuilder.Config);
                return _stitcher;
            }
        }

        public class DestinationConfigurationBuilder
        {
            public PackageCompilerConfig Config { get; private set; } = new PackageCompilerConfig();

            public void BundleAt(string destinationFolder)
            {
                Config.DestinationDirectory = destinationFolder;
            }

            public void BundleInto(string bundleFileName)
            {
                Config.BundleFileName = bundleFileName;
            }
        }
    }
}
