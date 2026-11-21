using System;
using System.Reflection;

namespace ObjectPrinting;

internal interface IPrintingSettings
{
    bool IsExcluded(MemberInfo member, Type memberType);

    bool TryGetTypePrinter(Type type, out Func<object, string> printer);

    bool TryGetMemberPrinter(MemberInfo member, out Func<object, string> printer);
}