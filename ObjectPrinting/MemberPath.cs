namespace ObjectPrinting;

public readonly record struct MemberPath
{
    public static readonly MemberPath Empty = new(string.Empty);

    public string Value { get; }

    public bool IsEmpty => string.IsNullOrEmpty(Value);

    public MemberPath(string? value = null)
    {
        Value = value ?? string.Empty;
    }

    public MemberPath Append(string memberName)
    {
        if (IsEmpty)
            return new(memberName);
        return new($"{Value}.{memberName}");
    }

    public override string ToString() => Value;

    public static implicit operator MemberPath(string path) => new(path);
}
