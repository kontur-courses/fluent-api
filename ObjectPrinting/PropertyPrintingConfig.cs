using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Internal;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private PrintingConfig<TOwner> printingConfig { get; }
        private readonly Expression<Func<TOwner, TPropType>> propertySelector;
        public PropertyPrintingConfig(
            PrintingConfig<TOwner> printingConfig, 
            Expression<Func<TOwner, TPropType>> propertySelector=null)
        {
            this.printingConfig = printingConfig;
            this.propertySelector = propertySelector;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> func)
        {
            if (propertySelector != null)
            {
                if (!(propertySelector.Body is MemberExpression body))
                    throw new ArgumentException();
                var propertyName = body.Member.Name;
                var property = typeof(TOwner).GetProperty(propertyName);
                ((IPrintingConfig) printingConfig).AddPropertySerializationFormat(property, func);
            }

            ((IPrintingConfig) printingConfig).AddTypeSerializationFormat(typeof(TPropType), func);
            return printingConfig;
        }
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.PrintingConfig => printingConfig;
        Expression<Func<TOwner, TPropType>> IPropertyPrintingConfig<TOwner, TPropType>.PropertySelector => propertySelector;
    }
}
