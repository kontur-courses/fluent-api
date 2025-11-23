using System;
using System.Collections.Generic;
using ObjectPrinting.Interface;

namespace ObjectPrinting;

internal class MemberPrinter
{
    public string PrintMember(
        string memberName,
        object value,
        Type memberType,
        IPrintingConfigInternal config,
        int level,
        HashSet<object> visited,
        ObjectSerializer serializer)
    {
        if (value == null)
            return memberName + " = null" + Environment.NewLine;

        if (config.PropertySerializers.TryGetValue(memberName, out var serializerFunc))
            return memberName + " = " + serializerFunc(value) + Environment.NewLine;

        if (config.TypeSerializers.TryGetValue(memberType, out var typeSerializer))
            return memberName + " = " + typeSerializer(value) + Environment.NewLine;

        if (value is string s)
        {
            int? maxLen = null;

            if (config.TrimLengths.TryGetValue(memberName, out var propTrim))
                maxLen = propTrim;
            else if (config.GlobalStringTrimLength.HasValue)
                maxLen = config.GlobalStringTrimLength.Value;

            if (maxLen.HasValue && s.Length > maxLen.Value)
                s = s.Substring(0, maxLen.Value);

            return memberName + " = " + s + Environment.NewLine;
        }

        if (TypeHelper.IsSimpleType(memberType))
            return memberName + " = " + value + Environment.NewLine;

        return memberName + " = " + serializer.PrintInternal(value, level, config, visited);
    }
}