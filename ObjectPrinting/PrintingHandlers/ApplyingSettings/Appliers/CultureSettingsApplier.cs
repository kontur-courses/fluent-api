using System;
using ObjectPrinting.PrintingHandlers.ApplyingSettings.Interfaces;

namespace ObjectPrinting.PrintingHandlers.ApplyingSettings.Appliers;

internal class CultureSettingsApplier : ISettingsApplier
{
    public ApplierResult Apply(ValueContext context)
    {
        if (context.Type == null ||
            !context.Settings.TypeCultures.TryGetValue(context.Type, out var culture))
            return ApplierResult.NotApplied;

        var str = ((IFormattable)context.Value).ToString(null, culture);
        return ApplierResult.SerializedValue(str);
    }
}