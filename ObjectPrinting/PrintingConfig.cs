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
        public HashSet<Type> ExcludeMembersByType { get; set; }
        public HashSet<string> ExcludeMembersByName { get; set; }
        public Dictionary<Type, Delegate> AlternativeSerializationByType { get; set; }
        public Dictionary<string, Delegate> AlternativeSerializationByName { get; set; }
        public HashSet<object> ViewedObjects { get; set; }
        public Dictionary<string, Delegate> TrimmingFunctions { get; set; }
        public Dictionary<Type, CultureInfo> CultureInfoForNumbers { get; set; }

        public PrintingConfig()
        {
            ExcludeMembersByType = new HashSet<Type>();
            AlternativeSerializationByType = new Dictionary<Type, Delegate>();
            ViewedObjects = new HashSet<object>();
            AlternativeSerializationByName = new Dictionary<string, Delegate>();
            TrimmingFunctions = new Dictionary<string, Delegate>();
            ExcludeMembersByName = new HashSet<string>();
            CultureInfoForNumbers = new Dictionary<Type, CultureInfo>();
        }
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            var excludedType = typeof(TPropType);
            ExcludeMembersByType.Add(excludedType);
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> excludedExpression)
        {
            var excludedName = ((MemberExpression)excludedExpression.Body).Member.Name;
            ExcludeMembersByName.Add(excludedName);
            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> Serializer<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }


        public TypePrintingConfig<TOwner, TPropType> Serializer<TPropType>(Expression<Func<TOwner, TPropType>> alternativeExpression)
        {
            ITypePrintingConfig<TOwner> tPrintingConfig = new TypePrintingConfig<TOwner, TPropType>(this);
            tPrintingConfig.NameMember = ((MemberExpression)alternativeExpression.Body).Member.Name;
            return (TypePrintingConfig<TOwner, TPropType>)tPrintingConfig;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(type))
                return SetCultureInfoForNumber(obj) + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var members = FilteringMembers(type);
            var result = PrintMembers(obj, nestingLevel, identation, members);
            return result;
        }

        private object SetCultureInfoForNumber(object obj)
        {
            switch (obj)
            {
                case int intObj:
                    return CultureInfoForNumbers.ContainsKey(typeof(int))
                        ? intObj.ToString(CultureInfoForNumbers[typeof(int)])
                        : obj;
                case double doubleObj:
                    return CultureInfoForNumbers.ContainsKey(typeof(double))
                        ? doubleObj.ToString(CultureInfoForNumbers[typeof(double)])
                        : obj;
                case float floatObj:
                    return CultureInfoForNumbers.ContainsKey(typeof(float))
                        ? floatObj.ToString(CultureInfoForNumbers[typeof(float)])
                        : obj;
                default:
                    return obj;
            }
        }

        private MemberInfo[] FilteringMembers(Type type)
        {
            return type.GetMembers()
                .Where(member => (member.MemberType & MemberTypes.Property) != 0 ||
                                                               (member.MemberType & MemberTypes.Field) != 0)
                .Where(member => !(ExcludeMembersByType.Contains(GetType(member))
                                                  || ExcludeMembersByName.Contains(member.Name))).ToArray();
        }

        //Отрефакторить, когда будет полная реализация
        private string PrintMembers(object obj, int nestingLevel, string identation, MemberInfo[] members)
        {
            var sb = new StringBuilder();
            sb.AppendLine(obj.GetType().Name);
            foreach (var memberInfo in members)
            {
                var value = GetValue(memberInfo, obj);
                var propertyType = GetType(memberInfo);
                if (ViewedObjects.Contains(value))
                    continue;
                ViewedObjects.Add(value);
                if (value is string str && TrimmingFunctions.ContainsKey(memberInfo.Name))
                {
                    value = TrimmingFunctions[memberInfo.Name].DynamicInvoke(str);
                }
                if (AlternativeSerializationByType.ContainsKey(propertyType))
                {
                    var result = AlternativeSerializationByType[propertyType].DynamicInvoke(value);
                    sb.Append(identation + result + "\r\n");
                    continue;
                }
                if (AlternativeSerializationByName.ContainsKey(memberInfo.Name))
                {
                    var result = AlternativeSerializationByName[memberInfo.Name].DynamicInvoke(value);
                    sb.Append(identation + result + "\r\n");
                    continue;
                }
                sb.Append(identation + memberInfo.Name + " = " +
                              PrintToString(value, nestingLevel + 1));
            }

            return sb.ToString();
        }

        private Type GetType(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.PropertyType;
            }

            var fieldInfo = (FieldInfo)memberInfo;
            return fieldInfo.FieldType;
        }

        private object GetValue(MemberInfo memberInfo, object obj)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetValue(obj);
            }

            var fieldInfo = (FieldInfo)memberInfo;
            return fieldInfo.GetValue(obj);
        }
    }
}