using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectPrinting
{
    public record PrintingConfig<TOwner>(SerializationSettings Settings)
    {
        private static readonly ImmutableArray<Type> FinalTypes =
        [
            typeof(int), typeof(double), typeof(float), typeof(long), typeof(ulong), typeof(short),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(string),
            typeof(bool),
            typeof(char),
        ];
        
        public PrintingConfig() : this(new SerializationSettings())
        {
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, typeof(TPropType));
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfo = GetMemberInfo(memberSelector);
            
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberInfo);
        }

        private MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException("memberSelector должен возвращать поле или свойство объекта");
            var memberExpr = (MemberExpression)memberSelector.Body;
            if (memberExpr.Member.MemberType != MemberTypes.Field && memberExpr.Member.MemberType != MemberTypes.Property)
                throw new ArgumentException("memberSelector должен возвращать поле или свойство объекта");
            
            return memberExpr.Member;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PrintingConfig<TOwner>(
                Settings with
                {
                    ExcludedPropertiesAndFields = Settings.ExcludedPropertiesAndFields.Add(GetMemberInfo(memberSelector))
                });
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            return new PrintingConfig<TOwner>(Settings with
            {
                ExcludedTypes = Settings.ExcludedTypes.Add(typeof(TPropType))
            });
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, new HashSet<object>());
        }

        private string PrintToString(object? obj, int nestingLevel, HashSet<object> serialized)
        {
            if (obj is null)
                return "null" + Environment.NewLine;
            
            var type = obj.GetType();
            if (Settings.AlternativeTypeSerialization.TryGetValue(type, out var serializer) &&
                !Settings.ExcludedTypes.Contains(type))
            {
                return serializer(obj) + Environment.NewLine;
            }
            if (FinalTypes.Contains(type))
                return obj + Environment.NewLine;
            
            var sb = new StringBuilder();
            var tabulation = new string('\t', nestingLevel + 1);
            sb.AppendLine(SerializeNameOfType(type));
            sb.Append(SerializeInstance(obj, nestingLevel, tabulation, serialized));
            if (obj is IEnumerable) sb.Append(SerializeСollection(obj, nestingLevel, tabulation, serialized));
            return sb.ToString();
        }

        private string SerializeNameOfType(Type type)
        {
            if (!type.IsGenericType) return type.Name;
            
            var separator = ", ";
            var name = new Regex("(`[0-9]+)$").Replace(type.Name, "");
            
            return $"{name}<{string.Join(separator, type.GenericTypeArguments.Select(SerializeNameOfType))}>";
        }

        private StringBuilder SerializeInstance(object obj, int nestingLevel, string tabulation, HashSet<object> serialized)
        {
            var result = new StringBuilder();
            
            foreach (var member in GetPropertiesAndFields(obj.GetType()))
            {
                // если member индексатор, то пропускаем
                if (member.Name == "Item") continue;
                
                var memberValue = GetValue(obj, member);
                if (!(memberValue is not null && Settings.ExcludedTypes.Contains(memberValue.GetType()) ||
                                                 Settings.ExcludedPropertiesAndFields.Contains(member) || 
                                                 serialized.Contains(memberValue)))
                {
                    if (Settings.AlternativeSerializationOfFieldsAndProperties.TryGetValue(member, out var serializer))
                        result.Append(tabulation + member.Name + " = " + serializer(memberValue));
                    else
                    {
                        if (memberValue is not null) serialized.Add(memberValue);
                        result.Append(tabulation + member.Name + " = " + 
                                      PrintToString(memberValue, nestingLevel + 1, serialized));
                    }
                }
            }

            return result;
        }

        private StringBuilder SerializeСollection(object obj,
            int nestingLevel, string tabulation, HashSet<object> searized)
        {
            var result = new StringBuilder();
            result.Append($"{new string('\t', nestingLevel + 1)}collection items:{Environment.NewLine}");
            tabulation = new string('\t', nestingLevel + 2);
            
            foreach (var elem in (IEnumerable)obj)
            {
                if (elem is not null) searized.Add(elem);
                result.Append(tabulation + PrintToString(elem, nestingLevel + 3, searized));
            }

            return result;
        }

        private IEnumerable<MemberInfo> GetPropertiesAndFields(Type type)
        {
            foreach (var property in type.GetProperties())
                yield return property;
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                yield return field;
        }

        private object? GetValue(object obj, MemberInfo member)
        {
            if (member is PropertyInfo propertyInfo)
                return propertyInfo.GetValue(obj);
            if (member is FieldInfo fieldInfo)
                return fieldInfo.GetValue(obj);
            
            throw new ArgumentException("member должен быть полем или свойством");
        }
    }
}