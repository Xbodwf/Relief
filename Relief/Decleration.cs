using System;
using System.Collections.Generic;
using System.Text;

namespace Relief
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct)]
    public class JavascriptTypeAttribute : Attribute
    {
        public JavascriptTypeAttribute(bool isSkipped = false)
        {
            SkipDefinition = isSkipped;
        }

        public bool SkipDefinition { get; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class JavascriptObjectAttribute : Attribute
    {
        public JavascriptObjectAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
