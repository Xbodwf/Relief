using System;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Runtime.Modules;
using Jint.Runtime.Interop;
using Relief.UI;

namespace Relief
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
            var host = new HostMethods(scriptDir);
            engine.SetValue("host", host);

            // fs
            engine.Modules.Add("fs", builder => {
                builder.ExportFunction("readFileSync", args => {
                    var path = args[0].AsString();
                    var encoding = args.Length > 1 ? args[1].AsString() : "utf8";
                    return JsValue.FromObject(engine, host.readFileSync(path, encoding));
                });
                builder.ExportFunction("writeFileSync", args => {
                    var path = args[0].AsString();
                    var content = args[1].AsString();
                    var encoding = args.Length > 2 ? args[2].AsString() : "utf8";
                    return JsValue.FromObject(engine, host.writeFileSync(path, content, encoding));
                });
                builder.ExportFunction("existsSync", args => {
                    var path = args[0].AsString();
                    return JsValue.FromObject(engine, host.existsSync(path));
                });
                builder.ExportFunction("mkdirSync", args => {
                    var path = args[0].AsString();
                    return JsValue.FromObject(engine, host.mkdirSync(path));
                });
                builder.ExportFunction("readdirSync", args => {
                    var path = args[0].AsString();
                    return JsValue.FromObject(engine, host.readdirSync(path));
                });
                builder.ExportFunction("unlinkSync", args => {
                    var path = args[0].AsString();
                    return JsValue.FromObject(engine, host.unlinkSync(path));
                });
                builder.ExportFunction("rmdirSync", args => {
                    var path = args[0].AsString();
                    return JsValue.FromObject(engine, host.rmdirSync(path));
                });
            });

            // path
            engine.Modules.Add("path", builder => {
                builder.ExportFunction("join", args => {
                    return JsValue.FromObject(engine, host.pathJoin(args));
                });
                builder.ExportFunction("resolve", args => {
                    return JsValue.FromObject(engine, host.pathResolve(args));
                });
                builder.ExportFunction("basename", args => {
                    var path = args[0].AsString();
                    return JsValue.FromObject(engine, host.pathBasename(path));
                });
                builder.ExportFunction("dirname", args => {
                    var path = args[0].AsString();
                    return JsValue.FromObject(engine, host.pathDirname(path));
                });
                builder.ExportFunction("extname", args => {
                    var path = args[0].AsString();
                    return JsValue.FromObject(engine, host.pathExtname(path));
                });
                builder.ExportFunction("isAbsolute", args => {
                    var path = args[0].AsString();
                    return JsValue.FromObject(engine, host.pathIsAbsolute(path));
                });
            });

            // process
            engine.Modules.Add("process", builder => {
                builder.ExportFunction("cwd", args => JsValue.FromObject(engine, host.processCwd()));
                builder.ExportFunction("uptime", args => JsValue.FromObject(engine, host.processUptime()));
            });

            // event handler
            engine.Modules.Add("eventHandler", builder => {
                builder.ExportFunction("registerEvent", args => {
                    var name = args[0].AsString();
                    var del = args[1].ToObject();
                    return JsValue.FromObject(engine, eventSystem.RegisterEvent(name, del as Delegate));
                });
                builder.ExportFunction("unregisterEvent", args => {
                    var name = args[0].AsString();
                    eventSystem.UnregisterEvent(name);
                    return JsValue.Undefined;
                });
                builder.ExportFunction("triggerEvent", args => {
                    var name = args[0].AsString();
                    var eventArgs = args.Skip(1).Select(a => a.ToObject()).ToArray();
                    eventSystem.TriggerEvent(name, eventArgs);
                    return JsValue.Undefined;
                });
            });

            engine.Modules.Add("ui", builder => {
                builder.ExportObject("Notification", new
                {
                    show = new Action<string, float, float>(showNotification)
                });
            });

            // React Modules
            engine.Modules.Add("react", Properties.Resources.react);
            engine.Modules.Add("react-unity", Properties.Resources.react_unity);
            engine.Modules.Add("@react-components/unity",Properties.Resources.reactComponents);
        }

        /// <summary>
        /// UI Module
        /// </summary>
        /// 
        private static void showNotification(string message, float duration = 1, float size = 32)
        {
            NotificationManager.Instance.ShowNotification(message, duration, size);
        }
    }
}