using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Config configuration;
        private readonly PropertyInfo propertyInfo;


        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Config configuration,  PropertyInfo propertyInfo = null)
        {
            this.printingConfig = printingConfig;
            this.configuration = configuration;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (propertyInfo == null)
            {
                if (!configuration.typeSerialization.ContainsKey(typeof(TPropType)))
                    configuration.typeSerialization.Add(typeof(TPropType), print);
                else
                    configuration.typeSerialization[typeof(TPropType)] = print;
            }
            else
            {
                if (!configuration.propertiesSerialization.ContainsKey(propertyInfo))
                    configuration.propertiesSerialization.Add(propertyInfo, print);
                else
                    configuration.propertiesSerialization[propertyInfo] = print;
            }
            configuration.typeSerialization[typeof(TPropType)] = print;
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            configuration.cultures.Add(typeof(TPropType), culture);
            return printingConfig;
        }

        public PrintingConfig<TOwner> ParentConfig => printingConfig;

    }
}