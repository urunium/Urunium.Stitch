using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch.FileHandlers
{
    public class BabelFilehandler : IFileHandler
    {
        Microsoft.ClearScript.ScriptEngine _engine;
        public BabelFilehandler()
        {
            _engine = new Microsoft.ClearScript.V8.V8ScriptEngine();
            string babelStandAlone = ResourceReader.Read("babel.min.js");

            _engine.Execute(babelStandAlone + "\n;var transform = Babel.transform;");
        }

        public IEnumerable<string> Extensions => new[] { "js", "jsx" };

        public string Build(string content, string fullModulePath, string moduleId)
        {
            var config = _engine.Evaluate("value = {presets : ['es2015', 'react']}");
            dynamic response = _engine.Invoke("transform", content, config);
            return response.code as string;
        }
    }
}
