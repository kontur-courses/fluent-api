using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public record PrintingConfig<TOwner>(SerializationSettings Settings)
    {
        private static readonly ImmutableArray<Type> FinalTypes =
        [
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime),
            typeof(TimeSpan)
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
                throw new ArgumentException();
            var memberExpr = (MemberExpression)memberSelector.Body;
            if (memberExpr.Member.MemberType != MemberTypes.Field && memberExpr.Member.MemberType != MemberTypes.Property)
                throw new ArgumentException();
            
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

        private string PrintToString(object? obj, int nestingLevel, HashSet<object> searized)
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
            sb.AppendLine(type.Name);
            sb.Append(SerializeInstance(obj, nestingLevel, new string('\t', nestingLevel + 1), searized));
            return sb.ToString();
        }

        private StringBuilder SerializeInstance(object obj, int nestingLevel, string tabulation, HashSet<object> searized)
        {
            var result = new StringBuilder();
            
            foreach (var member in GetPropertiesAndFields(obj.GetType()))
            {
                var memberValue = GetValue(obj, member);
                if (!(memberValue is not null && Settings.ExcludedTypes.Contains(memberValue.GetType()) ||
                                                Settings.ExcludedPropertiesAndFields.Contains(member) || 
                                                searized.Contains(searized)))
                {
                    if (Settings.AlternativeSerializationOfFieldsAndProperties.TryGetValue(member, out var serializer))
                        result.Append(tabulation + member.Name + " = " + serializer(memberValue));
                    else
                    {
                        if (memberValue is not null) searized.Add(memberValue);
                        result.Append(tabulation + member.Name + " = " + 
                                      PrintToString(memberValue, nestingLevel + 1, searized));
                    }
                }
            }

            return result;
        }

        private IEnumerable<MemberInfo> GetPropertiesAndFields(Type type)
        {
            foreach (var property in type.GetProperties())
                yield return property;
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                yield return field;
        }

        private object? GetValue(object obj, MemberInfo member)
        {
            if (member is PropertyInfo propertyInfo)
                return propertyInfo.GetValue(obj);
            if (member is FieldInfo fieldInfo)
                return fieldInfo.GetValue(obj);
            
            throw new ArgumentException();
        }
    }
}