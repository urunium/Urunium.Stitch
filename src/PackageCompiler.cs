using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch
{
    /// <summary>
    /// Bundles all modules in Package into one js file, and saves to destination directory. 
    /// Also copies files marked for copying to destination directory
    /// </summary>
    public class PackageCompiler
    {
        /// <summary>
        /// File system where js and copy files will be saved.
        /// </summary>
        protected IFileSystem FileSystem { get; private set; }

        /// <summary>
        /// PackageCompiler ctor
        /// </summary>
        /// <param name="fileSystem"></param>
        public PackageCompiler(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        /// <summary>
        /// Bundles all modules in one js files, and copies all files in `package.Files` to a destination directory
        /// </summary>
        /// <param name="package"></param>
        /// <param name="destinationDirectory"></param>
        /// <param name="bundleFileName"></param>
        public virtual void Compile(Package package, string destinationDirectory, string bundleFileName = "bundle.js")
        {
            var Path = FileSystem.Path;
            var Directory = FileSystem.Directory;
            var File = FileSystem.File;
            foreach (var file in package.Files)
            {
                Directory.CreateDirectory(destinationDirectory);

                string sourceFilePath = Path.GetFullPath(Path.Combine(package.RootPath, file));
                string destinationFilePath = Path.GetFullPath(Path.Combine(destinationDirectory, file));
                if (!File.Exists(destinationFilePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));
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
                if (!File.Exists(bundleFilePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(bundleFilePath));
                    File.Create(bundleFilePath).Dispose();
                }
                File.WriteAllText(bundleFilePath, bundledContent);
            }
        }

        /// <summary>
        /// Creates final js content out of package.Modules
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        protected virtual string Bundle(Package package)
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
