using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch.ModuleTransformers
{
    public class Base64ModuleTransformer : IModuleTransformer
    {
        IFileSystem _fileSystem;

        public Base64ModuleTransformer(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> Extensions => new[] { "png", "gif", "jpg" };

        public Module Transform(Module module)
        {
            string fullModulePath = module.FullPath;
            string moduleId = module.ModuleId;
            byte[] content = _fileSystem.File.ReadAllBytes(fullModulePath);

            var extension = System.IO.Path.GetExtension(fullModulePath);
            var base64 = System.Convert.ToBase64String(content);
            ApacheMimeTypes.Apache.MimeTypes.TryGetValue(extension.Substring(1), out string mimeType);
            // Not sure if OriginalContent of image will be required for some use cases.
            // module.OriginalContent = System.Text.Encoding.Default.GetString(content);
            module.TransformedContent = $"Object.defineProperty(exports, '__esModule', {{value: true}}); exports.default = \"data:{mimeType};base64,{base64}\"";
            return module;
        }
    }
}
