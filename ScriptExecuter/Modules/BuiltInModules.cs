using System;
using Jint;
using Jint.Native;
using Jint.Runtime.Modules;
using Jint.Runtime.Interop;

namespace ScriptExecuter
{
    /// <summary>
    /// </summary>
    public static class BuiltInModules
    {
        /// <summary>
        /// 注册所有内置模块到JavaScript引擎
        /// </summary>
        /// <param name="engine">JavaScript引擎实例</param>
        /// <param name="eventSystem">事件系统实例</param>
        /// <param name="scriptDir">脚本目录路径</param>
        public static void RegisterAllModules(Engine engine, EventSystem eventSystem, string scriptDir)
        {
            // 注册主机方法
            var hostMethods = new HostMethods(scriptDir);
            engine.SetValue("host", hostMethods);

            // fs
            RegisterFileSystemModule(engine);
            
            // path
            RegisterPathModule(engine);
            
            // process
            RegisterProcessModule(engine);
            
            // event handler
            RegisterEventModule(engine, eventSystem);

            // UI Module
            RegisterUiModule(engine);
        }

        /// <summary>
        /// Fs
        /// </summary>
        private static void RegisterFileSystemModule(Engine engine)
        {
            string fsModuleCode = @"
        export function readFileSync(filePath, encodingOrOptions) {
            let encoding = 'utf-8';
            if (typeof encodingOrOptions === 'object') {
                encoding = encodingOrOptions.encoding || 'utf-8';
            } else if (typeof encodingOrOptions === 'string') {
                encoding = encodingOrOptions;
            }
            return host.readFileSync(filePath, encoding);
        }

        export function readFile(filePath, encodingOrCallback, callback) {
            let encoding = 'utf-8';
            let cb = callback;

            if (typeof encodingOrCallback === 'function') {
                cb = encodingOrCallback;
            } else if (typeof encodingOrCallback === 'string') {
                encoding = encodingOrCallback;
            }

            host.readFile(filePath, encoding, (err, data) => {
                if (err) {
                    cb(new Error(err));
                } else {
                    cb(null, data);
                }
            });
        }

        export function writeFileSync(filePath, data, encodingOrOptions) {
            let encoding = 'utf-8';
            if (typeof encodingOrOptions === 'object') {
                encoding = encodingOrOptions.encoding || 'utf-8';
            } else if (typeof encodingOrOptions === 'string') {
                encoding = encodingOrOptions;
            }
            return host.writeFileSync(filePath, data, encoding);
        }

        export function writeFile(filePath, data, encodingOrCallback, callback) {
            let encoding = 'utf-8';
            let cb = callback;

            if (typeof encodingOrCallback === 'function') {
                cb = encodingOrCallback;
            } else if (typeof encodingOrCallback === 'string') {
                encoding = encodingOrCallback;
            }

            host.writeFile(filePath, data, encoding, (err) => {
                if (err) {
                    cb(new Error(err));
                } else {
                    cb(null);
                }
            });
        }

        export function existsSync(filePath) {
            return host.existsSync(filePath);
        }

        export function mkdirSync(dirPath) {
            return host.mkdirSync(dirPath);
        }

        export function readdirSync(dirPath) {
            return host.readdirSync(dirPath);
        }

        export function unlinkSync(filePath) {
            return host.unlinkSync(filePath);
        }

        export function rmdirSync(dirPath) {
            return host.rmdirSync(dirPath);
        }
    ";
            engine.Modules.Add("fs", fsModuleCode);
        }

        /// <summary>
        /// Path
        /// </summary>
        private static void RegisterPathModule(Engine engine)
        {
            string pathModuleCode = @"
        export function join(...paths) {
            return host.pathJoin(paths);
        }

        export function resolve(...paths) {
            return host.pathResolve(paths);
        }

        export function basename(path) {
            return host.pathBasename(path);
        }

        export function dirname(path) {
            return host.pathDirname(path);
        }

        export function extname(path) {
            return host.pathExtname(path);
        }

        export function isAbsolute(path) {
            return host.pathIsAbsolute(path);
        }
    ";
            engine.Modules.Add("path", pathModuleCode);
        }

        /// <summary>
        /// Process
        /// </summary>
        private static void RegisterProcessModule(Engine engine)
        {
            string processModuleCode = @"
            export {
                    cwd: () => host.processCwd(),
                    env: host.processEnv,
                    platform: host.processPlatform,
                    version: host.processVersion,
                    arch: host.processArch,
                    pid: host.processPid,
                    uptime: host.processUptime
                };
    ";
            engine.Modules.Add("process", processModuleCode);
        }

        /// <summary>
        /// Event Handlers
        /// </summary>
        private static void RegisterEventModule(Engine engine, EventSystem eventSystem)
        {
            string eventHandlerModuleCode = @"
        const events = {}";

            engine.Modules.Add("eventHandler", eventHandlerModuleCode);

            engine.SetValue("registerEvent", new Func<string, Delegate, string>((eventName, callback) =>
            {
                var guid = eventSystem.RegisterEvent(eventName, callback);
                return guid;
            }));

            engine.SetValue("unregisterEvent", new Action<string>(guid =>
            {
                eventSystem.UnregisterEvent(guid);
            }));

            engine.SetValue("triggerEvent", new Action<string, object[]>((eventName, args) =>
            {
                eventSystem.TriggerEvent(eventName, args);
            }));
        }

        /// <summary>
        /// UI Module
        /// </summary>
        private static void RegisterUiModule(Engine engine)
        {
            string uiModuleCode = @"
                export const MessageBox = {
                    show: (title, message, onConfirm, onCancel) => {
                        __messageBoxShow(title, message, onConfirm, onCancel);
                    }
                };
            ";
            engine.Modules.Add("ui", uiModuleCode);

            engine.SetValue("__messageBoxShow", new ClrFunction(engine, "__messageBoxShow", (thisObj, args) =>
            {
                string title = args[0].AsString();
                string message = args[1].AsString();

                Action onConfirm = null;
                if (args.Length > 2 && JsConsole.IsFunction(args[2]))
                {
                    onConfirm = () => args[2].AsFunctionInstance().Call();
                }

                Action onCancel = null;
                if (args.Length > 3 && JsConsole.IsFunction(args[3]))
                {
                    onCancel = () => args[2].AsFunctionInstance().Call();
                }

                return JsValue.FromObject(engine,new MessageBox());
            }));
        }
    }
}