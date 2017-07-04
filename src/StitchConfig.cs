using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch
{
    public class StitchConfig
    {
        public PackagerConfig Packager { get; set; }
        public PackageCompilerConfig Compiler { get; set; }
        public PackagerExtendibilityConfig Extendibility { get; set; }
    }
}
