using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urunium.Stitch.FileHandlers;

namespace Urunium.Stitch
{
    public class PackageBuilder
    {
        TinyIoC.TinyIoCContainer _container = TinyIoC.TinyIoCContainer.Current;
        private PackageCompilerConfig _packageCompilerConfig;
        private PackagerConfig _packagerConfig;
        private List<Type> _fileHandlerTypes = new List<Type>();
        private bool _useDefaultFileHandlers = true;

        private PackageBuilder()
        {

        }

        public static PackageBuilder Package => new PackageBuilder();

        public void BuildUsingJsonConfig(string json, IFileSystem fileSystem = null)
        {
            BuildUsingConfig(Newtonsoft.Json.JsonConvert.DeserializeObject<StitchConfig>(json), fileSystem);
        }

        public void BuildUsingConfig(StitchConfig config, IFileSystem fileSystem = null)
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

            if (fileSystem != null)
            {
                WithFileSystem(fileSystem);
            }

            Build();
        }

        public PackageBuilder WithFileSystem(IFileSystem fileSystem)
        {
            _container.Register(fileSystem);
            return this;
        }

        public PackageBuilder WithFileSystem<TFileSystem>() where TFileSystem : class, IFileSystem
        {
            _container.Register<IFileSystem, TFileSystem>();
            return this;
        }

        public PackageBuilder WithPackageCompiler<TPackageCompiler>() where TPackageCompiler : PackageCompiler
        {
            _container.Register<PackageCompiler, TPackageCompiler>();
            return this;
        }

        public PackageBuilder Register<RegisterType, RegisterImplementation>()
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            _container.Register<RegisterType, RegisterImplementation>();
            return this;
        }
        
        public PackageBuilder UsePackagerConfig(PackagerConfig packagerConfig)
        {
            _packagerConfig = packagerConfig;
            return this;
        }

        public PackageBuilder UseCompilerConfig(PackageCompilerConfig packageCompilerConfig)
        {
            _packageCompilerConfig = packageCompilerConfig;
            return this;
        }

        public PackageBuilder UseDefaultFileHandlers()
        {
            _useDefaultFileHandlers = true;
            return this;
        }

        public PackageBuilder UseDefaultFileSystem()
        {
            _container.Register<IFileSystem, FileSystem>().AsSingleton();
            return this;
        }

        private PackageBuilder UseDefaultPackageCompiler()
        {
            return WithPackageCompiler<PackageCompiler>();
        }

        public PackageBuilder AddFileHandler<THandler>() where THandler : class, IFileHandler

        {
            _useDefaultFileHandlers = false;
            _container.Register<THandler, THandler>();
            _fileHandlerTypes.Add(typeof(THandler));
            return this;
        }

        public void Build()
        {
            IFileSystem fileSystem;
            if (!_container.CanResolve<IFileSystem>())
            {
                UseDefaultFileSystem();
            }

            if (!_container.CanResolve<PackageCompiler>())
            {
                UseDefaultPackageCompiler();
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
    }
}
