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
        private Dictionary<PropertyInfo, Func<object, string>> propertyRules = new Dictionary<PropertyInfo, Func<object, string>>();
        private Dictionary<Type, Func<Type, string>> typeRules = new Dictionary<Type, Func<Type, string>>();
        private Dictionary<Type, Func<Type, string>> cultureRules = new Dictionary<Type, Func<Type, string>>();

        private HashSet<Type> excludedTypes = new HashSet<Type>();
        private HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();

        protected void addPropertyRule(Func<object, string> rule)
        {

        }
        protected void addTypeRule(Func<object, string> rule)
        {

        }
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

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
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PropertySerializationConfig<TOwner, TPropType> For<TPropType>(Expression<Func<TOwner, TPropType>> propType)
        {
            return new PropertySerializationConfig<TOwner, TPropType>();
        }
        public PropertySerializationConfig<TOwner, TPropType> For<TPropType>()
        {
            return new PropertySerializationConfig<TOwner, TPropType>();
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> func)
        {
            var propInfo = ((MemberExpression)func.Body).Member as PropertyInfo;

            return this;
        }
    }

    public class PropertySerializationConfig<TOwner, TPropType> : IPropertySerializingConfig<TOwner>
    {
        private PrintingConfig<TOwner> parentConfig;
        PrintingConfig<TOwner> IPropertySerializingConfig<TOwner>.ParentConfig => parentConfig;

        public PrintingConfig<TOwner> WithSerialization(Func<TPropType, string> serializationFunc)
        {
            return parentConfig;
        }
    }

    public interface IPropertySerializingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
    public static class PropertySerializingConfigExtensions
    {

        public static PrintingConfig<TOwner> WithCulture<TOwner>
            (this PropertySerializationConfig<TOwner, int> config, CultureInfo culture)
        {
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }


        public static PrintingConfig<TOwner> Trim<TOwner>(this PropertySerializationConfig<TOwner, string> config, int length)
        {
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }
    }

    public static class PrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> func)
        {
            return func(ObjectPrinter.For<T>()).PrintToString(obj);
        }
    }
}