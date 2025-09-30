using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using System.Runtime;
using System.IO;
using Jint;
using Jint.Native;
using System.Xml.Linq;
using System.Text.Json;
using Jint.Native.Object;
using Jint.Native.Function;
using Jint.Runtime;
using Jint.Runtime.Modules;
using static UnityModManagerNet.UnityModManager;
using Jint.Native.Array;
using System.Linq;
using System.Collections;
using UnityEngine.EventSystems;
using Jint.Runtime.Interop;
using System.Threading;
using Relief.Modules.vm;

// TODO: Rename this namespace to your mod's name.
namespace Relief
{
    /// <summary>
    /// The main class for the mod. Call other parts of your code from this
    /// class.
    /// </summary>
    public static class MainClass
    {
        /// <summary>
        /// Whether the mod is enabled. This is useful to have as a global
        /// property in case other parts of your mod's code needs to see if the
        /// mod is enabled.
        /// </summary>
        public static bool IsEnabled { get; private set; }
        public static Engine engine { get; private set; }

        public static Engine transformEngine { get; private set; }

        public static TypeScriptModuleLoader typeScriptLoader { get; private set; }

        static Thread Mainthread;

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        public static string ScriptDir
        {
            get
            {
                return Path.Combine(AssemblyDirectory, "Scripts");
            }
        }

        /// <summary>
        /// UMM's logger instance. Use this to write logs to the UMM settings
        /// window under the "Logs" tab.
        /// </summary>
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }

        private static Harmony harmony;

        private static ReactUnityHost _reactUnityHost;
        private static ReactDOM _reactDOM; // Added ReactDOM instance

        /// <summary>
        /// Perform any initial setup with the mod here.
        /// </summary>
        /// <param name="modEntry">UMM's mod entry for the mod.</param>
        internal static void Setup(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;

            // Add hooks to UMM event methods
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = Options.OnGUI;
        }

