using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using FluentAssertions.Common;

namespace ObjectPrinting
{
    public enum Mode
    {
        Property,
        Type
    }
    public class PrintingConfig<TOwner>
    {
        private HashSet<Type> ExludingList { get; set; }
        private Dictionary<Type,Func<object,string>> SpecialSerialize { get; set; }
        private HashSet<PropertyInfo> ExludingPropertyList { get; }
        private Dictionary<PropertyInfo,Func<object,string>> SpecialPropertySerrialize { get; set; }
        private Dictionary<PropertyInfo,int> ToTrim { get; }
        private int? lenghtToTrim { get; set; } = null;
        private CultureInfo cultureForDigits { get; set; }
        public PrintingConfig()
        {
            cultureForDigits = CultureInfo.CurrentCulture;
            SpecialSerialize= new Dictionary<Type, Func<object, string>>();
            SpecialPropertySerrialize = new Dictionary<PropertyInfo, Func<object, string>>();
            ExludingList=new HashSet<Type>();
            ExludingPropertyList = new HashSet<PropertyInfo>();
            ToTrim = new Dictionary<PropertyInfo, int>();
        }
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, null);
        }

        public void AddSpecialTypeSerialize<T>(Type type, Func<T, string> value)
        {
            Func<object, string> f = o => value((T) o);
            SpecialSerialize[type] = f;
        }

        public void SetCultureForDigits( CultureInfo culture)
        {
            cultureForDigits = culture;
        }

        public void AddPropertyToTrim(PropertyInfo info, int length)
        {
            ToTrim[info] = length;
        }

        public void AddSpecialPropertySerialize<T>(PropertyInfo info, Func<T, string> value)
        {
            Func<object, string> f = o => value((T) o);
            SpecialPropertySerrialize[info] = f;

        }

        private string PrintToString(object obj, int nestingLevel, PropertyInfo lastPropertyInfo)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };

            var digitsTypes = new[]
            {
                typeof(int), typeof(double), typeof(float)
            };

            if (SpecialSerialize.ContainsKey(obj.GetType()))
                return PrintToString(SpecialSerialize[obj.GetType()](obj), nestingLevel,null);

            if (lastPropertyInfo!=null &&  SpecialPropertySerrialize.ContainsKey(lastPropertyInfo))
                return PrintToString(SpecialPropertySerrialize[lastPropertyInfo](obj),nestingLevel,null);

            if (lastPropertyInfo != null && ToTrim.ContainsKey(lastPropertyInfo))
                return obj.ToString().Substring(0, ToTrim[lastPropertyInfo]) + Environment.NewLine;

            if (obj is string && !(lenghtToTrim is null))
                return obj.ToString().Substring(0, (int)lenghtToTrim) + Environment.NewLine;

            if (digitsTypes.Contains(obj.GetType())) return ((double)obj).ToString(cultureForDigits);

            if (finalTypes.Contains(obj.GetType())) return obj + Environment.NewLine;

            if (ExludingList.Contains(obj.GetType()))
                return string.Empty;
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if(ExludingList.Contains(propertyInfo.PropertyType) || ExludingPropertyList.Contains(propertyInfo))
                    continue;
                lastPropertyInfo = propertyInfo;
                if (ToTrim.ContainsKey(propertyInfo))
                    lenghtToTrim = ToTrim[propertyInfo];
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1, lastPropertyInfo));
                lenghtToTrim = null;
                lastPropertyInfo = null;
            }
            return sb.ToString();
        }

        private bool TrySetSettings (object obj, PropertyInfo lastPropertyInfo, int nestingLevel, out string result)
        {

            if (SpecialSerialize.ContainsKey(obj.GetType()))
            {
                result = SpecialSerialize[obj.GetType()](obj);
                return true;
            }
            if (lastPropertyInfo != null && SpecialPropertySerrialize.ContainsKey(lastPropertyInfo))
            {
                result = SpecialPropertySerrialize[lastPropertyInfo](obj);
                return true;
            }
            if (lastPropertyInfo != null && ToTrim.ContainsKey(lastPropertyInfo))
            {
                result = obj.ToString().Substring(0, ToTrim[lastPropertyInfo]) + Environment.NewLine;
                return true;
            }

            if (ExludingList.Contains(obj.GetType()))
            {
                result = string.Empty;
                return true;
            }

            result = null;
            return false;
        }

        public PrintingConfig<TOwner> Exluding<T>()
        {
            ExludingList.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Exluding<T>(Expression<Func<TOwner, T>> getProperty)
        {
            var propInfo = ((MemberExpression)getProperty.Body).Member as PropertyInfo;
            ExludingPropertyList.Add(propInfo);
            return this;
        }
        public PrintingConfig<TOwner> Serialize<T>(Func<T, string> f)
        {
            return this;
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this, Mode.Type);
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> func)
        {
            var propInfo = ((MemberExpression)func.Body).Member as PropertyInfo;

            return new PropertyPrintingConfig<TOwner, T>(this, Mode.Property, propInfo);
        }

    }

    public interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        Mode ChoosenMode { get; }
        PropertyInfo Info { get; }
    }
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        private Mode choosenMode;
        private PrintingConfig<TOwner> parentConfig;
        private PropertyInfo info = null;
        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, Mode mode)
        {
            this.choosenMode = mode;
            this.parentConfig = parentConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, Mode mode,PropertyInfo info)
        {
            this.choosenMode = mode;
            this.parentConfig = parentConfig;
            this.info = info;
        }

        public Mode ChoosenMode => choosenMode;

        public PropertyInfo Info => info;

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => parentConfig;

        public PrintingConfig<TOwner> Using(Expression<Func<TPropType, string>> func)
        {
            if (ChoosenMode==Mode.Type)
                parentConfig.AddSpecialTypeSerialize<TPropType>(typeof(TPropType), func.Compile());
            if(ChoosenMode==Mode.Property)
                parentConfig.AddSpecialPropertySerialize(info,func.Compile());
            return parentConfig;
        }
    }

    public static class PringtingExtensisons
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> config, CultureInfo info)
        {
             (config as IPropertyPrintingConfig<TOwner>).ParentConfig.SetCultureForDigits(info);
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> config, int count)
        {
            (config as IPropertyPrintingConfig<TOwner>).ParentConfig.AddPropertyToTrim(config.Info,count);
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

    }
}