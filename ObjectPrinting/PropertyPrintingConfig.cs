﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string memberName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
            memberName = null;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Expression<Func<TOwner, TPropType>> memberSelector)
        {
            this.printingConfig = printingConfig;
            memberName = ((MemberExpression)memberSelector.Body).Member.Name;
        }

        public PrintingConfig<TOwner> Using(Expression<Func<TPropType, string>> print)
        {
            if (memberName == null)
                ((IPrintingConfig<TOwner>) printingConfig).PrintersForTypes[typeof(TPropType)] =
                    prop => print.Compile().Invoke((TPropType) prop);
            else
                ((IPrintingConfig<TOwner>) printingConfig).PrintersForPropertiesNames[memberName] =
                    prop => print.Compile().Invoke((TPropType) prop);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            ((IPrintingConfig<TOwner>) printingConfig).CultureInfoForTypes[typeof(TPropType)] = culture;
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}