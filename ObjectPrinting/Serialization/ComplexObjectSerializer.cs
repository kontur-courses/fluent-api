using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Serialization;

internal class ComplexObjectSerializer : ISerializationStrategy
{
    public bool CanSerialize(object obj) => true;

    public string Serialize(object obj, int nestingLevel, ObjectSerializer serializer)
    {
        var type = obj.GetType();
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);

        foreach (var (memberInfo, memberType, value) in GetMembers(obj, type))
        {
            if (serializer.IsExcluded(memberInfo, memberType))
                continue;

            sb.Append(indentation)
                .Append(memberInfo.Name)
                .Append(" = ")
                .Append(serializer.Serialize(value, nestingLevel + 1, memberInfo));
        }

        return sb.ToString();
    }

    private static IEnumerable<(MemberInfo Member, Type MemberType, object? Value)> GetMembers(object obj, Type type)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

        foreach (var property in type.GetProperties(flags))
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
                continue;

            yield return (property, property.PropertyType, property.GetValue(obj));
        }

        foreach (var field in type.GetFields(flags))
        {
            yield return (field, field.FieldType, field.GetValue(obj));
        }
    }
}
