namespace ObjectPrinting.HomeWork.Tests;

public class TestSpec(
    string description,
    Func<string> execute,
    string[]? shouldContain = null,
    string[]? shouldNotContain = null)
{
    public string Description { get; } = description;
    public Func<string> Execute { get; } = execute;
    public string[] ShouldContain { get; } = shouldContain ?? [];
    public string[] ShouldNotContain { get; } = shouldNotContain ?? [];

    public override string ToString() => Description;
}