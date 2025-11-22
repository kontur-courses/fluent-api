using ObjectPrinting.PrintingHandlers.ApplyingSettings.Interfaces;

namespace ObjectPrinting.PrintingHandlers.ApplyingSettings.Appliers;

internal class SerializerSettingsApplier : ISettingsApplier
{
    public ApplierResult Apply(ValueContext context)
    {
        if (context.Settings.PropertySerializers.TryGetValue(context.Path, out var propertySerializer))
            return ApplierResult.SerializedValue(propertySerializer(context.Value));
        
        if (context.Type != null &&
            context.Settings.TypeSerializers.TryGetValue(context.Type, out var typeSerializer))
            return ApplierResult.SerializedValue(typeSerializer(context.Value));

        return ApplierResult.NotApplied;
    }
}