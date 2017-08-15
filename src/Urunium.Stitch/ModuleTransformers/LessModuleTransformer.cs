using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch.ModuleTransformers
{
    public class LessModuleTransformer : IModuleTransformer
    {
        IFileSystem _fileSystem;

        public LessModuleTransformer(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> Extensions => new[] { "css", "less" };

        public Module Transform(Module module)
        {
            string fullModulePath = module.FullPath;
            string moduleId = module.ModuleId;
            string content = module.TransformedContent ?? _fileSystem.File.ReadAllText(fullModulePath);

            dotless.Core.EngineFactory factory = new dotless.Core.EngineFactory(new dotless.Core.configuration.DotlessConfiguration { Debug = true, ImportAllFilesAsLess = true, InlineCssFiles = true, MinifyOutput = true });
            // Temporary solution until Pandora is exposed by Dotless.
            FileReader.FileSystem = _fileSystem;
            factory.Configuration.LessSource = typeof(FileReader);
            var engine = factory.GetEngine();

            engine.CurrentDirectory = _fileSystem.Path.GetFullPath(_fileSystem.Path.GetDirectoryName(fullModulePath));
            var css = engine.TransformToCss(content, moduleId);
            CssToJsModule cssTransformer = new CssToJsModule();
            module.OriginalContent = module.OriginalContent ?? content;
            module.TransformedContent = cssTransformer.Build(css, moduleId);
            return module;
        }

        private class FileReader : dotless.Core.Input.IFileReader
        {
            public static IFileSystem FileSystem;

            public bool UseCacheDependencies => true;

            public bool DoesFileExist(string fileName)
            {
                try
                {
                    return FileSystem.File.Exists(FileSystem.Path.GetFullPath(fileName));
                }
                catch
                {
                    return false;
                }
            }

            public byte[] GetBinaryFileContents(string fileName)
            {
                try
                {
                    return FileSystem.File.ReadAllBytes(FileSystem.Path.GetFullPath(fileName));
                }
                catch
                {
                    return null;
                }
            }

            public string GetFileContents(string fileName)
            {
                try
                {
                    return FileSystem.File.ReadAllText(FileSystem.Path.GetFullPath(fileName));
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
