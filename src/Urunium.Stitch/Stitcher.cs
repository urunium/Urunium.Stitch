using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urunium.Stitch.ModuleTransformers;

namespace Urunium.Stitch
{
    public class Stitcher
    {
        private Urunium.Stitch.TinyIoC.TinyIoCContainer _container;
        private DestinationConfig _destinationConfig;
        private SourceConfig _sourceConfig;
        private List<Type> _fileHandlerTypes = new List<Type>();
        private bool _useDefaultFileHandlers = true;

        private Stitcher()
        {
        }

        public static SourceConfigurator Stitch => new SourceConfigurator(new Stitcher());

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

        public Stitcher WithPackageBundler<TPackageBundler>() where TPackageBundler : PackageBundler
        {
            _container.Register<PackageBundler, TPackageBundler>();
            return this;
        }

        public Stitcher Register<RegisterType, RegisterImplementation>(Action<Urunium.Stitch.TinyIoC.TinyIoCContainer.RegisterOptions> callback = null)
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            var options = _container.Register<RegisterType, RegisterImplementation>();
            callback?.Invoke(options);
            return this;
        }

        public Stitcher UsingDefaultFileHandlers()
        {
            _useDefaultFileHandlers = true;
            return this;
        }

        public Stitcher UsingDefaultFileSystem()
        {
            _container.Register<IFileSystem, FileSystem>().AsSingleton();
            return this;
        }

        private Stitcher UsingDefaultPackageBundler()
        {
            return WithPackageBundler<PackageBundler>();
        }

        public Stitcher AddFileHandler<THandler>() where THandler : class, IModuleTransformer

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

            if (!_container.CanResolve<PackageBundler>())
            {
                UsingDefaultPackageBundler();
            }

            if (_useDefaultFileHandlers)
            {
                _fileHandlerTypes.Clear();
                AddFileHandler<BabelModuleTransformer>()
                    .AddFileHandler<LessModuleTransformer>()
                    .AddFileHandler<SassModuleTransformer>()
                    .AddFileHandler<Base64ModuleTransformer>();
            }

            fileSystem = _container.Resolve<IFileSystem>();

            var packager = new Packager(fileSystem, _fileHandlerTypes.Select(x => _container.Resolve(x)).Cast<IModuleTransformer>().ToArray());

            var package = packager.Package(_sourceConfig);

            PackageBundler bundler = _container.Resolve<PackageBundler>();
            bundler.CreateBundle(package, _destinationConfig.Directory, _destinationConfig.BundleFileName);
        }

        private Stitcher UsingSourceConfig(SourceConfig packagerConfig)
        {
            _sourceConfig = packagerConfig;
            return this;
        }

        private Stitcher UsingDestinationConfig(DestinationConfig destinationConfig)
        {
            _destinationConfig = destinationConfig;
            return this;
        }

        private Stitcher UsingJsonConfig(string json)
        {
            return UsingConfig(Newtonsoft.Json.JsonConvert.DeserializeObject<StitchConfig>(json));
        }

        private Stitcher UsingConfig(StitchConfig config)
        {
            if (config.Into == null)
            {
                throw new Exception("Compiler config is required");
            }

            if (string.IsNullOrWhiteSpace(config.Into.Directory))
            {
                throw new Exception("DestinationDirectory is required");
            }

            if (config.From == null)
            {
                throw new Exception("Packager config is required");
            }

            if (string.IsNullOrWhiteSpace(config.From.RootPath))
            {
                throw new Exception("RootPath is required");
            }

            if (((config.From.EntryPoints?.Length ?? 0) == 0) && (config.From.CopyFiles?.Length ?? 0) == 0)
            {
                throw new Exception("EntryPoints or CopyFiles is required");
            }

            _sourceConfig = config.From;
            _destinationConfig = config.Into;

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

        public class SourceConfigurator
        {
            Stitcher _stitcher;
            internal SourceConfigurator(Stitcher stitcher)
            {
                _stitcher = stitcher;
            }

            public SourceConfigurator UsingContainer(Urunium.Stitch.TinyIoC.TinyIoCContainer container)
            {
                _stitcher._container = container;
                return this;
            }

            public DestinationConfigurator From(Action<SourceConfig> configuratorCallback)
            {
                if (_stitcher._container == null)
                {
                    _stitcher._container = new Urunium.Stitch.TinyIoC.TinyIoCContainer();
                }

                var config = new SourceConfig();
                configuratorCallback(config);
                _stitcher.UsingSourceConfig(config);
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
        
        public class DestinationConfigurator
        {
            Stitcher _stitcher;
            internal DestinationConfigurator(Stitcher stitcher)
            {
                _stitcher = stitcher;
            }

            public Stitcher Into(Action<DestinationConfig> configuratorCallback)
            {
                var config = new DestinationConfig();
                configuratorCallback(config);
                _stitcher.UsingDestinationConfig(config);
                return _stitcher;
            }
        }
    }
}
