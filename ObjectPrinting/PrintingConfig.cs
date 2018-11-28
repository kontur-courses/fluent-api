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
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedPropertyTypes = new HashSet<Type>();
        private readonly HashSet<string> excludedPropertyNames = new HashSet<string>();
        private readonly HashSet<int> hashCodes = new HashSet<int>();
        private readonly Dictionary<string, int> propertiesToTrim = new Dictionary<string, int>();
        private readonly Dictionary<Type, CultureInfo> typesWithCulture = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<Type, Delegate> typesWithSpecificPrint = new Dictionary<Type, Delegate>();
        private readonly Dictionary<string, Delegate> namesWithSpecificPrint = new Dictionary<string, Delegate>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyName = ((MemberExpression)memberSelector.Body).Member.Name;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyName);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedPropertyNames.Add(((MemberExpression)memberSelector.Body).Member.Name);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedPropertyTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (obj.GetType().Namespace == nameof(System))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            if (hashCodes.Contains(obj.GetHashCode()))
                return sb.ToString();
            hashCodes.Add(obj.GetHashCode());

            var propertyInfos = type.GetProperties().Where(p => !excludedPropertyTypes.Contains(p.PropertyType)
                                                                && !excludedPropertyNames.Contains(p.Name));

            foreach (var propertyInfo in propertyInfos)
            {
                var propertyValue = propertyInfo.GetValue(obj);
                if (propertyValue is ICollection collection)
                {
                    sb.Append(identation + propertyInfo.Name + " = " + collection.GetType().Name + Environment.NewLine);
                    var count = 0;
                    foreach (var e in collection)
                    {
                        if (++count > 3)
                        {
                            sb.Append(identation + '\t' + "..." + Environment.NewLine);
                            break;
                        }
                        sb.Append(identation + '\t' + PrintToString(e, 0));
                    }

                }
                else
                {
                    if (propertyValue != null && propertyValue.GetType().Namespace == nameof(System))
                        propertyValue = ApplyConfig(propertyValue, propertyInfo);
                    sb.Append(identation + propertyInfo.Name + " = " +
                              PrintToString(propertyValue, nestingLevel + 1));
                }

            }
            return sb.ToString();
        }

        private string Trim(string propertyName, string value)
        {
            if (value == null)
                return null;
            if (propertiesToTrim.ContainsKey(propertyName))
            {
                var requiredLength = propertiesToTrim[propertyName];
                var resultLength = Math.Min(requiredLength, value.Length);
                return value.Substring(0, resultLength);
            }

            return value;
        }

        private object ApplyCulture(object obj)
        {
            if (typesWithCulture.ContainsKey(obj.GetType()))
                obj = string.Format(typesWithCulture[obj.GetType()], "{0}", obj);
            return obj;
        }

        private object ApplySpecificPrint(object obj, PropertyInfo propertyInfo)
        {
            if (namesWithSpecificPrint.ContainsKey(propertyInfo.Name))
            {
                var func = namesWithSpecificPrint[propertyInfo.Name];
                return func.DynamicInvoke(obj);
            }

            if (typesWithSpecificPrint.ContainsKey(obj.GetType()))
            {
                var func = typesWithSpecificPrint[obj.GetType()];
                return func.DynamicInvoke(obj);
            }

            return obj;
        }

        private string ApplyConfig(object obj, PropertyInfo propertyInfo)
        {
            if (obj == null)
                return "null";
            object result;
            result = ApplySpecificPrint(obj, propertyInfo);
            result = ApplyCulture(result);
            result = Trim(propertyInfo.Name, result.ToString());

            return result.ToString();
        }

        Dictionary<string, int> IPrintingConfig<TOwner>.PropertiesToTrim => propertiesToTrim;
        Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.TypesWithCulture => typesWithCulture;
        Dictionary<Type, Delegate> IPrintingConfig<TOwner>.TypesWithSpecificPrint => typesWithSpecificPrint;
        Dictionary<string, Delegate> IPrintingConfig<TOwner>.NamesWithSpecificPrint => namesWithSpecificPrint;
    }

    public interface IPrintingConfig<TOwner>
    {
        Dictionary<string, int> PropertiesToTrim { get; }
        Dictionary<Type, CultureInfo> TypesWithCulture { get; }
        Dictionary<Type, Delegate> TypesWithSpecificPrint { get; }
        Dictionary<string, Delegate> NamesWithSpecificPrint { get; }
    }
}