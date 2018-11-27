using System;
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
        private HashSet<Type> excludeMembersByType;
        private HashSet<string> excludeMembersByName;
        private Dictionary<Type, Delegate> alternativeSerializationByType;
        private Dictionary<string, Delegate> alternativeSerializationByName;
        private HashSet<object> viewedObjects;
        private Dictionary<string, Delegate> trimmingFunctions;
        private Dictionary<Type, CultureInfo> cultureInfoForNumbers;

        public PrintingConfig()
        {
            excludeMembersByType = new HashSet<Type>();
            alternativeSerializationByType = new Dictionary<Type, Delegate>();
            viewedObjects = new HashSet<object>();
            alternativeSerializationByName = new Dictionary<string, Delegate>();
            trimmingFunctions = new Dictionary<string, Delegate>();
            excludeMembersByName = new HashSet<string>();
            cultureInfoForNumbers = new Dictionary<Type, CultureInfo>();
        }
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            var excludedType = typeof(TPropType);
            excludeMembersByType.Add(excludedType);
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> excludedExpression)
        {
            var excludedName = ((MemberExpression)excludedExpression.Body).Member.Name;
            excludeMembersByName.Add(excludedName);
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

            var indentation = new string('\t', nestingLevel + 1);
            var members = FilteringMembers(type);
            var result = PrintMembers(obj, nestingLevel, indentation, members);
            return result;
        }

        private object SetCultureInfoForNumber(object obj)
        {
            switch (obj)
            {
                case int intObj:
                    return cultureInfoForNumbers.ContainsKey(typeof(int))
                        ? intObj.ToString(cultureInfoForNumbers[typeof(int)])
                        : obj;
                case double doubleObj:
                    return cultureInfoForNumbers.ContainsKey(typeof(double))
                        ? doubleObj.ToString(cultureInfoForNumbers[typeof(double)])
                        : obj;
                case float floatObj:
                    return cultureInfoForNumbers.ContainsKey(typeof(float))
                        ? floatObj.ToString(cultureInfoForNumbers[typeof(float)])
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
                .Where(member => !(excludeMembersByType.Contains(GetType(member))
                                                  || excludeMembersByName.Contains(member.Name))).ToArray();
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
                if (viewedObjects.Contains(value))
                    continue;
                viewedObjects.Add(value);
                if (value is string str && trimmingFunctions.ContainsKey(memberInfo.Name))
                {
                    value = trimmingFunctions[memberInfo.Name].DynamicInvoke(str);
                }
                if (alternativeSerializationByType.ContainsKey(propertyType))
                {
                    var result = alternativeSerializationByType[propertyType].DynamicInvoke(value);
                    sb.Append(identation + result + "\r\n");
                    continue;
                }
                if (alternativeSerializationByName.ContainsKey(memberInfo.Name))
                {
                    var result = alternativeSerializationByName[memberInfo.Name].DynamicInvoke(value);
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

        HashSet<Type> IPrintingConfig<TOwner>.ExcludeMembersByType
        {
            get => excludeMembersByType;
            set => excludeMembersByType = value;
        }
        HashSet<string> IPrintingConfig<TOwner>.ExcludeMembersByName
        {
            get => excludeMembersByName;
            set => excludeMembersByName = value;
        }
        Dictionary<Type, Delegate> IPrintingConfig<TOwner>.AlternativeSerializationByType
        {
            get => alternativeSerializationByType;
            set => alternativeSerializationByType = value;
        }
        Dictionary<string, Delegate> IPrintingConfig<TOwner>.AlternativeSerializationByName
        {
            get => alternativeSerializationByName;
            set => alternativeSerializationByName = value;
        }
        HashSet<object> IPrintingConfig<TOwner>.ViewedObjects
        {
            get => viewedObjects;
            set => viewedObjects = value;
        }
        Dictionary<string, Delegate> IPrintingConfig<TOwner>.TrimmingFunctions
        {
            get => trimmingFunctions;
            set => trimmingFunctions = value;
        }
        Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.CultureInfoForNumbers
        {
            get => cultureInfoForNumbers;
            set => cultureInfoForNumbers = value;
        }
    }
}