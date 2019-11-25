using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Expression<Func<TOwner, TPropType>> member;
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Expression<Func<TOwner, TPropType>> memberSelector = null)
        {
            this.printingConfig = printingConfig;
            member = memberSelector;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (member is null)
            {
                var fieldInfo =
                    typeof(PrintingConfig<TOwner>).GetField("serialised", BindingFlags.Instance | BindingFlags.NonPublic);
                var result = (Dictionary<Type, Func<object, string>>) fieldInfo.GetValue(printingConfig);
                result[typeof(TPropType)] = x => print((TPropType) x);
            }
            else
            {
                var fieldInfo =
                    typeof(PrintingConfig<TOwner>).GetField("serialisedProperty", BindingFlags.Instance | BindingFlags.NonPublic);
                var result = (Dictionary<string, Func<object, string>>) fieldInfo.GetValue(printingConfig);
                var propInfo =
                    ((MemberExpression) member.Body).Member as PropertyInfo;
                result[propInfo.Name] = x => print((TPropType) x);
            }
            
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }
    
    
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}