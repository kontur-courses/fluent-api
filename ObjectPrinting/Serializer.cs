using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class Serializer
    {
        private const BindingFlags SerializingMembersFlag = BindingFlags.Public | BindingFlags.Instance;
        private readonly HashSet<MemberInfo> excludedMembers;
        private readonly HashSet<Type> excludedTypes;

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private readonly int maxRecursion;
        private readonly Dictionary<MemberInfo, IHasSerializationFunc> membersSerializesInfos;
        private readonly Dictionary<Type, IHasSerializationFunc> typeSerializesInfos;
        private readonly Func<object, string> handleMaxRecursion;

        public Serializer(
            HashSet<MemberInfo> excludedMembers,
            HashSet<Type> excludedTypes,
            Dictionary<MemberInfo, IHasSerializationFunc> membersSerializesInfos,
            Dictionary<Type, IHasSerializationFunc> typeSerializesInfos,
            int maxRecursion,
            Func<object, string> handleMaxRecursion)
        {
            this.typeSerializesInfos = typeSerializesInfos;
            this.excludedMembers = excludedMembers;
            this.excludedTypes = excludedTypes;
            this.membersSerializesInfos = membersSerializesInfos;
            this.maxRecursion = maxRecursion;
            this.handleMaxRecursion = handleMaxRecursion;
        }

        public string Serialize(object obj, int nestingLevel)
        {
            if (obj == null)
                return $"null{Environment.NewLine}";

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine; // todo

            if (nestingLevel == maxRecursion)
            {
                if (handleMaxRecursion != null)
                    return handleMaxRecursion(obj);

                return $"Maximum recursion has been reached{Environment.NewLine}";
            }
            
            var indentation = string.Intern(new string('\t', nestingLevel + 1));

            var type = obj.GetType();
            var sb = new StringBuilder().AppendLine(type.Name);

            HandleMembers(type, sb, indentation, obj, nestingLevel);

            return sb.ToString();
        }

        private void HandleMembers(Type type, StringBuilder sb, string indentation, object obj, int nestingLevel)
        {
            foreach (var propertyInfo in type.GetProperties(SerializingMembersFlag))
            {
                var data = new DataMember(propertyInfo);
                var serializedValue = HandleMember(data, obj, indentation, nestingLevel);
                if (serializedValue != null)
                    sb.Append(serializedValue);
            }

            foreach (var fieldInfo in type.GetFields(SerializingMembersFlag))
            {
                var data = new DataMember(fieldInfo);
                var serializedValue = HandleMember(data, obj, indentation, nestingLevel);
                if (serializedValue != null)
                    sb.Append(serializedValue);
            }
        }

        private string HandleMember(DataMember member, object obj, string indentation,
            int nestingLevel)
        {
            if (excludedMembers.Any(memberInfo => memberInfo.Name == member.Name) ||
                excludedTypes.Contains(member.Type))
                return null;

            if (membersSerializesInfos.TryGetValue(member.MemberInfo, out var memberSerialization))
                return GetSerializedString(obj, member, indentation, memberSerialization.SerializationFunc);

            if (typeSerializesInfos.TryGetValue(member.Type, out var typeSerialization))
                return GetSerializedString(obj, member, indentation, typeSerialization.SerializationFunc);

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
    }
}