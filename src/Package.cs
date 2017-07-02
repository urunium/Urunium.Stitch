using System.Collections.Generic;

namespace Urunium.Stitch
{
    public class Package
    {
        public List<Module> Modules { get; } = new List<Module>();
        public List<string> Files { get; } = new List<string>();
    }

    public class Module
    {
        public string FullPath { get; set; }
        public string ModuleId { get; set; }
        public string OriginalContent { get; set; }
        public string TransformedContent { get; set; }
    }
}