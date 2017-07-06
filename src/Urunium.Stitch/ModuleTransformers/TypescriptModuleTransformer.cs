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
    public class TypescriptModuleTransformer : IModuleTransformer
    {
        IFileSystem _fileSystem;
        Microsoft.ClearScript.ScriptEngine _engine;
        public TypescriptModuleTransformer(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _engine = new Microsoft.ClearScript.V8.V8ScriptEngine();

            string tsc = ResourceReader.Read("typescriptservices.js");
            string bootStrap = @"function transform(source, filename, compilerOptions) {
    return ts.transpileModule(source, { fileName: 'test.tsx', compilerOptions: compilerOptions, reportDiagnostics: true });
}";
            _engine.Execute(tsc);
            _engine.Execute(bootStrap);
        }

        public IEnumerable<string> Extensions => new[] { "ts", "tsx" };

        public Module Transform(Module module)
        {
            string fullModulePath = module.FullPath;
            string moduleId = module.ModuleId;
            string content = module.TransformedContent ?? _fileSystem.File.ReadAllText(fullModulePath);
            module.OriginalContent = module.OriginalContent ?? content;

            dynamic compilerOptions = _engine.Evaluate("value = { 'jsx': ts.JsxEmit.React }");
            dynamic transpileResult = _engine.Invoke("transform", content, _fileSystem.Path.GetFileName(fullModulePath), compilerOptions);
            
            module.TransformedContent = transpileResult.outputText;
            return module;
        }
    }
}
