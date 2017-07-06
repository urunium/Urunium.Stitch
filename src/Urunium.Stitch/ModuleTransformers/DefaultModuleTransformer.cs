using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch.ModuleTransformers
{
    public class DefaultModuleTransformer : IModuleTransformer
    {
        IFileSystem _fileSystem;

        public DefaultModuleTransformer(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> Extensions => new[] { "*" };

        public Module Transform(Module module)
        {
            string fullModulePath = module.FullPath;
            string moduleId = module.ModuleId;
            string content = module.TransformedContent ?? ReadFileContent(fullModulePath);

            module.OriginalContent = module.OriginalContent ?? content;
            module.TransformedContent = content;
            return module;
        }

        private string ReadFileContent(string fullModulePath)
        {
            string content;
            var binaryExtensions = ApacheMimeTypes.Apache.MimeTypes.Where(x => x.Value.StartsWith("image/")
                                                                              || x.Value.StartsWith("video/")
                                                                              || x.Value.StartsWith("audio/")
                                                                              || x.Value.StartsWith("application/x-font")).Select(x => "." + x.Key);

            if (binaryExtensions.Contains(_fileSystem.Path.GetExtension(fullModulePath).ToLower()))
            {
                content = _fileSystem.File.ReadAllText(fullModulePath, Encoding.Default);
            }
            else
            {
                content = _fileSystem.File.ReadAllText(fullModulePath);
            }

            return content;
        }
    }
}
