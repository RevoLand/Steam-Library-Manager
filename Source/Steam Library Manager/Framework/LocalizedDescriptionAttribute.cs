using System.ComponentModel;

namespace Steam_Library_Manager.Framework
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly string resourceKey;

        public LocalizedDescriptionAttribute(string resourceKey) => this.resourceKey = resourceKey;

        public override string Description
        {
            get
            {
                return Functions.SLM.Translate(resourceKey);
            }
        }
    }
}