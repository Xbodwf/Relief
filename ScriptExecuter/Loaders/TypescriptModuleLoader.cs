using Jint;
using Jint.Native;
using Jint.Runtime.Modules;
using System;
using System.IO;
using System.Text;
using UnityModManagerNet;
using static UnityEngine.GraphicsBuffer;

namespace ScriptExecuter
{
    public class TypeScriptModuleLoader : IModuleLoader
    {
        private readonly Engine _transformEngine;
        private readonly string _baseDirectory;
        private readonly UnityModManager.ModEntry.ModLogger _logger;

        public TypeScriptModuleLoader(Engine transformEngine, string baseDirectory, UnityModManager.ModEntry.ModLogger logger)
        {
            _transformEngine = transformEngine;
            _baseDirectory = baseDirectory ?? Directory.GetCurrentDirectory();
            _logger = logger;
        }

        public ResolvedSpecifier Resolve(string referencingModuleLocation, ModuleRequest moduleRequest)
        {
            var specifier = moduleRequest.Specifier;

            // Handle relative paths
            string fullPath;
            if (specifier.StartsWith("./") || specifier.StartsWith("../"))
            {
                var referencingDir = Path.GetDirectoryName(referencingModuleLocation) ?? _baseDirectory;
                fullPath = Path.GetFullPath(Path.Combine(referencingDir, specifier));
            }
            else
            {
                fullPath = Path.GetFullPath(Path.Combine(_baseDirectory, specifier));
            }

            // Try different file extensions
            var possibleExtensions = new[] { "", ".ts", ".tsx", ".jsx", ".js" };
            foreach (var ext in possibleExtensions)
            {
                var testPath = fullPath + ext;
                Uri targetUri = new Uri(testPath);
                targetUri = targetUri.MakeRelativeUri(targetUri);
                if (File.Exists(testPath))
                {
                    return new ResolvedSpecifier(moduleRequest, testPath, targetUri,SpecifierType.RelativeOrAbsolute);
                }
            }

            throw new FileNotFoundException($"Module not found: {specifier}");
        }

        public Module LoadModule(Engine engine, ResolvedSpecifier resolved)
        {
            var filePath = resolved.Uri.LocalPath;
            var sourceCode = File.ReadAllText(filePath, Encoding.UTF8);

            // Check if transformation is needed
            var extension = Path.GetExtension(filePath).ToLower();
            var needsTransform = extension == ".ts" || extension == ".tsx" || extension == ".jsx";

            string transformedCode;
            if (needsTransform)
            {
                transformedCode = TransformTypeScript(sourceCode, extension,"module.ts");
                _logger?.Log($"Transformed {extension} file: {Path.GetFileName(filePath)}");
            }
            else
            {
                transformedCode = sourceCode;
            }
            
            return ModuleFactory.BuildSourceTextModule(engine, resolved, transformedCode);
        }

        public string TransformTypeScript(string sourceCode, string extension,string fileName)
        {
            try
            {
                // Configure presets based on file type
                /*var presets = extension == ".tsx" || extension == ".jsx"
                    ? "['env','typescript', 'react']"
                    : "['env','typescript']";

                var transformScript = $@"
                    Babel.transform(`{EscapeJavaScript(sourceCode)}`, {{
                        filename: '{fileName}',
                        presets: {presets},
                        plugins: [
                            'transform-modules-commonjs',
                            'proposal-class-properties',
                            'proposal-object-rest-spread'
                        ]
                    }}).code;
                ";*/

                var presets = @"({
                    target: 'ES6',
                    module: 'ES6',
                    jsx: 'react',
                    allowJs: true,
                    esModuleInterop: true,
                    moduleResolution: 'bundler',
                    skipLibCheck: true,
                    forceConsistentCasingInFileNames: true
                })";

                var transformScript = $@"
                        let result = ts.transpileModule(`{EscapeJavaScript(sourceCode)}`, {{
                            compilerOptions: {presets},
                            fileName: '{fileName}'
                        }});
                        result.outputText;
                 ";
                var result = _transformEngine.Evaluate(transformScript);
                return result.AsString();
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex);
                throw new InvalidOperationException($"TypeScript transformation failed: {ex.Message}", ex);
            }
        }

        private string EscapeJavaScript(string input)
        {
            return input.Replace("\\", "\\\\")
                       .Replace("`", "\\`")
                       .Replace("$", "\\$")
                       .Replace("\r", "\\r")
                       .Replace("\n", "\\n");
        }
    }
}