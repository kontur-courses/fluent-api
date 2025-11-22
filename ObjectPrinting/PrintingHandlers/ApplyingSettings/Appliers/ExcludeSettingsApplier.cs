using ObjectPrinting.PrintingHandlers.ApplyingSettings.Interfaces;

namespace ObjectPrinting.PrintingHandlers.ApplyingSettings.Appliers;

internal class ExcludeSettingsApplier : ISettingsApplier
{
    public ApplierResult Apply(ValueContext context)
    {
        if (context.Settings.ExcludedProperties.Contains(context.Path) ||
            (context.Type != null && context.Settings.ExcludedTypes.Contains(context.Type)))
            return ApplierResult.Excluded();

        return ApplierResult.NotApplied;
    }
}