using System.Reflection;

namespace ObjectPrinting.Common
{
    public class PrintingPropertyConfig<TOwner, T> : IPrintingPropertyConfig<TOwner, T>
    {
        private readonly PrintingConfigRoot root;
        private readonly MemberInfo currentProperty;

        PrintingConfigRoot IHaveRoot.Root => root;
        MemberInfo IPrintingPropertyBaseConfig<TOwner, T>.CurrentProperty => currentProperty;

        internal PrintingPropertyConfig(MemberInfo property, PrintingConfigRoot root)
        {
            this.root = root;
            currentProperty = property;
        }
    }
}