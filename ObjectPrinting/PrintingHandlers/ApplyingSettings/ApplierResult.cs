namespace ObjectPrinting.PrintingHandlers.ApplyingSettings;

public readonly struct ApplierResult
{
    public bool Applied { get; }
    public bool Exclude { get; }
    public string? Serialized { get; }
    public object? NewValue { get; }

    private ApplierResult(bool applied, bool exclude, string? serialized, object? newValue)
    {
        Applied = applied;
        Exclude = exclude;
        Serialized = serialized;
        NewValue = newValue;
    }

    public static ApplierResult NotApplied => new(false, false, null, null);

    public static ApplierResult Excluded() => new(true, true, null, null);

    public static ApplierResult SerializedValue(string value) =>
        new(true, false, value, null);

    public static ApplierResult Modified(object? newValue) =>
        new(true, false, null, newValue);
}