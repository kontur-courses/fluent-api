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
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        private PrintingConfig<TOwner> printingConfig { get; set; }
        private Expression<Func<TOwner, TPropType>> propertySelector;
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Expression<Func<TOwner, TPropType>> propertySelector=null)
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
                var propetry = typeof(TOwner).GetProperty(propertyName);
                ((IPrintingConfig<TOwner>) printingConfig).AddPropertySerializationFormat(propetry, func);
            }

            ((IPrintingConfig<TOwner>) printingConfig).AddTypeSerializationFormat(typeof(TPropType), func);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo cultureInfo)
        {
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.PrintingConfig => printingConfig;
    }
}
