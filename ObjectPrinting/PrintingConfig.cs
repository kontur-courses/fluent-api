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
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<MemberInfo> excludingMembers;

        private readonly HashSet<Type> excludingTypes;

        private readonly Type[] finalTypes =
        {
            typeof(Guid), typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig()
        {
            AlternativePrintForMembers = new Dictionary<MemberInfo, Delegate>();
            MaxStringLength = new Dictionary<MemberInfo, int>();
            excludingMembers = new HashSet<MemberInfo>();
            excludingTypes = new HashSet<Type>();
            AlternativePrintingForTypes = new Dictionary<Type, Delegate>();
            AlternativeCulturesForTypes = new Dictionary<Type, CultureInfo>();
        }

        internal Dictionary<MemberInfo, int> MaxStringLength { get; }

        internal Dictionary<Type, Delegate> AlternativePrintingForTypes { get; }

        internal Dictionary<MemberInfo, Delegate> AlternativePrintForMembers { get; }

        internal Dictionary<Type, CultureInfo> AlternativeCulturesForTypes { get; }

        public TypePrintingConfig<TOwner, TPropType> Print<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Print<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = memberSelector.Body as MemberExpression;
            var propertyInfo = member!.Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludingTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = memberSelector.Body as MemberExpression;
            var memberInfo = member!.Member;
            excludingMembers.Add(memberInfo);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, new HashSet<object>());
        }

        private string PrintToString(object obj, int nestingLevel, HashSet<object> objects)
        {
            if (objects.Count > 10)
                return obj.GetType().Name;

            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (IsNonNestedCase(obj, objects))
                return GetNonNestedCaseStrings(obj, objects);

            objects.Add(obj);

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder(type.Name+Environment.NewLine);

            var properties = type.GetProperties().Where(p =>
                !excludingTypes.Contains(p.PropertyType) && !excludingMembers.Contains(p));

            var fields = type.GetFields()
                .Where(f => !excludingTypes.Contains(f.FieldType) &&
                            !excludingMembers.Contains(f) && f.IsPublic &&
                            (type.IsClass || !f.IsStatic));

            foreach (var propertyInfo in properties)
                sb.Append(indentation + propertyInfo.Name + " = " +
                          GetMemberInfoStrings(propertyInfo.GetValue(obj), propertyInfo, objects));

            foreach (var fieldsInfo in fields)
                sb.Append(indentation + fieldsInfo.Name + " = " +
                          GetMemberInfoStrings(fieldsInfo.GetValue(obj), fieldsInfo, objects));

            return sb.ToString();
        }

        private bool IsNonNestedCase(object obj, HashSet<object> objects)
        {
            return AlternativePrintingForTypes.ContainsKey(obj.GetType()) ||
                   AlternativeCulturesForTypes.ContainsKey(obj.GetType()) ||
                   finalTypes.Contains(obj.GetType()) ||
                   objects.Any(o => ReferenceEquals(o, obj)) ||
                   obj is IEnumerable;
        }

        private string GetNonNestedCaseStrings(object obj, HashSet<object> objects)
        {
            var type = obj.GetType();

            if (AlternativePrintingForTypes.ContainsKey(type))
                return AlternativePrintingForTypes[obj.GetType()].DynamicInvoke(obj)!.ToString();

            if (AlternativeCulturesForTypes.ContainsKey(type))
            {
                var result = obj as IFormattable;
                var culture = AlternativeCulturesForTypes[type];
                return result!.ToString(null, culture);
            }

            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            if (objects.Any(o => ReferenceEquals(o, obj)))
                return type.Name;

            if (obj is IEnumerable enumerable) return GetIEnumerable(enumerable, objects);

            return type.Name;
        }

        private string GetMemberInfoStrings(object obj, MemberInfo memberInfo, HashSet<object> objects)
        {
            var str = "";
            if (AlternativePrintForMembers.ContainsKey(memberInfo))
                return AlternativePrintForMembers[memberInfo].DynamicInvoke(obj)! + "\r\n";
            str += PrintToString(obj, objects.Count + 1, objects);

            if (MaxStringLength.ContainsKey(memberInfo))
                str = str.Substring(0, MaxStringLength[memberInfo]) + "\r\n";

            return str;
        }

        private string GetIEnumerable(IEnumerable enumerable, HashSet<object> objects)
        {
            var sb = new StringBuilder(enumerable.GetType().Name + "{");
            if (enumerable is IDictionary dictionary)
                foreach (var key in dictionary.Keys)
                    sb.Append($"[{key}] = {PrintToString(dictionary[key], objects.Count + 1, objects)}");
            else
                foreach (var item in enumerable)
                    sb.Append($"{PrintToString(item, objects.Count + 1, objects)}");

            sb.Append(" }");
            return sb.ToString();
        }
    }
}