using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Urunium.Stitch
{
    public class StitchTask : Task
    {
        [Required]
        public string RootPath { get; set; }
        [Required]
        public string DestinationPath { get; set; }

        public string JsonConfig { get; set; }

        public override bool Execute()
        {
            Log.LogMessage("Stitch Task");
            if (string.IsNullOrWhiteSpace(JsonConfig))
            {
                Stitcher.Stitch.From((source) =>
                {
                    source.RootPath = RootPath;
                    source.EntryPoints = new[] { "App" };
                    source.Globals = new GlobalsConfig
                    {
                        ["react"] = "React",
                        ["react-dom"] = "ReactDOM"
                    };
                }).Into((dest) =>
                {
                    dest.Directory = DestinationPath;
                    dest.BundleFileName = "App.js";
                }).UsingDefaultFileHandlers()
                .UsingDefaultFileSystem().Sew();
            }
            else
            {
                Stitcher.Stitch.UsingJsonConfig(File.ReadAllText(JsonConfig)).Sew();
            }
            Log.LogMessage("End Stitch Task");
            return true;
        }
    }
}
