using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class Serializer
    {
        private readonly SerializerSettings settings;
        private readonly HashSet<object> visited = new();
        private readonly HashSet<Type> finalTypes = new()
        {
            typeof(int), typeof(double), typeof(float),
            typeof(string), typeof(DateTime), typeof(TimeSpan),
            typeof(Guid), typeof(long), typeof(Enum)
        };

        public Serializer(SerializerSettings settings)
        {
            this.settings = settings;
        }

        public string PrintToString(object obj, int nestingLevel = 0)
        {
            if (obj == null)
                return FormatString("null");

            if (visited.Contains(obj))
            {
                return settings.IsAllowCyclingReference
                    ? FormatString("Cycle")
                    : throw new Exception("Unexpected cycle reference");
            }
            
            var type = obj.GetType();

            if (settings.TryGetTypeSerializer(type, out var serializer))
                return FormatString(serializer.Invoke(obj));

            if (finalTypes.Contains(type))
                return FormatString(obj.ToString());
            
            if (typeof(ICollection).IsAssignableFrom(type))
                return PrintCollection((ICollection)obj, nestingLevel);

            visited.Add(obj);
            
            var ident = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            var members = 
                type
                    .GetFieldsAndProperties()
                    .Where(t => !IsExcluded(t));
            foreach (var member in members)
            {
                sb.Append($"{ident}{ToString(member, obj, nestingLevel)}");
            }
            
            return sb.ToString();
        }
        
        private string PrintCollection(ICollection collection, int nestingLevel)
        {
            if (collection.Count == 0) 
                return FormatString("[]");
            var ident = new string('\t', nestingLevel);
            var values = new StringBuilder()
                .AppendLine()
                .AppendLine($"{ident}[");
            foreach (var element in collection)
            {
                values.Append($"{ident}\t{PrintToString(element, nestingLevel + 1)}");
            }

            values.AppendLine($"{ident}]");

            return values.ToString();
        }
        
        private string FormatString(string str) => $"{str}{Environment.NewLine}";
        
        private bool IsExcluded(MemberInfo memberInfo) =>
            settings.IsExcluded(memberInfo.GetMemberType()) || settings.IsExcluded(memberInfo);

        private string ToString(MemberInfo memberInfo, object obj, int nestingLevel) =>
            $"{memberInfo.Name} = {MemberValueToString(memberInfo, obj, nestingLevel)}";

        private string MemberValueToString(MemberInfo memberInfo, object obj, int nestingLevel)
        {
            var memberValue = memberInfo.GetValue(obj);
            if (settings.TryGetMemberSerializer(memberInfo, out var serializer))
                return FormatString(serializer.Invoke(memberValue));
            return PrintToString(memberValue, nestingLevel + 1);
        }
    }
}