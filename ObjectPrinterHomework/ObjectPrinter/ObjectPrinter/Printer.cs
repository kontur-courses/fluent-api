using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinter.ObjectPrinter
{
    public class Printer<T>
    {
        public readonly PrintingConfig<T> Config;

        public Printer(PrintingConfig<T> config)
        {
            Config = config;
        }
        
        public string? PrintToString(object? obj, int nestingLevel)
        {
            if (nestingLevel > 20)
                throw new TypeAccessException("Max nesting level reached. Possible cycle reference");
            
            if (obj is null)
                return "null\n";

            CultureInfo.CurrentCulture = 
                Config.CultureForTypes.TryGetValue(obj.GetType(), out var cultureForType)
                    ? cultureForType 
                    : CultureInfo.InstalledUICulture;

            if (Config.CustomTypeSerializer.TryGetValue(obj.GetType(), out var customTypeSerializer))
                return customTypeSerializer(obj);

            if (PrintFinalType(obj) != null)
                return PrintFinalType(obj);

            if (obj.GetType().IsPrimitive || obj.GetType().IsValueType)
                return obj + "\n";

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            
            
            foreach (var memberInfo in type.GetMembers())
            {
                if (MustBeSkipped(memberInfo))
                    continue;

                if (!TryGetMemberNameAndValue(obj, memberInfo, out var memberName, out var memberValue))
                    continue;
                
                sb.Append(indentation + memberName + " = ");
                
                string? valueString = 
                    Config.CustomMemberSerializer.TryGetValue(memberInfo, out var customMemberSerializer)
                    ? customMemberSerializer(memberValue)
                    : PrintToString(memberValue, nestingLevel + 1);

                if (Config.TrimForMembers.TryGetValue(memberInfo, out var trimLen))
                    valueString = string.Join("", (valueString ?? "").Take(trimLen));

                sb.Append(valueString);
                
                if (sb[^1] != '\n')
                    sb.Append('\n');

            }
            return sb.ToString();
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
        
        private bool MustBeSkipped(MemberInfo memberInfo)
        {
            return Config.ExcludingMembers.Contains(memberInfo)
                   ||  GetFieldOrPropertyType(memberInfo) is null
                   ||  Config.ExcludingTypes.Contains( GetFieldOrPropertyType(memberInfo)!);
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
    }
}