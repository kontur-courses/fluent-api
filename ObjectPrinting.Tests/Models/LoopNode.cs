namespace ObjectPrinting.Tests.Models;

public class LoopNode
{
    public int Value { get; set; }
    public LoopNode Next { get; set; }
    public LoopNode Previous { get; set; }
}