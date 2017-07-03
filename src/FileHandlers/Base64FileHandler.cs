using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch.FileHandlers
{
    public class Base64FileHandler : IFileHandler
    {
        public IEnumerable<string> Extensions => new[] { "png", "gif", "jpg" };

        public string Build(string content, string fullModulePath, string moduleId)
        {
            var extension = System.IO.Path.GetExtension(fullModulePath);
            var base64 = System.Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(content));
            ApacheMimeTypes.Apache.MimeTypes.TryGetValue(extension.Substring(1), out string mimeType);
            return $"Object.defineProperty(exports, '__esModule', {{value: true}}); exports.default = \"data:{mimeType};base64,{base64}\"";
        }
    }
}
