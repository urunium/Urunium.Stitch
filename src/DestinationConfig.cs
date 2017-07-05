using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch
{
    public class DestinationConfig
    {
        /// <summary>
        /// Javascript file name (*only filename) where all stitched modules is kept.
        /// *Only filename, not full path, directory path must be set into BundleAt.
        /// Defaults to "bundle.js"
        /// </summary>
        public string BundleFileName { get; set; }
        /// <summary>
        /// Directory where bundle must be created. 
        /// Bundle includes the javascript file, as well as copied resources
        /// </summary>
        public string Directory { get; set; }
    }
}
