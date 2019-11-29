using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly Expression<Func<TOwner, TPropType>> memberSelector;
        private readonly PrintingConfig<TOwner> printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            this.printingConfig = printingConfig;
            this.memberSelector = memberSelector;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;


        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (memberSelector == null)
                printingConfig.SetSerialization(print);
            else
                printingConfig.SetSerialization(memberSelector, print);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            if (typeof(TPropType) == typeof(int))
                Using(x => ((int) (object) x).ToString(culture));
            if (typeof(TPropType) == typeof(double))
                Using(x => ((double) (object) x).ToString(culture));
            if (typeof(TPropType) == typeof(float))
                Using(x => ((float) (object) x).ToString(culture));
            if (typeof(TPropType) == typeof(sbyte))
                Using(x => ((sbyte) (object) x).ToString(culture));
            if (typeof(TPropType) == typeof(short))
                Using(x => ((short) (object) x).ToString(culture));
            if (typeof(TPropType) == typeof(ushort))
                Using(x => ((ushort) (object) x).ToString(culture));
            if (typeof(TPropType) == typeof(uint))
                Using(x => ((uint) (object) x).ToString(culture));
            if (typeof(TPropType) == typeof(long))
                Using(x => ((long) (object) x).ToString(culture));
            if (typeof(TPropType) == typeof(ulong))
                Using(x => ((ulong) (object) x).ToString(culture));
            return printingConfig;
        }
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}