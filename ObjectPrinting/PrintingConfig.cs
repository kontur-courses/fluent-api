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
        private HashSet<Type> excludingTypes = new HashSet<Type>();

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            if (excludingTypes.Contains(obj.GetType()))
                return string.Empty;

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

        private string PrintToString_old(object obj, int nestingLevel)
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
            excludingTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding(Expression<Func<TOwner, object>> func)
        {
          
            return this;
        }

        public PropertySerializerConfig<TOwner, T> Serializing<T>()
        {
            return new PropertySerializerConfig<TOwner, T>(this);
        }

        public PropertySerializerConfig<TOwner, T> Serializing<T>(Expression<Func<TOwner, T>> func)
        {
            return new PropertySerializerConfig<TOwner, T>(this);
        }
    }

    public class PropertySerializerConfig<TOwner, T> : IPropertySerializingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;

        public PropertySerializerConfig(PrintingConfig<TOwner> parentConfig)
        {
            throw new NotImplementedException();
        }

        PrintingConfig<TOwner> IPropertySerializingConfig<TOwner>.ParentConfig => parentConfig;

        public PrintingConfig<TOwner> Using(Func<T, string> func)
        {
            return parentConfig;
        }
    }

    public interface IPropertySerializingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }

    public static class PropertySerializingConfigExcentions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializerConfig<TOwner, byte> config, CultureInfo cultureInfo)
        {
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializerConfig<TOwner, short> config, CultureInfo cultureInfo)
        {
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializerConfig<TOwner, int> config, CultureInfo cultureInfo)
        {
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializerConfig<TOwner, long> config, CultureInfo cultureInfo)
        {
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializerConfig<TOwner, float> config, CultureInfo cultureInfo)
        {
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializerConfig<TOwner, double> config, CultureInfo cultureInfo)
        {
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> WithMaxLength<TOwner>(this PropertySerializerConfig<TOwner, string> config, int maxLength)
        {
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }
    }

    public static class ObjectExtentions
    {
        public static PrintingConfig<T> GetObjectPrinter<T>(this T _)
        {
            return ObjectPrinter.For<T>();
        }

        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }
    }

}