        /// <summary>
        /// Handler for toggling the mod on/off.
        /// </summary>
        /// <param name="modEntry">UMM's mod entry for the mod.</param>
        /// <param name="value">
        /// <c>true</c> if the mod is being toggled on, <c>false</c> if the mod
        /// is being toggled off.
        /// </param>
        /// <returns><c>true</c></returns>
        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            IsEnabled = value;
            if (value)
            {
                StartMod(modEntry);
            }
            else
            {
                StopMod(modEntry);
            }
            return true;
        }

        /// <summary>
        /// Start the mod up. You can create Unity GameObjects, patch methods,
        /// etc.
        /// </summary>
        /// <param name="modEntry">UMM's mod entry for the mod.</param>
        private static void StartMod(UnityModManager.ModEntry modEntry)
        {
            // Patch everything in this assembly
            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Assembly.LoadFile(Path.Combine(AssemblyDirectory, "./JSNet.dll"));
            try
            {
                if (!Directory.Exists(ScriptDir))
                {
                    Directory.CreateDirectory(ScriptDir);
                }

                transformEngine = new Engine(options =>
                {
                    options.EnableModules(ScriptDir);
                    options.ExperimentalFeatures = ExperimentalFeature.All;
                    options.AllowClr();
                });

                var jsConsole = new JsConsole(Logger);

                transformEngine.SetValue("console", jsConsole);

                transformEngine.Execute(Properties.Resources.tsc);

                typeScriptLoader = new TypeScriptModuleLoader(transformEngine, ScriptDir, Logger,new Jint.Runtime.Modules.DefaultModuleLoader(ScriptDir));

                engine = new Engine(options =>
                {
                    options.EnableModules(ScriptDir);
                    options.ExperimentalFeatures = ExperimentalFeature.All;
                    options.AllowClr();
                    options.Modules.ModuleLoader = typeScriptLoader;
                });
                engine.SetValue("window", engine);
                engine.SetValue("document", engine);
                engine.SetValue("self", engine);

                engine.SetValue("GameObject", typeof(GameObject));
                engine.SetValue("Transform", typeof(Transform));
                engine.SetValue("Debug", typeof(Debug));

               

                var eventSystem = new EventSystem();
                var unityBridge = new UnityBridge(engine);
                var reactUnity = new ReactUnity(engine, unityBridge);

                _reactDOM = new ReactDOM(engine, reactUnity);

                // Create a GameObject for ReactUnityHost and add the component
                var hostGameObject = new GameObject("ReactUnityHost");
                _reactUnityHost = hostGameObject.AddComponent<ReactUnityHost>();
                // Initialize ReactUnityHost with a dummy element for now, will be updated by actual render calls
                _reactUnityHost.Initialize(reactUnity.CreateRoot(hostGameObject), JsValue.Undefined);

                engine.Modules.Add("react", Properties.Resources.react);

                var reactDOMModuleCode = @"
                import React from 'react';

                export function createRoot(container) {
                    return ReactDOM.CreateRoot(container);
                }

                export function render(element, container, callback) {
                    return ReactDOM.Render(element, container, callback);
                }

                export function unmountComponentAtNode(container) {
                    return ReactDOM.UnmountComponentAtNode(container);
                }

                export function findDOMNode(component) {
                    return ReactDOM.FindDOMNode(component);
                }

                export function createPortal(children, container) {
                    return ReactDOM.CreatePortal(children, container);
                }

                export function flushSync(callback) {
                    return ReactDOM.FlushSync(callback);
                }

                const ReactDOMClient = {
                    createRoot,
                    render,
                    unmountComponentAtNode,
                    findDOMNode,
                    createPortal,
                    flushSync
                };

                export default ReactDOMClient;
                ";
                engine.Modules.Add("react-dom", reactDOMModuleCode);

                // 注册所有内置模块
                BuiltInModules.RegisterAllModules(engine, eventSystem, ScriptDir);

                engine.SetValue<JsConsole>("console", jsConsole);

                Mainthread = new Thread(new ThreadStart(ScanModE));
                Mainthread.Start();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private static void ScanModE()
        {
            ScanMods(engine);
        }
        /// <summary>
        /// Stop the mod by cleaning up anything that you created in
        /// <see cref="StartMod(UnityModManager.ModEntry)"/>.
        /// </summary>
        /// <param name="modEntry">UMM's mod entry for the mod.</param>
        private static void StopMod(UnityModManager.ModEntry modEntry)
        {
            // Unpatch everything
            harmony.UnpatchAll(modEntry.Info.Id);
            engine = null;

            if (_reactDOM != null)
            {
                _reactDOM.Cleanup();
                _reactDOM = null;
            }

            if (_reactUnityHost != null)
            {
                UnityEngine.Object.Destroy(_reactUnityHost.gameObject);
                _reactUnityHost = null;
            }
        }

        private static void ScanMods(Engine engine)
        {
            if (engine == null) { return; }


            foreach (var directory in Directory.GetDirectories(ScriptDir))
            {
                string projectJsonPath = Path.Combine(directory, "project.json");
                if (!File.Exists(projectJsonPath)) continue;

                string projectJsonContent = File.ReadAllText(projectJsonPath);
                var projectInfo = JsonSerializer.Deserialize<ProjectInfo>(projectJsonContent,JsonSerializerOptions.Default);

                if (string.IsNullOrEmpty(projectInfo.EntryPoint))
                    continue;

                string entryPointFilePath = Path.Combine(directory, projectInfo.EntryPoint);

                if (!File.Exists(entryPointFilePath)) continue;

                // 构建模块路径
                string modulePath = Path.Combine(ScriptDir, $"{Path.GetFileName(directory)}/{projectInfo.EntryPoint}");

                if (projectInfo.EntryPoint.EndsWith(".ts") || projectInfo.EntryPoint.EndsWith(".tsx") || projectInfo.EntryPoint.EndsWith(".jsx"))
                {
                    // Transform TypeScript/JSX to JavaScript
                    string tsCode = File.ReadAllText(entryPointFilePath);
                    var transformResult = typeScriptLoader.TransformTypeScript(tsCode, Path.GetExtension(entryPointFilePath).ToLower(), Path.GetFileName(entryPointFilePath));
                    var transformedCode = transformResult;

                    // Save transformed code to a .js file
                    string jsFilePath = Path.ChangeExtension(entryPointFilePath, ".temp.js");
                    File.WriteAllText(jsFilePath, transformedCode);
                    modulePath = Path.Combine(ScriptDir, $"{Path.GetFileName(directory)}/{Path.GetFileName(jsFilePath)}");
                }
                Logger.Log($"Load Module <color=#debb7b>{projectInfo.Name}</color>");
                try
                {
                    var exports = engine.Modules.Import(modulePath);

                    // 获取导出的函数
                    var exportFunction = exports.Get("default").AsFunctionInstance();

                    // 调用导出函数
                    exportFunction.Call(engine.Global, new JsValue[] { projectInfo.Id, projectInfo.Name });
                    Logger.Log($"Module <color=#debb7b>{projectInfo.Name}</color> Loaded.");
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    Logger.Log($"Failed to Load Module <color=#debb7b>{projectInfo.Name}</color>");
                }


            }
        }

        public class ProjectInfo
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public object[] Authors { get; set; }
            public string EntryPoint { get; set; }
            public string Version { get; set; }
            public string Description { get; set; }
            public string Inject { get; set; }
        }




    }
}
