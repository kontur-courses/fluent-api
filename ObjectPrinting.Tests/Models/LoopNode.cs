namespace ObjectPrinting.Tests.Models;

public class LoopNode
{
    public int Value { get; set; }
    public LoopNode Next { get; set; }
    public LoopNode Previous { get; set; }
    public override bool Equals(object? obj) => obj is LoopNode other && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
}