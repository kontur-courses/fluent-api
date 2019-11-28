using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public enum Mode
    {
        Property,
        Type
    }

    public class PrintingConfig<TOwner>
    {
        private readonly Type[] digitsTypes;
        private readonly HashSet<Type> excludingList;
        private readonly HashSet<PropertyInfo> excludingPropertyList;
        private readonly Type[] finalTypes;
        private readonly Dictionary<PropertyInfo, Func<object, string>> specialPropertySerialization;
        private readonly Dictionary<Type, Func<object, string>> specialSerialize;
        private readonly Dictionary<PropertyInfo, int> toTrim;

        public PrintingConfig()
        {
            finalTypes = new[]
            {
                typeof(int),
                typeof(double),
                typeof(float),
                typeof(string),
                typeof(DateTime),
                typeof(TimeSpan)
            };
            digitsTypes = new[]
            {
                typeof(double),
                typeof(float)
            };
            CultureForDigits = CultureInfo.CurrentCulture;
            specialSerialize = new Dictionary<Type, Func<object, string>>();
            specialPropertySerialization = new Dictionary<PropertyInfo, Func<object, string>>();
            excludingList = new HashSet<Type>();
            excludingPropertyList = new HashSet<PropertyInfo>();
            toTrim = new Dictionary<PropertyInfo, int>();
        }

        private int? LengthToTrim { get; set; }
        private CultureInfo CultureForDigits { get; set; }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, null);
        }

        public void AddSpecialTypeSerialize<T>(Type type, Func<T, string> value)
        {
            specialSerialize[type] = obj => value((T) obj);
        }

        public void SetCultureForDigits(CultureInfo culture)
        {
            CultureForDigits = culture;
        }

        public void AddPropertyToTrim(PropertyInfo info, int length)
        {
            toTrim[info] = length;
        }

        public void AddSpecialPropertySerialize<T>(PropertyInfo info, Func<T, string> value)
        {
            specialPropertySerialization[info] = obj => value((T) obj);
        }

        private string PrintToString(object obj, int nestingLevel, PropertyInfo lastPropertyInfo)
        {
            var result = TryToSetSettings(obj, lastPropertyInfo, nestingLevel);
            if (!(result is null))
                return PrintToString(result, nestingLevel, null);
            if (finalTypes.Contains(obj.GetType())) return obj + Environment.NewLine;
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludingList.Contains(propertyInfo.PropertyType) ||
                    excludingPropertyList.Contains(propertyInfo))
                    continue;
                lastPropertyInfo = propertyInfo;
                if (toTrim.ContainsKey(propertyInfo))
                    LengthToTrim = toTrim[propertyInfo];
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1, lastPropertyInfo));
                LengthToTrim = null;
            }

            return sb.ToString();
        }

        private string TryToSetSettings(object obj, PropertyInfo lastPropertyInfo, int nestingLevel)
        {
            var firstResult = TryToSetPrimarySettings(obj, lastPropertyInfo, nestingLevel);
            var tempResult = firstResult ?? obj;
            var finalResult = TryToSetSecondarySettings(tempResult);
            return finalResult ?? firstResult;
        }

        private string TryToSetPrimarySettings(object obj, PropertyInfo lastPropertyInfo, int nestingLevel)
        {
            return TryToSerializeNull(obj) ??
                   TryToExcluding(obj) ??
                   TryToSerializeEnumerableType(obj, nestingLevel) ??
                   TryToUseSpecialSerialize(obj) ??
                   TryToUseSpecialPropertySerialize(obj, lastPropertyInfo);
        }

        private string TryToSerializeEnumerableType(object obj, int nestingLevel)
        {
            if (obj is string || !(obj is IEnumerable collection)) return null;
            if (!collection.GetEnumerator().MoveNext())
                return "Empty collection";
            var sb = new StringBuilder();
            if (nestingLevel == 0)
                sb.Append(obj.GetType().Name);
            sb.Append(Environment.NewLine);
            var indentation = new string('\t', nestingLevel + 1);
            foreach (var item in collection)
                sb.Append(indentation + PrintToString(item, nestingLevel + 1, null));
            sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }

        private string TryToSetSecondarySettings(object obj)
        {
            return TryTrimStringProperty(obj) ??
                   TryToSerializeDigit(obj);
        }

        private string TryToSerializeDigit(object obj)
        {
            return digitsTypes.Contains(obj.GetType())
                ? ((double) obj).ToString(CultureForDigits)
                : null;
        }

        private string TryTrimStringProperty(object obj)
        {
            if (obj is string && !(LengthToTrim is null))
            {
                var result = obj.ToString().Substring(0, (int) LengthToTrim);
                LengthToTrim = null;
                return result;
            }

            return null;
        }

        private string TryToSerializeNull(object obj)
        {
            return obj is null
                ? "null"
                : null;
        }

        private string TryToExcluding(object obj)
        {
            return excludingList.Contains(obj.GetType()) ? string.Empty : null;
        }

        private string TryToUseSpecialSerialize(object obj)
        {
            return specialSerialize.ContainsKey(obj.GetType()) ? specialSerialize[obj.GetType()](obj) : null;
        }

        private string TryToUseSpecialPropertySerialize(object obj, PropertyInfo lastPropertyInfo)
        {
            return lastPropertyInfo != null && specialPropertySerialization.ContainsKey(lastPropertyInfo)
                ? specialPropertySerialization[lastPropertyInfo](obj)
                : null;
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludingList.Add(typeof(T));
            return this;
        }

        private static PropertyInfo GetPropertyFromExpression<TPropType>(Expression<Func<TOwner, TPropType>> expression)
        {
            if (!(expression.Body is MemberExpression memberExpression))
                throw new ArgumentException("Не является свойством объекта");
            return memberExpression.Member as PropertyInfo;
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> getProperty)
        {
            var propInfo = GetPropertyFromExpression(getProperty);
            excludingPropertyList.Add(propInfo);
            return this;
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this, Mode.Type);
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> func)
        {
            var propInfo = GetPropertyFromExpression(func);
            return new PropertyPrintingConfig<TOwner, T>(this, Mode.Property, propInfo);
        }
    }

    public interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        PropertyInfo Info { get; }
        Mode SelectedMode { get; }
    }

    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        private readonly PropertyInfo info;
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly Mode selectedMode;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, Mode mode)
        {
            selectedMode = mode;
            this.parentConfig = parentConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, Mode mode, PropertyInfo info)
        {
            selectedMode = mode;
            this.parentConfig = parentConfig;
            this.info = info;
        }

        Mode IPropertyPrintingConfig<TOwner>.SelectedMode => selectedMode;
        PropertyInfo IPropertyPrintingConfig<TOwner>.Info => info;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => parentConfig;

        public PrintingConfig<TOwner> Using(Expression<Func<TPropType, string>> func)
        {
            switch (selectedMode)
            {
                case Mode.Type:
                    parentConfig.AddSpecialTypeSerialize(typeof(TPropType), func.Compile());
                    break;
                case Mode.Property:
                    parentConfig.AddSpecialPropertySerialize(info, func.Compile());
                    break;
            }

            return parentConfig;
        }
    }

    public static class PrintingExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> config,
            CultureInfo info)
        {
            (config as IPropertyPrintingConfig<TOwner>).ParentConfig.SetCultureForDigits(info);
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> config,
            int count)
        {
            var propertyConfig = config as IPropertyPrintingConfig<TOwner>;
            propertyConfig.ParentConfig.AddPropertyToTrim(propertyConfig.Info, count);
            return propertyConfig.ParentConfig;
        }
    }
}