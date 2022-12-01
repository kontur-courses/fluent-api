using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinter.ObjectPrinter
{
    public class PrintingConfig<TOwner>
    {
        public readonly List<Type> ExcludingTypes = new();
        public readonly List<MemberInfo> ExcludingMembers = new();
        public readonly Dictionary<Type, CultureInfo> CultureForTypes = new();
        public readonly Dictionary<MemberInfo, CultureInfo> CultureForMembers = new();
        public readonly Dictionary<MemberInfo, Func<object?, string>> CustomMemberSerializer = new();
        public readonly Dictionary<Type, Func<object?, string>> CustomTypeSerializer = new();
        public readonly Dictionary<MemberInfo, int> TrimForMembers = new();
        
        
        public MemberConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new(this);
        }

        public MemberConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfo = ((MemberExpression) memberSelector.Body).Member;
            return new MemberConfig<TOwner, TPropType>(memberInfo, this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            ExcludingMembers.Add(((MemberExpression) memberSelector.Body).Member);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            ExcludingTypes.Add(typeof(TPropType));
            return this;
        }

        public string? PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string? PrintToString(object? obj, int nestingLevel)
        {
            if (nestingLevel > 20)
                throw new TypeAccessException("Max nesting level reached. Possible cycle reference");
            
            if (obj is null)
                return "null\n";

            CultureInfo.CurrentCulture = 
                CultureForTypes.TryGetValue(obj.GetType(), out var cultureForType)
                    ? cultureForType 
                    : CultureInfo.InstalledUICulture;

            if (CustomTypeSerializer.TryGetValue(obj.GetType(), out var customTypeSerializer))
                return customTypeSerializer(obj);

            if (PrintFinalType(obj) != null)
                return PrintFinalType(obj);

            if (obj.GetType().IsPrimitive || obj.GetType().IsValueType)
                return obj + "\n";

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            
            
            foreach (var memberInfo in type.GetMembers())
            {
                if (MustBeSkipped(memberInfo))
                    continue;

                if (!TryGetMemberNameAndValue(obj, memberInfo, out var memberName, out var memberValue))
                    continue;
                
                sb.Append(identation + memberName + " = ");
                
                string? valueString = CustomMemberSerializer.TryGetValue(memberInfo, out var customMemberSerializer)
                    ? customMemberSerializer(memberValue)
                    : PrintToString(memberValue, nestingLevel + 1);

                if (TrimForMembers.TryGetValue(memberInfo, out var trimLen))
                    valueString = string.Join("", (valueString ?? "").Take(trimLen));

                sb.Append(valueString);
                
                if (sb[^1] != '\n')
                    sb.Append('\n');

            }
            return sb.ToString();
        }

        private static bool TryGetMemberNameAndValue(object obj, MemberInfo memberInfo, out string? name, out object? value)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    var fieldInfo = (FieldInfo) memberInfo;
                    name = fieldInfo.Name;
                    value = fieldInfo.GetValue(obj);
                    break;

                case MemberTypes.Property:
                    var propertyInfo = (PropertyInfo) memberInfo;
                    name = propertyInfo.Name;
                    value = propertyInfo.GetValue(obj);
                    break;

                default:
                    name = null;
                    value = null;
                    return false;
            }

            return true;
        }

        public bool MustBeSkipped(MemberInfo memberInfo)
        {
            return ExcludingMembers.Contains(memberInfo)
                   || GetFieldOrPropertyType(memberInfo) is null
                   || ExcludingTypes.Contains(GetFieldOrPropertyType(memberInfo)!);
        }

        public string? PrintFinalType(object obj)
        {
            var finalTypes = new HashSet<Type>
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;
            return null;
        }
        private Type? GetFieldOrPropertyType(MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).PropertyType;
                default:
                    return null;
            }
        }
    }
}