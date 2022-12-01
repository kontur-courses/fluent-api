using System.Reflection;

namespace ObjectPrinting.Common
{
    public class PrintingPropertyBaseConfig<TOwner, T> : IPrintingPropertyConfig<TOwner, T>
    {
        private MemberInfo currentProperty;
        private PrintingConfigRoot root;

        PrintingConfigRoot IHaveRoot.Root => root;
        MemberInfo IPrintingPropertyBaseConfig<TOwner, T>.CurrentProperty => currentProperty;

        internal PrintingPropertyBaseConfig(MemberInfo property, PrintingConfigRoot root)
        {
            currentProperty = property;
            this.root = root;
        }
    }
}