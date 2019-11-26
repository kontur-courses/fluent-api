using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, T> : IPropertyPrintingConfig<TOwner>
    {
        private readonly MemberInfo elementInfo;
        private readonly PrintingConfig<TOwner> parentConfig;

        internal PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, MemberInfo elementInfo = null)
        {
            this.parentConfig = parentConfig;
            this.elementInfo = elementInfo;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => parentConfig;

        public PrintingConfig<TOwner> Using(Func<T, string> print)
        {
            if (elementInfo == null)
                (parentConfig as IPrintingConfig).TypePrinters[typeof(T)] = print;
            else
                (parentConfig as IPrintingConfig).PropertyPrinters[elementInfo] = print;
            return parentConfig;
        }
    }

    internal interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}