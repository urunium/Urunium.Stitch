using System.Collections.Generic;

namespace Urunium.Stitch
{
    public class SourceConfig
    {
        public string RootPath { get; set; }
        public string[] EntryPoints { get; set; }
        public string[] CopyFiles { get; set; }
        public GlobalsConfig Globals { get; set; }
    }
}