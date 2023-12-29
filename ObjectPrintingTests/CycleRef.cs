#nullable enable
namespace ObjectPrinting.Tests
{
    public class CycleRef
    {
        public int Id { get; set; }
        public CycleRef? Child { get; set; }
    }
}