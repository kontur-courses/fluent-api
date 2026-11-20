using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting;

internal class PrintableMember
{
    private readonly MemberInfo _member;

    private PrintableMember(MemberInfo member, Type valueType)
    {
        _member = member;
        ValueType = valueType;
    }

    internal string Name => _member.Name;
    internal Type ValueType { get; }
    internal MemberInfo Key => GetKey(_member);

    internal object? GetValue(object owner)
    {
        return _member switch
        {
            PropertyInfo property => property.GetValue(owner),
            FieldInfo field => field.GetValue(owner),
            _ => throw new InvalidOperationException()
        };
    }

    internal static IEnumerable<PrintableMember> GetAll(Type type)
    {
        var flags = BindingFlags.Public | BindingFlags.Instance;

        foreach (var member in type.GetMembers(flags))
        {
            switch (member)
            {
                case PropertyInfo property
                    when property.GetMethod?.IsPublic == true &&
                         property.GetIndexParameters().Length == 0:
                    yield return new PrintableMember(
                        property, property.PropertyType);
                    break;

                case FieldInfo field:
                    yield return new PrintableMember(field, field.FieldType);
                    break;
            }
        }
    }

    internal static MemberInfo GetKey(MemberInfo member)
    {
        var declaringType = member.DeclaringType!;

        return member.ReflectedType == declaringType
            ? member
            : declaringType.GetMemberWithSameMetadataDefinitionAs(member);
    }
}