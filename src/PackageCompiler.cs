using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch
{
    public class PackageCompiler
    {
        private IFileSystem _fileSystem;

        public PackageCompiler(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void Compile(Package package, string destinationDirectory, string bundleFileName = "bundle.js")
        {
            var Path = _fileSystem.Path;
            var Directory = _fileSystem.Directory;
            var File = _fileSystem.File;
            foreach (var file in package.Files)
            {
                Directory.CreateDirectory(destinationDirectory);

                string sourceFilePath = Path.GetFullPath(Path.Combine(package.RootPath, file));
                string destinationFilePath = Path.GetFullPath(Path.Combine(destinationDirectory, file));
                if (!File.Exists(destinationFilePath))
                {
                    File.Create(destinationFilePath).Dispose();
                }
                File.Copy(sourceFilePath, destinationFilePath, true);
            }

            if (package.Modules.Count > 0)
            {
                string bundledContent = Bundle(package);
                string moduleId = package.Modules[0].ModuleId;
                string bundleFilePath = Path.Combine(Path.GetDirectoryName(moduleId), bundleFileName);
                bundleFilePath = Path.GetFullPath(Path.Combine(destinationDirectory, bundleFilePath));
                File.WriteAllText(bundleFilePath, bundledContent);
            }
        }

        private string Bundle(Package package)
        {
            StringBuilder modulesCollector = new StringBuilder();
            modulesCollector.AppendLine("var modules = {");
            string comma = "";
            foreach (var module in package.Modules)
            {
                modulesCollector.Append(comma);
                modulesCollector.Append($"'{module.ModuleId}' : function(require, exports, module) ");
                modulesCollector.AppendLine("{");
                modulesCollector.AppendLine(module.TransformedContent);
                modulesCollector.AppendLine("}");
                comma = ",";
            }
            modulesCollector.AppendLine("};");

            StringBuilder stitchIt = new StringBuilder(ResourceReader.Read("stitchIt.js"));
            stitchIt.Replace("__MODULES__", modulesCollector.ToString());
            return stitchIt.ToString();
        }
    }
}
