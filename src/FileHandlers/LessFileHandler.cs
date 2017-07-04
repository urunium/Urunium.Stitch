﻿using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch.FileHandlers
{
    public class LessFileHandler : IFileHandler
    {
        IFileSystem _fileSystem;
        public LessFileHandler(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> Extensions => new[] { "css", "less" };

        public string Build(string content, string fullModulePath, string moduleId)
        {
            dotless.Core.EngineFactory factory = new dotless.Core.EngineFactory(new dotless.Core.configuration.DotlessConfiguration { Debug = true, ImportAllFilesAsLess = true, InlineCssFiles = true, MinifyOutput = true });
            FileReader.FileSystem = _fileSystem;
            factory.Configuration.LessSource = typeof(FileReader);
            var engine = factory.GetEngine();

            engine.CurrentDirectory = _fileSystem.Path.GetFullPath(_fileSystem.Path.GetDirectoryName(fullModulePath));
            var css = engine.TransformToCss(content, moduleId);
            CssToJsModule cssHandler = new CssToJsModule();
            return cssHandler.Build(css, moduleId);
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
