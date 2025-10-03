using Jint;
using Jint.Native;

namespace Relief
{
    public static class ProcessModule
    {
        public static void Register(Engine engine)
        {
            var host = new HostMethods(""); // 或根据需要传递参数
            engine.Modules.Add("process", builder => {
                builder.ExportFunction("cwd", args => JsValue.FromObject(engine, host.processCwd()));
                builder.ExportFunction("uptime", args => JsValue.FromObject(engine, host.processUptime()));
            });
        }
    }
}