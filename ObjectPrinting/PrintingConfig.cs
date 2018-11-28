using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FluentAssertions.Common;
using ObjectPrinting.PrintingInterfaces;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private List<Type> TypesToExclude;
        private List<PropertyInfo> PropsToExclude;

        Dictionary<Type, Delegate> IPrintingConfig.TypeCustomSerializers => typeCustomSerializers;

        Dictionary<PropertyInfo, Delegate> IPrintingConfig.PropertyCustomSerializers => propertyCustomSerializers;

        private int depth;
        private readonly int _indentSize;
        private readonly StringBuilder _stringBuilder;
        private readonly List<int> _hashListOfFoundElements;
        private readonly Dictionary<Type, Delegate> typeCustomSerializers;
        private readonly Dictionary<PropertyInfo, Delegate> propertyCustomSerializers;


        public PrintingConfig()
        {
            TypesToExclude = new List<Type>();
            PropsToExclude = new List<PropertyInfo>();
            typeCustomSerializers = new Dictionary<Type, Delegate>();
            propertyCustomSerializers = new Dictionary<PropertyInfo, Delegate>();
            _stringBuilder = new StringBuilder();
            _hashListOfFoundElements = new List<int>();
        }

        #region Fluent-API members

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            TypesToExclude.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> ExcludingProperty<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            PropsToExclude.Add(memberSelector.GetPropertyInfo());
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>() =>
            new PropertyPrintingConfig<TOwner, TPropType>(this, typeof(TPropType));

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector) =>
            new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector.GetPropertyInfo());

        public PrintingConfig<TOwner> SetNestingLevel(int nestingLevel)
        {
            this.depth = nestingLevel;
            return this;
        }

        #endregion

        #region Dumping

        public string PrintToString(TOwner obj) => Stringify(obj);

        private string Stringify(object obj)
        {
            if (obj == null || obj is ValueType || obj is string)
                Write(FormatValue(obj));
            else
            {
                var objectType = obj.GetType();
                Write("{{{0}}}", objectType.FullName);
                if (!typeof(IEnumerable).IsAssignableFrom(objectType))
                {
                    _hashListOfFoundElements.Add(obj.GetHashCode());
                    depth++;
                }

                var enumerableElement = obj as IEnumerable;
                if (enumerableElement != null)
                    foreach (object item in enumerableElement)
                        ExploreIEnumerableItem(item);
                else
                {
                    MemberInfo[] members = obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var memberInfo in members)
                        ExploreObjectMember(obj, memberInfo);
                }

                if (!typeof(IEnumerable).IsAssignableFrom(objectType))
                    depth--;
            }

            return _stringBuilder.ToString();
        }

        private void ExploreIEnumerableItem(object item)
        {
            if (item is IEnumerable && !(item is string))
            {
                depth++;
                Stringify(item);
                depth--;
            }
            else
            {
                if (!AlreadyTouched(item))
                    Stringify(item);
                else
                    Write("{{{0}}} <-- bidirectional reference found", item.GetType().FullName);
            }
        }

        private void ExploreObjectMember(object obj, MemberInfo memberInfo)
        {
            var fieldInfo = memberInfo as FieldInfo;
            var propertyInfo = memberInfo as PropertyInfo;

            if (fieldInfo == null && propertyInfo == null)
                return;

            //Если тип свойства или поля исключены из преобразования
            if (TypesToExclude.Contains(fieldInfo?.FieldType) ||
                TypesToExclude.Contains(propertyInfo?.PropertyType))
                return;

            //Если свойство исключено из преобразования
            if (PropsToExclude.Contains(propertyInfo))
                return;

            object value = fieldInfo != null
                ? fieldInfo.GetValue(obj)
                : propertyInfo.GetValue(obj, null);


            //если тип свойства имеет кастомную функцию для парсинга 
            if (fieldInfo != null && typeCustomSerializers.ContainsKey(fieldInfo.FieldType))
                value = typeCustomSerializers[fieldInfo.FieldType].DynamicInvoke(value);
            else if (propertyInfo != null && typeCustomSerializers.ContainsKey(propertyInfo.PropertyType))
                value = typeCustomSerializers[propertyInfo.PropertyType].DynamicInvoke(value);

            //если свойство имеет особенную функцию для парсинга
            if (propertyInfo != null && propertyCustomSerializers.ContainsKey(propertyInfo))
                value = propertyCustomSerializers[propertyInfo].DynamicInvoke(value);


            var type = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;
            if (type.IsValueType || type == typeof(string))
                Write("{0}: {1}", memberInfo.Name, FormatValue(value));
            else
            {
                var isEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
                Write("{0}: {1}", memberInfo.Name, isEnumerable ? "..." : "{ }");

                var alreadyTouched = !isEnumerable && AlreadyTouched(value);
                depth++;
                if (!alreadyTouched)
                    Stringify(value);
                else
                    Write("{{{0}}} <-- bidirectional reference found", value.GetType().FullName);
                depth--;
            }
        }


        private bool AlreadyTouched(object value)
        {
            if (value == null)
                return false;
            var hash = value.GetHashCode();
            return _hashListOfFoundElements.Any(t => t == hash);
        }

        private void Write(string value, params object[] args)
        {
            var space = new string(' ', depth * _indentSize);
            if (args != null)
                value = string.Format(value, args);
            _stringBuilder.AppendLine(space + value);
        }

        private string FormatValue(object o)
        {
            if (o == null)
                return ("null");
            if (o is DateTime)
                return (((DateTime) o).ToShortDateString());
            if (o is string)
                return string.Format("\"{0}\"", o);
            if (o is char && (char) o == '\0')
                return string.Empty;
            if (o is ValueType)
                return (o.ToString());
            if (o is IEnumerable<TOwner>)
                return ("...");

            return ("{ }");
        }

        #endregion
    }

   
}