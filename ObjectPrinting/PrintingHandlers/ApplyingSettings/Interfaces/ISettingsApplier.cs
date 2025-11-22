namespace ObjectPrinting.PrintingHandlers.ApplyingSettings.Interfaces;

internal interface ISettingsApplier
{
    ApplierResult Apply(ValueContext ctx);
}