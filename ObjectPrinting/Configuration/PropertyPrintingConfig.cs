using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting.Configuration
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> _printingConfig;
        private readonly MemberInfo? _memberInfo;


        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo? memberInfo)
        {
            _printingConfig = printingConfig;
            _memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (_memberInfo == null)
                return _printingConfig.AddAlternativeTypeSerializer<TPropType>(print);

            return _printingConfig.AddAlternativeTypeSerializer(_memberInfo!, print);
        }


        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentPrinter => _printingConfig;
    }
}