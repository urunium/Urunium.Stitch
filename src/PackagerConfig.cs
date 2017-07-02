using System.Collections.Generic;

namespace Urunium.Stitch
{
    public class PackagerConfig
    {
        public string RootPath { get; set; }
        public string[] EntryPoints { get; set; }
        public string[] CopyFiles { get; set; }
        public Dictionary<string, string> Globals { get; set; }
    }
}