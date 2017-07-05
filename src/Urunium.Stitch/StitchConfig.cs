using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch
{
    public class StitchConfig
    {
        public SourceConfig From { get; set; }
        public DestinationConfig Into { get; set; }
        public PackagerExtendibilityConfig Extendibility { get; set; }
    }
}
