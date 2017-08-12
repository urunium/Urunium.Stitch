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
            bool success = false;
            try
            {
                Log.LogMessage("Stitch Task");
                var json = File.ReadAllText(JsonConfig).Replace("$(ProjectDir)", RootPath.Replace("\\", "\\\\")).Replace("$(OutDir)", DestinationPath.Replace("\\", "\\\\"));
                Log.LogMessage($"Stitch Task with following Config: {json}");
                Stitcher.Stitch.UsingJsonConfig(json).Sew();
                success = true;
            }
            catch (Exception ex)
            {
                Log.LogError(ex.ToString());
            }
            finally
            {
                Log.LogMessage("End Stitch Task");
            }
            return success;
        }
    }
}
