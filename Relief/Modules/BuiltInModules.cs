using System;
using System.Linq;
using Jint;
using Jint.Native;
using Relief.Modules;
using UnityEngine;

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
            FsModule.Register(engine, scriptDir);
            PathModule.Register(engine);
            ProcessModule.Register(engine);
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


            // Register UIText module
            engine.Modules.Add("uitext", builder =>
            {
                GameObject uiTextGameObject = new GameObject("UITextModule");
                UnityEngine.Object.DontDestroyOnLoad(uiTextGameObject);
                UIText uiTextInstance = uiTextGameObject.AddComponent<UIText>();

                builder.ExportObject("instance", new
                {
                    setText = new Action<string>(uiTextInstance.setText),
                    setPosition = new Action<float, float>(uiTextInstance.setPosition),
                    setSize = new Action<int>(uiTextInstance.setSize),
                    // setFont = new Action<Font>(uiTextInstance.setFont), // Font cannot be directly passed from JS
                    setAlignment = new Action<int>(align => uiTextInstance.setAlignment(uiTextInstance.toAlign(align))),
                    setFontStyle = new Action<int>(style => uiTextInstance.setFontStyle((FontStyle)style)),
                    setShadowEnabled = new Action<bool>(uiTextInstance.setShadowEnabled)
                });
            });

            // React Modules
            engine.Modules.Add("react", Properties.Resources.react);
            engine.Modules.Add("react-unity", Properties.Resources.react_unity);
            engine.Modules.Add("@react-components/unity", Properties.Resources.reactComponents);
        }

    }
}