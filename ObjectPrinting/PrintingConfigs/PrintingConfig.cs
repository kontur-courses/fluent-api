using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly List<SerializationRule> serializationRules;

         public PrintingConfig()
        {
            serializationRules = new List<SerializationRule>();
        }

        public string PrintToString(TOwner obj)
        {
            return ObjectPrinter.PrintToString(obj, this);
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            serializationRules.Add(
                new SerializationRule((obj, propertyInfo) => propertyInfo.PropertyType == typeof(T),
                null));
            return this;
        }

        public PrintingConfig<TOwner> Excluding(Expression<Func<TOwner, string>> func)
        {
            var propInfo = ((MemberExpression) func.Body).Member as PropertyInfo;

            serializationRules.Add(
                new SerializationRule((obj, propertyInfo) => propertyInfo.Name == propInfo?.Name,
                null));
            return this;
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> func)
        {
            return new PropertyPrintingConfig<TOwner, T>(this, func);
        }

        IReadOnlyList<SerializationRule> IPrintingConfig.SerializationRules => serializationRules;

        void IPrintingConfig.ApplyNewSerializationRule(SerializationRule rule) => serializationRules.Add(rule);
    }
}