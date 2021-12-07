using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TPropType> : PrintingConfig<TOwner>, IMemberPrintingConfig<TOwner, TPropType>
    {
        private readonly MemberInfo property;
        PrintingConfig<TOwner> IMemberPrintingConfig<TOwner, TPropType>.PrintingConfig => this;

        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo property = null) : base(printingConfig)
        {
            this.property = property;
        }
        
        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (property is null)
                TypeCustomPrintings[typeof(TPropType)] = o => print((TPropType)o);
            else
                MemberCustomPrinting[property] =  o => print((TPropType)o);
            return this;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture) 
        {
            var toStringWithCulture = typeof(TPropType).GetMethod("ToString", new[] { typeof(IFormatProvider) });
            return toStringWithCulture == null ? this : 
                Using(o => (string)toStringWithCulture.Invoke(o, new object[] { culture })) ;
        }

    }
    

    public interface IMemberPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
        PrintingConfig<TOwner> Using(Func<TPropType, string> print);
        public PrintingConfig<TOwner> Using(CultureInfo culture);
    }
}