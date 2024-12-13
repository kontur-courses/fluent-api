using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class SerrializeConfig
    {
        /*
         * У объекта для серриализации может быть вложенное поле, имеющшее тип, который мы сами определили,
         * и для него тоже может быть возможность определить кастомный сериализатор.
         * Также изначально не известно какие собственные типы есть у объекта, поэтому нужен отдельный хеш-сет,
         * где хранятся все исключенные типы.
         */

        private readonly HashSet<Type> ExcludedTypes;
        private readonly HashSet<PropertyInfo> ExcludedProperties;
        private readonly HashSet<FieldInfo> ExcludedFields;

        private readonly Dictionary<Type, Delegate> TypeSerrializers;
        private readonly Dictionary<PropertyInfo, Delegate> PropertySerrializers;
        private readonly Dictionary<FieldInfo, Delegate> FieldSerrializers;


        public SerrializeConfig()
        {
            ExcludedTypes = new HashSet<Type>();

            ExcludedProperties = new HashSet<PropertyInfo>();

            ExcludedFields = new HashSet<FieldInfo>();

            TypeSerrializers = new Dictionary<Type, Delegate>
            {
                { typeof(int), DefaultSerrialize },
                { typeof(double), DefaultSerrialize },
                { typeof(float), DefaultSerrialize },
                { typeof(string), DefaultSerrialize },
                { typeof(DateTime), DefaultSerrialize },
                { typeof(TimeSpan), DefaultSerrialize },
                { typeof(Guid), DefaultSerrialize },
            };

            PropertySerrializers = new Dictionary<PropertyInfo, Delegate>();

            FieldSerrializers = new Dictionary<FieldInfo, Delegate>();
        }

        public SerrializeConfig(SerrializeConfig old)
        {
            ExcludedTypes = new HashSet<Type>(old.ExcludedTypes);

            ExcludedProperties = new HashSet<PropertyInfo>(old.ExcludedProperties);

            ExcludedFields = new HashSet<FieldInfo>(old.ExcludedFields);

            TypeSerrializers = new Dictionary<Type, Delegate>(old.TypeSerrializers);

            PropertySerrializers = new Dictionary<PropertyInfo, Delegate>(old.PropertySerrializers);

            FieldSerrializers = new Dictionary<FieldInfo, Delegate>(old.FieldSerrializers);
        }

        private string DefaultSerrialize(object obj) => obj.ToString();

        public bool IsExcludedMember(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ExcludedFields.Contains(member as FieldInfo);
                case MemberTypes.Property:
                    return ExcludedProperties.Contains(member as PropertyInfo);
                default:
                    return false;
            }
        }

        public bool IsExcludedType(Type type) => ExcludedTypes.Contains(type);

        public bool TryGetSerializer(MemberInfo member, out Delegate serializer)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return FieldSerrializers.TryGetValue(member as FieldInfo, out serializer);
                case MemberTypes.Property:
                    return PropertySerrializers.TryGetValue(member as PropertyInfo, out serializer);
                default:
                    serializer = null;
                    return false;
            }
        }

        public void AddMemberSerializer(MemberInfo member, Delegate serializer)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    FieldSerrializers[member as FieldInfo] = serializer;
                    break;
                case MemberTypes.Property:
                    PropertySerrializers[member as PropertyInfo] = serializer;
                    break;
                default:
                    TypeSerrializers[member.DeclaringType] = serializer;
                    break;
            }
        }

        public bool ExcludeMember(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ExcludedFields.Add(member as FieldInfo);
                case MemberTypes.Property:
                    return ExcludedProperties.Add(member as PropertyInfo);
                default:
                    return false;
            }
        }

        public bool ExcludeType(Type type) => ExcludedTypes.Add(type);

        public void AddTypeSerializer(Type type, Delegate serializer) => TypeSerrializers[type] = serializer;

                public bool TryGetSerializer(Type type, out Delegate serializer) => TypeSerrializers.TryGetValue(type, out serializer);
    }
}
