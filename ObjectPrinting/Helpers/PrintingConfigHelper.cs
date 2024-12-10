using System;
using ObjectPrinting.Constants;

namespace ObjectPrinting.Helpers;

internal static class PrintingConfigHelper
{
    public static bool IsCoreType(Type type)
    {
        return type.IsValueType || PrintingConfigConstants.CoreTypes.Contains(type);
    }
}