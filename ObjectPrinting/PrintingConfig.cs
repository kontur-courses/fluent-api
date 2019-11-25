using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> :
        IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> ExcludedTypes = new HashSet<Type>();
        private readonly HashSet<string> ExcludedProperties = new HashSet<string>();
        private readonly  Dictionary<Type, Func<object, string> > exactSerializationForType = 
            new Dictionary<Type, Func<object, string>>();
        private readonly  Dictionary<string, Func<object, string> > exactSerializationForProperty = 
            new Dictionary<string, Func<object, string>>();
        private readonly Dictionary<Type, CultureInfo> numberTypesCultures =
            new Dictionary<Type, CultureInfo>();
        private int maxLengthOfStringProperty = int.MaxValue;


        Dictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.SerializationForType
        { get => exactSerializationForType;}

        Dictionary<string, Func<object, string>> IPrintingConfig<TOwner>.SerializationForProperty
        { get => exactSerializationForProperty; }

        Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.NumberTypesCultures
        {
            get => numberTypesCultures;
        }

        int IPrintingConfig<TOwner>.MaxLengthOfStringProperty
        {
            get => maxLengthOfStringProperty;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Length of string properties must be nonnegative");
                maxLengthOfStringProperty = value;
            }
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
            if (ExcludedTypes.Contains(obj.GetType()))
                return string.Empty;
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
            {
                if (numberTypesCultures.ContainsKey(obj.GetType())) //Now it's only for int
                {
                    Thread.CurrentThread.CurrentCulture = numberTypesCultures[obj.GetType()];
                    Thread.CurrentThread.CurrentUICulture = numberTypesCultures[obj.GetType()];
                }
                if (exactSerializationForType.ContainsKey(obj.GetType()))
                    return exactSerializationForType[obj.GetType()](obj)
                           + Environment.NewLine;
                if (obj.GetType() == typeof(string)) 
                    return obj.ToString().Substring(0, Math.Min(maxLengthOfStringProperty,
                        obj.ToString().Length)) + Environment.NewLine;
                return obj + Environment.NewLine;
            }
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (ExcludedProperties.Contains((propertyInfo as MemberInfo).Name))
                    continue;
                if (ExcludedTypes.Contains(propertyInfo.PropertyType))
                    continue;
                if (numberTypesCultures.ContainsKey(propertyInfo.GetType()))
                {
                    Thread.CurrentThread.CurrentCulture = numberTypesCultures[propertyInfo.GetType()];
                    Thread.CurrentThread.CurrentUICulture = numberTypesCultures[propertyInfo.GetType()];
                }
                if (exactSerializationForProperty.ContainsKey(propertyInfo.Name))
                    sb.Append(exactSerializationForProperty[propertyInfo.Name](
                        propertyInfo.GetValue(obj)));
                if (exactSerializationForType.ContainsKey(propertyInfo.PropertyType))
                    sb.Append(exactSerializationForType[propertyInfo.PropertyType]
                        (propertyInfo.GetValue(obj)));
                else
                    sb.Append(identation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj),
                                  nestingLevel + 1));
            }
            return sb.ToString();
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            ExcludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            ExcludedProperties.Add((((MemberExpression)func.Body).Member as PropertyInfo).Name);
            return this;
        }

        public PropertySerializingConfig<TOwner, T> Serializing<T>()
        {
            return new PropertySerializingConfig<TOwner, T>(this);
        }

        public PropertySerializingConfig<TOwner, T> Serializing<T>(Expression<Func<TOwner, T>> func)
        {
            var propertyName = (((MemberExpression)func.Body).Member as PropertyInfo).Name;
            return new PropertySerializingConfig<TOwner, T>(this, propertyName);
        }
    }

    public class PropertySerializingConfig<TOwner, TPropType>
        : IPropertySerializingConfig<TOwner>
    {
        private PrintingConfig<TOwner> parentConfig;
        private string propertyName = null;

        PrintingConfig<TOwner> IPropertySerializingConfig<TOwner>.ParentConfig => parentConfig;

        public PropertySerializingConfig(
            PrintingConfig<TOwner> parentConfig, string propertyName=null)
        {
            this.parentConfig = parentConfig;
            this.propertyName = propertyName;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializationFunc)
        {
            if (propertyName == null)
                (parentConfig as IPrintingConfig<TOwner>).SerializationForType[typeof(TPropType)]
                    = serializationFunc as Func<object, string>;
            else
                (parentConfig as IPrintingConfig<TOwner>).SerializationForProperty[propertyName]
                    = serializationFunc as Func<object, string>;
            return parentConfig;
        }
    }

    public static class PropertySerializingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this //Now it's only for int
            PropertySerializingConfig<TOwner, int> config,
            CultureInfo cultureInfo) //Now it's only for int
        {
            ((config as IPropertySerializingConfig<TOwner>).ParentConfig
                as IPrintingConfig<TOwner>).NumberTypesCultures[typeof(int)] = cultureInfo;
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> CutPrefix<TOwner>(this
                PropertySerializingConfig<TOwner, string> config,
            int prefixLen)
        {
            ((config as IPropertySerializingConfig<TOwner>).ParentConfig
                as IPrintingConfig<TOwner>).MaxLengthOfStringProperty = prefixLen;
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }
    }

    public interface IPropertySerializingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }

    public interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, Func<object, string>> SerializationForType { get; }
        Dictionary<string, Func<object, string>> SerializationForProperty { get; }
        Dictionary<Type, CultureInfo> NumberTypesCultures { get; }
        int MaxLengthOfStringProperty { get; set; }
    }

    public static class ObjectExtensions
    {
        public static PrintingConfig<TOwner> Serialize<TOwner>(this TOwner owner)
        {
            return new PrintingConfig<TOwner>();
        }
    }
}