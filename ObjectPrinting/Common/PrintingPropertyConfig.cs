using System.Reflection;

namespace ObjectPrinting.Common
{
    public class PrintingPropertyConfig<TOwner, T> : IPrintingPropertyConfig<TOwner, T>
    {
        private readonly PrintingConfigRoot root;
        private readonly PropertyInfo currentProperty;

        PrintingConfigRoot IHaveRoot.Root => root;
        PropertyInfo IPrintingPropertyBaseConfig<TOwner, T>.CurrentProperty => currentProperty;

        internal PrintingPropertyConfig(PropertyInfo property, PrintingConfigRoot root)
        {
            this.root = root;
            currentProperty = property;
        }
    }
}