using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.HomeWork
{
    public static class SerializationDelegateMaker
    {
        public static Delegate MakeSerializeDelegate(MemberInfo memberSerialization, 
            Dictionary<Type, Delegate> specialSerializationsForTypes,
            Dictionary<MemberInfo, Delegate> specialSerializationsForFieldsProperties)
        {
            if (specialSerializationsForFieldsProperties.ContainsKey(memberSerialization))
                return specialSerializationsForFieldsProperties[memberSerialization];

            if (memberSerialization is FieldInfo fieldSerialization &&
                specialSerializationsForTypes.ContainsKey(fieldSerialization.FieldType))
            {
                return specialSerializationsForTypes[fieldSerialization.FieldType];
            }

            if (memberSerialization is PropertyInfo propertySerialization &&
                specialSerializationsForTypes.ContainsKey(propertySerialization.PropertyType))
            {
                return specialSerializationsForTypes[propertySerialization.PropertyType];
            }

            return null;
        }
    }
}
