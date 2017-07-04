using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch
{
    public class PackagerExtendibilityConfig
    {
        /// <summary>
        /// Register to Dependency Injection container, maps of fully qualified type names.
        /// e.g.: 
        /// new PackagerExtendibilityConfig 
        /// {
        ///     DI = new Dictionary&lt;string, string&gt;
        ///     {
        ///         {
        ///             "System.IO.Abstractions.IFileSystem", "MyAwesomeApp.MyAwesomeFileSystem"
        ///         },
        ///         {
        ///             "Urunium.Stitch.PackageCompiler", "MyAwesomeApp.PackageCompilerWithSomeAwesomeFeature"
        ///         },
        ///         {
        ///             "ISomeOtherDependency", "MyAwesomeApp.SomImplementation"
        ///         }
        ///     }
        /// }
        /// </summary>
        public Dictionary<string, string> DI { get; set; }

        /// <summary>
        /// List of FileHandlers. If any one of handler is provided default handlers will be discarded.
        /// If need to add new handler on top of default handlers them all default handlers must also be listed.
        /// e.g.:
        /// new PackagerExtendibilityConfig
        /// {
        ///    FileHandlers = new List&lt;string&gt
        ///    {
        ///         "Urunium.Stitch.BabelFileHandler",
        ///         "Urunium.Stitch.LessFileHandler",
        ///         "Urunium.Stitch.SassFileHandler",
        ///    }
        /// }
        /// </summary>
        public List<string> FileHandlers { get; set; }
    }
}
