using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.PrintingConfiguration
{
    public class PrintingConfig<TOwner>
    {
        private List<Type> excludedTypes;
        private List<MemberInfo> excludedProperties;
        private Dictionary<Type, Delegate> customTypesSerialization;
        private Dictionary<MemberInfo, Delegate> customPropertiesSerialization;
        private Dictionary<MemberInfo, int> stringMaxLengths;
        private Dictionary<Type, CultureInfo> differentCultures;
        private PropertyInfo currentProperty;
        private HashSet<object> history;
        private bool ignoringCyclicReferences;

        private Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig()
        {
            excludedTypes = new List<Type>();
            excludedProperties = new List<MemberInfo>();
            customPropertiesSerialization = new Dictionary<MemberInfo, Delegate>();
            customTypesSerialization = new Dictionary<Type, Delegate>();
            stringMaxLengths = new Dictionary<MemberInfo, int>();
            differentCultures = new Dictionary<Type, CultureInfo>();
            history = new HashSet<object>();
            ignoringCyclicReferences = false;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public string PrintEnumerable(IEnumerable objects, int nestingLevel)
        {
            var result = new List<string>();
            foreach (var obj in objects)
            {
                var convertedObject = PrintToString(obj, nestingLevel + 1);
                result.Add(new string('\t', nestingLevel + 1) + convertedObject);
            }

            var str = string.Join("", result);
            return objects.GetType().Name + '{' + "\n" + str + new string('\t', nestingLevel) + '}';
        }


        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
            {
                return "null" + Environment.NewLine;
            }

            var type = obj.GetType();
            if (finalTypes.Contains(type) && !excludedTypes.Contains(type))
            {
                return HandleObjectWithFinalType(obj);
            }

            if (obj is IEnumerable enumerable)
            {
                return PrintEnumerable(enumerable, nestingLevel);
            }

            if (history.Contains(obj))
            {
                if (!ignoringCyclicReferences)
                {
                    throw new ArgumentException("Cyclic reference");
                }

                return "New cyclic reference detected";
            }

            history.Add(obj);
            var starter = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                currentProperty = propertyInfo;
                if (!excludedTypes.Contains(propertyInfo.PropertyType) && !excludedProperties.Contains(propertyInfo))
                {
                    sb.Append(starter + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
                }
            }

            history.Remove(obj);
            currentProperty = null;
            return sb.ToString();
        }

        private string HandleObjectWithFinalType(object obj)
        {
            var type = obj.GetType();
            if (customPropertiesSerialization.ContainsKey(currentProperty))
            {
                var func = customPropertiesSerialization[currentProperty];
                return func.DynamicInvoke(obj) + Environment.NewLine;
            }

            if (stringMaxLengths.ContainsKey(currentProperty))
            {
                var str = obj.ToString();
                var maxLength = stringMaxLengths[currentProperty];
                var result = str != null && str.Length > maxLength
                    ? str[..maxLength]
                    : str;
                return result + Environment.NewLine;
            }

            if (differentCultures.ContainsKey(type) && obj is IFormattable formatObj)
            {
                return formatObj.ToString(null, differentCultures[type]) + Environment.NewLine;
            }

            if (customTypesSerialization.ContainsKey(type))
            {
                var func = customTypesSerialization[type];
                return func.DynamicInvoke(obj) + Environment.NewLine;
            }

            return obj + Environment.NewLine;
        }

        public TypePrintingConfig<TOwner, TType> For<TType>()
        {
            return new TypePrintingConfig<TOwner, TType>(this);
        }

        public PropertyPrintingConfig<TOwner, TMemberInfo> For<TMemberInfo>(
            Expression<Func<TOwner, TMemberInfo>> memberSelector)
        {
            var member = ((MemberExpression)memberSelector.Body).Member;
            return new PropertyPrintingConfig<TOwner, TMemberInfo>(this, member);
        }

        public void IgnoreCyclicReference()
        {
            ignoringCyclicReferences = true;
        }

        protected internal void ExcludeType(Type type)
        {
            excludedTypes.Add(type);
        }

        protected internal void AddSerializationTypeRule(Type type, Delegate func)
        {
            customTypesSerialization[type] = func;
        }


        protected internal void ExcludeMember(MemberInfo memberInfo)
        {
            excludedProperties.Add(memberInfo);
        }

        protected internal void SetMaxLength(MemberInfo memberInfo, int length)
        {
            stringMaxLengths[memberInfo] = length;
        }

        protected internal void AddSerializationRule<TType>(MemberInfo memberInfo, Func<TType, string> func)
        {
            customPropertiesSerialization[memberInfo] = func;
        }

        protected internal void AddCultureUsing(Type type, CultureInfo invariantCulture)
        {
            differentCultures[type] = invariantCulture;
        }
    }
}