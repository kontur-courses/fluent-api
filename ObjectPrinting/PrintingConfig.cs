using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ObjectPrinting
{

    public class PrintingConfig<TOwner> : BasePrintingConfig
    {
        private readonly HashSet<Type> excludedTypes = new();
        private readonly Dictionary<Type, Func<object, string>> typeTransformers = new();
        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return $"null{Environment.NewLine}";
            var type = obj.GetType();
            
            if (typeTransformers.ContainsKey(type))
                return typeTransformers[type](obj);
            
            if (FinalTypes.Contains(type))
                return $"{obj}{Environment.NewLine}";

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties().Where(t => !excludedTypes.Contains(t.PropertyType)))
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        private PrintingConfig<TOwner> UseTransform<TType>(Func<TType, string> transformer)
        {
            typeTransformers[typeof(TType)] = obj => transformer((TType)obj);
            return this;
        }
        
        public PrintingConfig<TOwner> UseFormat<TType>(IFormatProvider formatProvider) 
            where TType : IFormattable
        {
            typeTransformers[typeof(TType)] = obj =>((TType)obj).ToString(null, formatProvider);
            return this;
        }

        public NestingPrintingConfig<TType> When<TType>() => new(this);
        
        

        public class NestingPrintingConfig<TType>
        {
            private readonly PrintingConfig<TOwner> parent;

            public NestingPrintingConfig(PrintingConfig<TOwner> parent)
            {
                this.parent = parent;
            }

            public PrintingConfig<TOwner> Use(Func<TType, string> transformer) => parent.UseTransform(transformer);
        }
    }
}