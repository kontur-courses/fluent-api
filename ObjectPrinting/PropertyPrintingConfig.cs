using ObjectPrinting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        public readonly PrintingConfig<TOwner> printingConfig;
        public readonly PropertyInfo propertyInfo;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
        {
            this.printingConfig = printingConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            var func = new Func<object, string>(obj => print((TPropType)obj));
            printingConfig.PropertySerializers.Add(propertyInfo, func);
            return printingConfig;
        }
    }
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}
