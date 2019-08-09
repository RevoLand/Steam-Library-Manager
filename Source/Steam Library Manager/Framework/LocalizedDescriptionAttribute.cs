using System.ComponentModel;

namespace Steam_Library_Manager.Framework
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly string _resourceKey;

        public LocalizedDescriptionAttribute(string resourceKey) => _resourceKey = resourceKey;

        public override string Description => Functions.SLM.Translate(_resourceKey);
    }
}