using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.TypesSerializers
{
    public class MembersSerializer : TypeSerializer
    {
        private readonly ImmutableHashSet<Type> excludedTypes;
        private readonly IReadOnlyDictionary<Type, Delegate> typesSerializers;
        private readonly IReadOnlyDictionary<Type, CultureInfo> customCultures;
        private readonly IReadOnlyDictionary<MemberInfo, Delegate> membersSerializers;
        private readonly IReadOnlyDictionary<MemberInfo, int> stringsTrimValues;
        private readonly ImmutableHashSet<MemberInfo> excludedSpecificMembers;
        private readonly Lazy<TypeSerializer> typeSerializer;

        public MembersSerializer(
            ImmutableHashSet<Type> excludedTypes,
            IReadOnlyDictionary<Type, Delegate> typesSerializers,
            IReadOnlyDictionary<Type, CultureInfo> customCultures,
            IReadOnlyDictionary<MemberInfo, Delegate> membersSerializers,
            IReadOnlyDictionary<MemberInfo, int> stringsTrimValues,
            ImmutableHashSet<MemberInfo> excludedSpecificMembers,
            TypeSerializer typeSerializer)
        {
            this.excludedTypes = excludedTypes;
            this.typesSerializers = typesSerializers;
            this.customCultures = customCultures;
            this.membersSerializers = membersSerializers;
            this.stringsTrimValues = stringsTrimValues;
            this.excludedSpecificMembers = excludedSpecificMembers;
            this.typeSerializer = new Lazy<TypeSerializer>(() => typeSerializer);
        }

        public override string Serialize(
            object obj,
            int nestingLevel,
            ImmutableHashSet<object> excludedValues)
        {
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            var identation = new string('\t', nestingLevel + 1);

            foreach (var memberInfo in type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(member => member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field)
                .OrderBy(member => member.Name))
            {
                var typeValue = GetTypeValue(memberInfo, obj);
                var memberType = typeValue.Item1;
                var value = typeValue.Item2;

                if (excludedTypes.Contains(memberType))
                {
                    continue;
                }

                if (excludedSpecificMembers.Contains(memberInfo))
                {
                    continue;
                }

                if (excludedValues.Contains(value))
                {
                    sb.AppendLine(identation + memberInfo.Name + " = " + Constants.Circular);

                    continue;
                }

                if (typesSerializers.TryGetValue(memberType, out var serializer))
                {
                    value = serializer.DynamicInvoke(value);
                }

                if (customCultures.TryGetValue(memberType, out var culture))
                {
                    value = ((IConvertible)value).ToString(culture);
                }

                if (membersSerializers.TryGetValue(memberInfo, out var propSerializer))
                {
                    value = propSerializer.DynamicInvoke(value);
                }

                if (stringsTrimValues.TryGetValue(memberInfo, out var count))
                {
                    var memberValueAsString = value.ToString();

                    if (count < 0)
                    {
                        throw new ArgumentException("{0} can't be less than zero.", nameof(count));
                    }

                    if (count > memberValueAsString.Length)
                    {
                        count = memberValueAsString.Length;
                    }

                    value = memberValueAsString
                        .Substring(0, count);
                }

                sb.Append(identation + memberInfo.Name + " = " +
                    typeSerializer.Value.Serialize(value,
                        nestingLevel + 1, excludedValues.Add(value)
                            .Add(obj)));
            }

            return sb
                .Append(Successor?.Serialize(obj, nestingLevel, excludedValues))
                .ToString();
        }

        private static (Type, object) GetTypeValue(MemberInfo memberInfo, object obj)
        {
            switch (memberInfo)
            {
                case FieldInfo fieldInfo:

                    return (fieldInfo.FieldType, fieldInfo.GetValue(obj));
                case PropertyInfo propertyInfo:

                    return (propertyInfo.PropertyType, propertyInfo.GetValue(obj));
                default:

                    throw new InvalidOperationException("Can't get value from given MemberInfo");
            }
        }
    }
}