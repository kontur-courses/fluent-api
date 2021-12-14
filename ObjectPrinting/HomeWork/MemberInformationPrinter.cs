using System;
using System.Reflection;
using System.Text;
using static ObjectPrinting.HomeWork.SerializationDelegateMaker;

namespace ObjectPrinting.HomeWork
{
    public static class MemberInformationPrinter
    {
        public static void PrintMemberInformation<TOwner>(PrintingConfig<TOwner> config, MemberInfo memberInfo, object obj, StringBuilder sb, int nestingLevel, string identation)
        {
            SerializationMemberInfo memberSerialization = default;
            if (obj is Delegate)
            {
                config.SerializeDelegate(obj, sb, identation, config.Items, nestingLevel);
                return;
            }
            if (memberInfo is PropertyInfo propertyInfo)
            {
                var indexParameters = propertyInfo.GetIndexParameters();
                if (indexParameters.Length != 0)
                {
                    config.SerializeComplexTypes(obj, sb, identation, config.Items, nestingLevel);
                    return;
                }
                memberSerialization =
                    new SerializationMemberInfo(propertyInfo, obj);
            }
            else if (memberInfo is FieldInfo fieldInfo)
            {
                memberSerialization =
                    new SerializationMemberInfo(fieldInfo, obj);
            }

            if (memberSerialization == null || config.ExcludedTypes.Contains(memberSerialization.MemberType) 
                                            || config.ExcludedFieldsProperties.Contains(memberInfo))
                return;

            var serializeDelegate = MakeSerializeDelegate(memberInfo,
                config.SpecialSerializationsForTypes, config.SpecialSerializationsForFieldsProperties);
            config.SerializedMembers.Add(obj);

            string specialSerialize;

            if (serializeDelegate != null)
                specialSerialize = (config.FormSerializeDelegateString(identation, nestingLevel, memberSerialization,
                    serializeDelegate));
            else
                specialSerialize = (config.FormSerializeString(identation, nestingLevel, memberSerialization));


            if (config.TrimMembers.TryGetValue(memberInfo, out var memberBorders))
                specialSerialize = TrimMaker.MakeTrim(identation, memberBorders, memberSerialization);
            if (config.TrimTypes.TryGetValue(memberSerialization.MemberType, out var typeBorders))
                specialSerialize = TrimMaker.MakeTrim(identation, typeBorders, memberSerialization);


            sb.Append(specialSerialize);
        }
    }
}
