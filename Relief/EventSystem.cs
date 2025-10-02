using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using System.Linq;

namespace Relief
{
    /// <summary>
    /// 简单事件系统，支持事件注册、移除和触发，参数可正常传递
    /// </summary>
    public class EventSystem
    {
        // 事件名 -> 委托列表
        private readonly Dictionary<string, List<Delegate>> _eventHandlers = new();
        private readonly Engine _jsEngine;

        public EventSystem(Engine jsEngine)
        {
            _jsEngine = jsEngine;
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="handler">事件处理委托</param>
        /// <returns>是否注册成功</returns>
        public bool RegisterEvent(string eventName, Delegate handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null)
                return false;

            if (!_eventHandlers.TryGetValue(eventName, out var handlers))
            {
                handlers = new List<Delegate>();
                _eventHandlers[eventName] = handlers;
            }

            if (!handlers.Contains(handler))
                handlers.Add(handler);

            return true;
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="eventName">事件名</param>
        public void UnregisterEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            _eventHandlers.Remove(eventName);
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="args">参数</param>
        public void TriggerEvent(string eventName, params object[] args)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            if (_eventHandlers.TryGetValue(eventName, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        // 如果是 Action<object[]>，直接传递参数数组
                        if (handler is Action<object[]> arrHandler)
                        {
                            arrHandler(args);
                        }
                        else
                        {
                            handler.DynamicInvoke(args);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"事件 {eventName} 触发异常: {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// 注册 JavaScript 函数作为事件回调
        /// 支持参数解构：如 function({__result}) {...}
        /// </summary>
        public bool RegisterEvent(string eventName, JsValue callback)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentException("Event name cannot be null or empty", nameof(eventName));

            if (callback == null || callback == JsValue.Undefined || callback == JsValue.Null)
                throw new ArgumentException("Callback cannot be null or undefined", nameof(callback));

            if (!(callback is Function function))
                throw new ArgumentException("Callback must be a function", nameof(callback));

            var thisArg = function.Engine.Global;

            // 包装器：将所有参数打包为一个对象传递给 JS（支持解构）
            return RegisterEvent(eventName, new Action<object[]>(args =>
            {
                try
                {
                    JsValue jsArg = JsValue.Undefined;

                    // 如果参数是 Dictionary<string, object>，则转为 JS 对象
                    if (args != null && args.Length == 1 && args[0] is Dictionary<string, object> dict)
                    {
                        // 用 FromObject 直接转换为 JS 对象
                        var jsObj = JsValue.FromObject(_jsEngine, dict);
                        jsArg = jsObj;
                    }
                    else
                    {
                        // 否则直接传递参数数组
                        jsArg = JsValue.FromObject(_jsEngine, args);
                    }

                    function.Call(thisArg, jsArg);
                }
                catch (Exception ex)
                {
                    LogCallbackError(eventName, ex, args);
                }
            }));
        }

        /// <summary>
        /// 将 C# 值转换为 JavaScript 值
        /// </summary>
        private JsValue ConvertToJsValue(object value, string eventName, int argIndex)
        {
            try
            {
                if (value == null)
                {
                    return JsValue.Null;
                }

                // 处理基本类型
                if (value is int intValue)
                {
                    return new JsNumber(intValue);
                }
                if (value is double doubleValue)
                {
                    return new JsNumber(doubleValue);
                }
                if (value is float floatValue)
                {
                    return new JsNumber((double)floatValue);
                }
                if (value is long longValue)
                {
                    return new JsNumber((double)longValue);
                }
                if (value is bool boolValue)
                {
                    return boolValue ? JsBoolean.True : JsBoolean.False;
                }
                if (value is string stringValue)
                {
                    return new JsString(stringValue);
                }

                // 其他类型使用通用转换
                return JsValue.FromObject(_jsEngine, value);
            }
            catch (Exception ex)
            {
                MainClass.Logger.Error($"Error converting argument {argIndex} for event {eventName}: {ex.Message}");
                return JsValue.Undefined;
            }
        }

        /// <summary>
        /// 记录回调错误信息
        /// </summary>
        private void LogCallbackError(string eventName, Exception ex, object[] args)
        {
            
            string argsStr = args == null ? "null" : 
                $"[{string.Join(", ", args.Select(a => a?.ToString() ?? "null"))}]";
            MainClass.Logger.Error($"Arguments: {argsStr}");
            
            if (ex.InnerException != null)
            {
                MainClass.Logger.Error($"Inner exception: {ex.InnerException.Message}");
                MainClass.Logger.Error($"Inner exception stack trace: {ex.InnerException.StackTrace}");
            }
            
            MainClass.Logger.Error($"Stack trace: {ex.StackTrace}");
        }
    }
}

