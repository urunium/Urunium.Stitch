using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch
{
    internal class ResourceReader
    {
        const string DefaultNamespace = "Urunium.Stitch.Resources";

        public static string Read(string name, string @namespace = DefaultNamespace)
        {
            var assembly = typeof(ResourceReader).Assembly;
            var fullName = string.Format("{0}.{1}", @namespace, name);

            using (var stream = assembly.GetManifestResourceStream(fullName))
            {
                if (stream == null) throw new ArgumentException(fullName + " is not a valid resource.");

                var reader = new StreamReader(stream);

                return reader.ReadToEnd();
            }
        }
    }
}
