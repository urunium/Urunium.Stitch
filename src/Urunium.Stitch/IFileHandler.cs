using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch
{
    public interface IFileHandler
    {
        /// <summary>
        /// Gets the file extension associated with this handler
        /// </summary>
        IEnumerable<string> Extensions { get; }

        /// <summary>
        /// Build the file content into CommonJS Module
        /// </summary>
        /// <param name="content">The file content</param>
        /// <returns>Content suitable for a CommonJS module</returns>
        string Build(string content, string fullModulePath, string moduleId);
    }
}
