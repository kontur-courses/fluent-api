using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly ISet<Type> excluded = new HashSet<Type>();
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            
            if (excluded.Contains(obj.GetType()))
                return string.Empty;

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

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excluded.Add(typeof(T));
            return this;
        }

        public PropertySerializingConfig<TOwner, T> Serializing<T>()
        {
            return new PropertySerializingConfig<TOwner, T>(this);
        }

        public PropertySerializingConfig<TOwner, T> Serializing<T>(Expression<Func<TOwner, T>> func)
        {
            var property = ((MemberExpression) func.Body).Member as PropertyInfo;
            return new PropertySerializingConfig<TOwner, T>(this);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            return new PrintingConfig<TOwner>();
        }
    }

    internal interface IPropertySerializingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }

    public class PropertySerializingConfig<TOwner, TPropType> : IPropertySerializingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;

        PrintingConfig<TOwner> IPropertySerializingConfig<TOwner>.ParentConfig => parentConfig;

        public PropertySerializingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializationMethod)
        {
            return parentConfig;
        }
    }

    public static class PropertySerializingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializingConfig<TOwner, int> config,
            CultureInfo cultureInfo)
        {
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }
        
        public static PrintingConfig<TOwner> TrimmingToLength<TOwner>(this PropertySerializingConfig<TOwner, string> config,
            int count)
        {
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }
    }

    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }
        
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> func)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }
    }
}