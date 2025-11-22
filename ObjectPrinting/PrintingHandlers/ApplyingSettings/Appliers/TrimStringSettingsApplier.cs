using System;
using ObjectPrinting.PrintingHandlers.ApplyingSettings.Interfaces;

namespace ObjectPrinting.PrintingHandlers.ApplyingSettings.Appliers;

internal class TrimStringSettingsApplier : ISettingsApplier
{
    public ApplierResult Apply(ValueContext context)
    {
        if (context.Type != typeof(string) || context.Value is not string s)
            return ApplierResult.NotApplied;

        var settings = context.Settings;

        if (settings.StringTrimLengths.TryGetValue(context.Path, out var propertyMax))
        {
            return ApplierResult.Modified(s.Length <= propertyMax ? s : s.AsSpan(0, propertyMax).ToString());
        }

        if (settings.GlobalStringTrimLength <= 0) return ApplierResult.NotApplied;

        var max = settings.GlobalStringTrimLength;
        return ApplierResult.Modified(s.Length <= max ? s : s.AsSpan(0, max).ToString());
    }
}