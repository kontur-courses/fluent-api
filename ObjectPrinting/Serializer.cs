using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ObjectPrinting.Solved;

namespace ObjectPrinting
{
    public class Serializer
    {
        private const BindingFlags SerializingMembersFlag = BindingFlags.Public | BindingFlags.Instance;
        private readonly HashSet<object> complexObjectLinks = new HashSet<object>();
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

            var indentation = string.Intern(new string('\t', nestingLevel + 1));

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine; 

            if (complexObjectLinks.Contains(obj))
            {
                if (handleMaxRecursion != null)
                    return handleMaxRecursion(obj);

                return $"Maximum recursion has been reached{Environment.NewLine}";
            }

            complexObjectLinks.Add(obj);

            if (obj is ICollection collection)
                return SerializeCollection(collection, nestingLevel);

            var type = obj.GetType();
            var sb = new StringBuilder().AppendLine(type.Name);

            HandleMembers(type, sb, indentation, obj, nestingLevel);

            complexObjectLinks.Remove(obj);
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

            if (typeof(ICollection).IsAssignableFrom(member.Type))
                return GetSerializedString(
                    obj,
                    member,
                    indentation,
                    obj => SerializeCollection((ICollection) obj, nestingLevel + 1),
                    false);

            return GetSerializedString(
                obj,
                member,
                indentation,
                value => Serialize(
                    value,
                    nestingLevel + 1),
                false);
        }

        private string SerializeCollection(ICollection collection, int nestingLevel)
        {
            if (collection == null)
                return $"null{Environment.NewLine}";

            var sb = new StringBuilder();
            sb.AppendLine(collection.GetType().Name);

            var indentation = GetIndentation(nestingLevel + 1);
            foreach (var element in collection)
            {
                sb.Append(indentation);
                sb.Append(Serialize(element, nestingLevel + 1));
            }

            return sb.ToString();
        }

        private string GetIndentation(int nestingLevel)
        {
           return string.Intern(new string('\t', nestingLevel));
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