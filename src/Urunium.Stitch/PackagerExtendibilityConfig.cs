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
        /// List of Transformers. If any one of transformer is provided default transformers will be discarded.
        /// If need to add new transformer on top of default transformers them all default transformers must also be listed.
        /// e.g.:
        /// new PackagerExtendibilityConfig
        /// {
        ///    Transformers = new List&lt;string&gt
        ///    {
        ///         "Urunium.Stitch.ModuleTransformers.BabelModuleTransformer",
        ///         "Urunium.Stitch.ModuleTransformers.LessModuleTransformer",
        ///         "Urunium.Stitch.ModuleTransformers.SassModuleTransformer",
        ///    }
        /// }
        /// </summary>
        public List<string> Transformers { get; set; }
    }
}
