using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo propertyInfo = null;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            this.printingConfig = printingConfig;
            propertyInfo = ((MemberExpression)memberSelector.Body).Member;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (propertyInfo is null)
                printingConfig.serializersForTypes[typeof(TPropType)] = print;
            else
                printingConfig.serializersForMembers[propertyInfo] = print;

            return printingConfig;
        }
    }
}