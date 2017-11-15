using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public PrintingConfig<TOwner> ExcludeType<TExcludedType>()
        {
            return this;
        }

        private Dictionary<Type, Func<object, string>> typeToSerializers;
        private Dictionary<string, Func<object, string>> propertyNameToSerializer;
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel, String properyName = null)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            if (properyName != null && propertyNameToSerializer.ContainsKey(properyName))
                return propertyNameToSerializer[properyName](obj);
            if (typeToSerializers.ContainsKey(obj.GetType()))
                return typeToSerializers[obj.GetType()](obj);
            
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        public TypePrintingConfig<T, TOwner> ConfigureType<T>()
        {
            return new TypePrintingConfig<T, TOwner>(this);
        }

        public TypePrintingConfig<T, TOwner> ConfigureProperty<T>(Expression<Func<TOwner, T>> selector)
        {
            return new TypePrintingConfig<T, TOwner>(this);
        }

        public PrintingConfig<TOwner> ExcludeProperty<TProperty>(Expression<Func<TOwner, TProperty>> selector)
        {
            return this;
        }
    }

    public class TypePrintingConfig<TType, TOwner> : ITypePrintingConfig<TType, TOwner>
    {
        private readonly PrintingConfig<TOwner> context;

        public TypePrintingConfig(PrintingConfig<TOwner> context)
        {
            this.context = context;
        }

        public PrintingConfig<TOwner> SetSerializer(Func<PrintingConfig<TType>, string> func)
        {
            return context;
        }

        PrintingConfig<TOwner> ITypePrintingConfig<TType, TOwner>.ParentConfig 
            => context;
    }

    public interface ITypePrintingConfig<TType, TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }

    public static class TypePrintingExtensions
    {
        public static PrintingConfig<TOwner> SetCulture<TOwner>(this ITypePrintingConfig<int, TOwner> config, CultureInfo cultureInfo)
        {
            return config.ParentConfig;
        }
        public static PrintingConfig<TOwner> SetCulture<TOwner>(this ITypePrintingConfig<long, TOwner> config, CultureInfo cultureInfo)
        {
            return config.ParentConfig;
        }
        public static PrintingConfig<TOwner> SetCulture<TOwner>(this ITypePrintingConfig<double, TOwner> config, CultureInfo cultureInfo)
        {
            return config.ParentConfig;
        }
        
        public static PrintingConfig<TOwner> ShrinkToLength<TOwner>(this ITypePrintingConfig<string, TOwner> config, int length)
        {
            return config.ParentConfig;
        }
    }

    public static class PrintingExtensions
    {
        public static string PrintToString<T>(this T obj)
            => ObjectPrinter.For<T>().PrintToString(obj);

        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> configurer)
           => configurer(ObjectPrinter.For<T>()).PrintToString(obj);
    }
}