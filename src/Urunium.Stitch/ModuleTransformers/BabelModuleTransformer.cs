using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch.ModuleTransformers
{
    public class BabelModuleTransformer : IModuleTransformer
    {
        IFileSystem _fileSystem;
        Microsoft.ClearScript.ScriptEngine _engine;
        public BabelModuleTransformer(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _engine = new Microsoft.ClearScript.V8.V8ScriptEngine();
            string babelStandAlone = ResourceReader.Read("babel.min.js");

            _engine.Execute(babelStandAlone + "\n;var transform = Babel.transform;");
        }

        public IEnumerable<string> Extensions => new[] { "js", "jsx" };

        public Module Transform(Module module)
        {
            string fullModulePath = module.FullPath;
            string moduleId = module.ModuleId;
            string content = module.TransformedContent ?? _fileSystem.File.ReadAllText(fullModulePath);

            module.OriginalContent = module.OriginalContent ?? content;

            object config = _engine.Evaluate("value = {presets : ['es2015', 'react']}");
            dynamic response = _engine.Invoke("transform", content, config);

            module.TransformedContent = response.code;
            return module;
        }

    }
}
