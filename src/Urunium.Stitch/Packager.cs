using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urunium.Stitch.ModuleTransformers;

namespace Urunium.Stitch
{
    public class Packager
    {
        ModuleFinder _moduleFinder;
        IFileSystem _fileSystem;
        IEnumerable<IModuleTransformer> _transformers;
        public Packager(IFileSystem fileSystem, IEnumerable<IModuleTransformer> transformers)
        {
            _fileSystem = fileSystem;
            _transformers = transformers;

            _moduleFinder = new ModuleFinder(_fileSystem, _transformers.SelectMany(x => x.Extensions));
        }

        public Package Package(SourceConfig packagerConfig)
        {
            string rootPath = packagerConfig.RootPath;
            Package package = new Package(rootPath);
            string currentPath = null;

            if (packagerConfig.Globals?.Count > 0)
            {
                foreach (string moduleId in packagerConfig.Globals.Keys)
                {
                    package.Modules.Add(new Module
                    {
                        ModuleId = moduleId,
                        TransformedContent = $"Object.defineProperty(exports, '__esModule', {{value: true}}); exports.default = {packagerConfig.Globals[moduleId]}"
                    });
                }
            }

            if (packagerConfig.EntryPoints?.Length > 0)
            {
                foreach (string entryPoint in packagerConfig.EntryPoints)
                {
                    ProcessModule(rootPath, package, entryPoint, currentPath);
                }
            }

            if (packagerConfig.CopyFiles?.Length > 0)
            {
                foreach (string copyFile in packagerConfig.CopyFiles)
                {
                    package.Files.Add(copyFile);
                }
            }
            return package;
        }

        private void ProcessRequire(string rootPath, Package package, Module module)
        {
            string currentPath = _fileSystem.Path.GetDirectoryName(module.FullPath);
            using (Microsoft.ClearScript.ScriptEngine engine = new Microsoft.ClearScript.V8.V8ScriptEngine())
            {
                Action<string> require = (entryPoint) =>
                {
                    ProcessModule(rootPath, package, entryPoint, currentPath);
                };
                engine.AddHostObject("require", new Action<string>(require));
                engine.Execute("(function(exports){try{" + module.TransformedContent + "}catch(e){}}({}));");
            }
        }

        private void ProcessModule(string rootPath, Package package, string entryPoint, string currentPath)
        {
            string fullModulePath = _moduleFinder.FindModulePath(entryPoint, rootPath, currentPath);
            string moduleId = CalculateModuleId(rootPath, fullModulePath, entryPoint);
            
            Module module = TransformModule(new Module
            {
                ModuleId = moduleId,
                FullPath = fullModulePath
            });

            package.Modules.Add(module);
            ProcessRequire(rootPath, package, package.Modules.Last());
        }

        private Module TransformModule(Module module)
        {
            var transformers = _transformers.Where(x => x.Extensions.Contains(_fileSystem.Path.GetExtension(module.FullPath).Substring(1))).Concat(Enumerable.Repeat(new DefaultModuleTransformer(_fileSystem), 1));

            foreach (var transformer in transformers)
            {
                module = transformer.Transform(module);
            }

            return module;
        }

        private string CalculateModuleId(string rootPath, string moduleFullPath, string actualReferencedModule)
        {
            bool hasExtension = _fileSystem.Path.GetExtension(actualReferencedModule) != "";
            var fileNameWithoutExtension = _fileSystem.Path.GetFileNameWithoutExtension(moduleFullPath);
            var calculatedModuleId = "";
            var modulePath = moduleFullPath.Replace("\\", "/");
            rootPath = rootPath.Replace("\\", "/");
            calculatedModuleId = modulePath.Replace(rootPath, "").TrimStart('/');
            if (!hasExtension)
            {
                calculatedModuleId = _fileSystem.Path.ChangeExtension(calculatedModuleId, null);
            }
            if (calculatedModuleId.EndsWith($"/{fileNameWithoutExtension}") && !actualReferencedModule.EndsWith($"/{fileNameWithoutExtension}"))//Path.GetFileNameWithoutExtension(moduleFullPath) == "index")
            {
                calculatedModuleId = calculatedModuleId.Replace($"/{fileNameWithoutExtension}", "/");
            }

            return calculatedModuleId;
        }
    }
}
