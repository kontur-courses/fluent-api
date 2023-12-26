using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            var func = new Func<object, string>(obj => print((TPropType)obj));
            printingConfig.TypeSerializers.Add(typeof(TPropType), func);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using<T>(CultureInfo culture) where T:IFormattable
        {
            var cultureFunc = new Func<T, string>(x=>x.ToString(null,culture));
            var func = new Func<object, string>(obj => cultureFunc((T)obj));
            printingConfig.TypeSerializers.Add(typeof(TPropType), func);
            return printingConfig;
        }
    }
}
