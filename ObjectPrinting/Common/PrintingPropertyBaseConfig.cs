using System.Reflection;

namespace ObjectPrinting.Common
{
    public class PrintingPropertyBaseConfig<TOwner, T> : IPrintingPropertyConfig<TOwner, T>
    {
        private PropertyInfo currentProperty;
        private PrintingConfigRoot root;

        PrintingConfigRoot IHaveRoot.Root => root;
        PropertyInfo IPrintingPropertyBaseConfig<TOwner, T>.CurrentProperty => currentProperty;

        internal PrintingPropertyBaseConfig(PropertyInfo property, PrintingConfigRoot root)
        {
            currentProperty = property;
            this.root = root;
        }
    }
}