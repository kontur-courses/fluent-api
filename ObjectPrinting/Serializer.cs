using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting
{
    public class Serializer
    {
        private readonly Dictionary<object, int> complexObjectLinks = new Dictionary<object, int>();

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private readonly HashSet<MemberInfo> excludedMembers;
        private readonly HashSet<Type> excludedTypes;
        private readonly Dictionary<MemberInfo, Func<object, string>> membersSerializesInfos;
        private readonly Dictionary<Type, Func<object, string>> typeSerializesInfos;
        private readonly int maxRecursion = 2;

        public Serializer(
            HashSet<MemberInfo> excludedMembers, 
            HashSet<Type> excludedTypes,
            Dictionary<MemberInfo, Func<object, string>> membersSerializesInfos,
            Dictionary<Type, Func<object, string>> typeSerializesInfos,
            int maxRecursion)
        {                     
            this.typeSerializesInfos = typeSerializesInfos;
            this.excludedMembers = excludedMembers;
            this.excludedTypes = excludedTypes;
            this.membersSerializesInfos = membersSerializesInfos;
            this.maxRecursion = maxRecursion;
        }

        public string Serialize(object obj, int nestingLevel)
        {
            if (obj == null)
                return $"null{Environment.NewLine}";

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            if (MaxRecursionHasBeenReached(obj))
                return $"Maximum recursion has been reached{Environment.NewLine}";

            var indentation = string.Intern(new string('\t', nestingLevel + 1));

            var type = obj.GetType();
            var sb = new StringBuilder().AppendLine(type.Name);

            HandleMembers(type, sb, indentation, obj, nestingLevel);

            return sb.ToString();

        }

        private void HandleMembers(Type type, StringBuilder sb, string indentation, object obj, int nestingLevel)
        {
            foreach (var propertyInfo in type.GetProperties())
            {
                var data = new DataMember(propertyInfo);
                var serializedValue = HandleMember(sb, data, obj, indentation, nestingLevel);
                if (serializedValue != null)
                    sb.Append(serializedValue);
            }

            foreach (var fieldInfo in type.GetFields())
            {
                var data = new DataMember(fieldInfo);
                var serializedValue = HandleMember(sb, data, obj, indentation, nestingLevel);
                if (serializedValue != null)
                    sb.Append(serializedValue);
            }
        }

        private string HandleMember(StringBuilder sb, DataMember member, object obj, string indentation,
            int nestingLevel)
        {
            if (excludedMembers.Any(memberInfo => memberInfo.Name == member.Name) ||
                excludedTypes.Contains(member.Type))
                return null;

            if (membersSerializesInfos.TryGetValue(member.MemberInfo, out var serializeMember))
                return GetSerializedString(obj, member, indentation, serializeMember);

            if (typeSerializesInfos.TryGetValue(member.Type, out var serializeType))
                return GetSerializedString(obj, member, indentation, serializeType);

            return GetSerializedString(
                obj,
                member,
                indentation,
                value => Serialize(
                    value,
                    nestingLevel + 1),
                false);
        }

        private string GetSerializedString(
            object obj,
            DataMember memberInfo,
            string indentation,
            Func<object, string> serializeMember,
            bool needNewLine = true)
        {
            var serializedString = serializeMember(memberInfo.GetValue(obj));
            var stringEnd = needNewLine ? Environment.NewLine : string.Empty;

            return $"{indentation}{memberInfo.Name} = {serializedString}{stringEnd}";
        }

        private bool MaxRecursionHasBeenReached(object obj)
        {
            complexObjectLinks.TryAdd(obj, 0);
            complexObjectLinks[obj]++;

            return complexObjectLinks[obj] == maxRecursion;
        }
    }
}