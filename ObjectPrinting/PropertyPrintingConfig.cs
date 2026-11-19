using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo? member;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo? member)
        {
            this.printingConfig = printingConfig;
            this.member = member;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig) 
            => this.printingConfig = printingConfig;

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
            => Register(obj => print((TPropType)obj));


        public PrintingConfig<TOwner> Using(CultureInfo culture)
            => Register(obj => obj is IFormattable f
                ? f.ToString(null, culture) 
                : obj.ToString() ?? "");

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        private PrintingConfig<TOwner> Register(Func<object, string> formater)
        {
            if (member != null)
                printingConfig._formaterMember[MemberKey.From(member)] = formater;
            else
                printingConfig._formaterType[typeof(TPropType)] = formater;
            return printingConfig;
        }


    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }

}