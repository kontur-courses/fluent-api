using System;
using System.Reflection;

namespace ObjectPrinting.Configs;

public class StringMemberPrintingConfig<TOwner> : MemberPrintingConfig<TOwner, string>
{
    private readonly MemberInfo memberInfo;
    private readonly PrintingConfig<TOwner> parent;

    public StringMemberPrintingConfig(PrintingConfig<TOwner> parent, MemberInfo memberInfo, PrintingContext context)
        : base(parent, memberInfo, context)
    {
        this.parent = parent;
        this.memberInfo = memberInfo;
    }

    public PrintingConfig<TOwner> Trim(int maxLength)
    {
        var type = memberInfo switch
        {
            PropertyInfo p => p.PropertyType,
            FieldInfo f => f.FieldType,
            _ => throw new ArgumentException("Member must be a property or field")
        };

        string ApplyTrim(string s) => s.Length <= maxLength ? s : s[..maxLength];

        if (context.memberSerializers.TryGetValue(memberInfo, out var prevSerializer))
            context.memberSerializers[memberInfo] = obj =>
            {
                var intermediateResult = prevSerializer(obj);
                return ApplyTrim(intermediateResult);
            };
        else if (context.typeSerializers.TryGetValue(type, out var prevTypeSerializer))
            context.memberSerializers[memberInfo] = obj =>
            {
                var intermediateResult = prevTypeSerializer(obj);
                return ApplyTrim(intermediateResult);
            };
        else
            return Using(ApplyTrim);

        return parent;
    }

}