using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch
{
    public interface IModuleTransformer
    {
        /// <summary>
        /// Gets the file extension associated with this handler
        /// </summary>
        IEnumerable<string> Extensions { get; }

        /// <summary>
        /// Build the file content into CommonJS Module
        /// </summary>
        /// <param name="module"></param>
        Module Transform(Module module);//string content, string fullModulePath, string moduleId);
    }
}
