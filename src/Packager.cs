using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch
{
    public class Packager
    {
        ModuleFinder _moduleFinder;
        IFileSystem _fileSystem;
        IEnumerable<IFileHandler> _handlers;
        public Packager(IFileSystem fileSystem, IEnumerable<IFileHandler> handlers)
        {
            _fileSystem = fileSystem;
            _handlers = handlers;

            _moduleFinder = new ModuleFinder(_fileSystem, _handlers.SelectMany(x => x.Extensions));
        }

        public Package Package(PackagerConfig packagerConfig)
        {
            string rootPath = packagerConfig.RootPath;
            Package package = new Package();
            string currentPath = null;
            foreach (string entryPoint in packagerConfig.EntryPoints)
            {
                string fullModulePath = _moduleFinder.FindModulePath(entryPoint, rootPath, currentPath);
                string moduleId = CalculateModuleId(rootPath, fullModulePath, entryPoint);
                string content = _fileSystem.File.ReadAllText(fullModulePath);
                var handler = _handlers.Where(x => x.Extensions.Contains(_fileSystem.Path.GetExtension(fullModulePath).Substring(1))).FirstOrDefault();
                package.Modules.Add(new Module
                {
                    ModuleId = moduleId,
                    FullPath = fullModulePath,
                    OriginalContent = content,
                    TransformedContent = handler?.Build(content, fullModulePath, moduleId)
                });
                ProcessRequire(rootPath, package, package.Modules.Last());
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
                    string fullModulePath = _moduleFinder.FindModulePath(entryPoint, rootPath, currentPath);
                    string moduleId = CalculateModuleId(rootPath, fullModulePath, entryPoint);
                    string content = _fileSystem.File.ReadAllText(fullModulePath);
                    var handler = _handlers.Where(x => x.Extensions.Contains(_fileSystem.Path.GetExtension(fullModulePath).Substring(1))).FirstOrDefault();
                    package.Modules.Add(new Module
                    {
                        ModuleId = moduleId,
                        FullPath = fullModulePath,
                        OriginalContent = content,
                        TransformedContent = handler?.Build(content, fullModulePath, moduleId)
                    });
                    ProcessRequire(rootPath, package, package.Modules.Last());
                };
                engine.AddHostObject("require", new Action<string>(require));
                engine.Execute("(function(exports){try{" + module.TransformedContent + "}catch(e){}}({}));");
            }
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
