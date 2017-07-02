using LibSass.Compiler.Options;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch.FileHandlers
{
    public class SassFileHandler : IFileHandler
    {
        IFileSystem _fileSystem;
        public SassFileHandler(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> Extensions => new[] { "scss", "sass" };

        public string Build(string content, string fullModulePath, string moduleId)
        {
            LibSass.Compiler.SassCompiler compiler = new LibSass.Compiler.SassCompiler(new LibSass.Compiler.Options.SassOptions
            {
                Data = content,
                InputPath = fullModulePath,
                OutputStyle = LibSass.Compiler.Options.SassOutputStyle.Compressed,
                Importers = new LibSass.Compiler.Options.CustomImportDelegate[]
                {
                    new CustomImportDelegate((string currentImport, string parentImport, ISassOptions sassOptions) =>
                    {
                        var importPath = _fileSystem.Path.GetFullPath(_fileSystem.Path.Combine(_fileSystem.Path.GetDirectoryName(fullModulePath), currentImport));
                        return new SassImport[]
                        {
                            new SassImport
                            {
                                Data = _fileSystem.File.ReadAllText(importPath),
                                Path = importPath
                            }
                        };
                    })
                }
            });

            CssToJsModule cssHandler = new CssToJsModule();
            var css = compiler.Compile().Output;
            return cssHandler.Build(css, moduleId);
        }
    }
}
