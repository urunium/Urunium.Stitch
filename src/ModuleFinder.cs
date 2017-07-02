using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch
{
    public class ModuleFinder
    {
        IFileSystem _fileSystem;
        IEnumerable<string> _supportedExtensions;
        public ModuleFinder(IFileSystem fileSystem, IEnumerable<string> supportedExtensions)
        {
            _fileSystem = fileSystem;
            _supportedExtensions = supportedExtensions;
        }

        public string FindModulePath(string moduleId, string rootPath, string currentPath)
        {
            currentPath = currentPath ?? rootPath;
            string moduleFullPath = "";
            if (moduleId.StartsWith("./") || moduleId.StartsWith("../") || moduleId.StartsWith("/"))//If X begins with './' or '/' or '../'
            {
                moduleFullPath = FindLocalModule(currentPath, moduleId);
            }
            else if (_fileSystem.Directory.Exists(_fileSystem.Path.Combine(rootPath, "node_modules", moduleId)))
            {
                moduleFullPath = FindNodeModule(rootPath, moduleId);
            }

            return moduleFullPath;
        }

        private string FindLocalModule(string currentPath, string moduleId)
        {
            return LoadAsFile(currentPath, moduleId) ?? LoadAsDirectory(currentPath, moduleId);
        }

        private string LoadAsDirectory(string dir, string module)
        {
            Func<string> loadAsPackage = () =>
            {
                var packageJsonPath = _fileSystem.Path.GetFullPath(_fileSystem.Path.Combine(dir, module, "package.json"));
                if (_fileSystem.File.Exists(packageJsonPath))
                {
                    var packageJson = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(_fileSystem.File.ReadAllText(packageJsonPath), new { main = "" });
                    if (packageJson.main != null)
                    {
                        return LoadAsFile(_fileSystem.Path.Combine(dir, module), packageJson.main) ?? LoadIndex(_fileSystem.Path.Combine(dir, module), packageJson.main);
                    }
                }
                return null;
            };

            return loadAsPackage() ?? LoadIndex(dir, module);
        }

        private string LoadIndex(string dir, string module)
        {
            return LoadAsFile(_fileSystem.Path.Combine(dir, module), "index");
        }

        private string LoadAsFile(string dir, string module)
        {
            var file = _fileSystem.Path.GetFullPath(_fileSystem.Path.Combine(dir, module));

            return Enumerable.Repeat(file, 1).Concat(_supportedExtensions.Select(x => file + "." + x)).Where(_fileSystem.File.Exists).FirstOrDefault();
        }

        private string FindNodeModule(string rootPath, string moduleId)
        {
            /*LOAD_NODE_MODULES(X, START)
1. let DIRS=NODE_MODULES_PATHS(START)
2. for each DIR in DIRS:
   a. LOAD_AS_FILE(DIR/X)
   b. LOAD_AS_DIRECTORY(DIR/X)

NODE_MODULES_PATHS(START)
1. let PARTS = path split(START)
2. let I = count of PARTS - 1
3. let DIRS = []
4. while I >= 0,
   a. if PARTS[I] = "node_modules" CONTINUE
   b. DIR = path join(PARTS[0 .. I] + "node_modules")
   c. DIRS = DIRS + DIR
   d. let I = I - 1
5. return DIRS
*/
            var nodeModulePath = _fileSystem.Path.GetFullPath(_fileSystem.Path.Combine(rootPath, "node_modules"));
            return LoadAsFile(nodeModulePath, moduleId) ?? LoadAsDirectory(nodeModulePath, moduleId);
        }
    }
}
