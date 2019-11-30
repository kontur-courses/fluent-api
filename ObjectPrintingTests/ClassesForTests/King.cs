namespace ObjectPrintingTests.ClassesForTests
{
    public class King
    {
        public King Parent { get; set; }

        public King(King parent)
        {
            Parent = parent;
        }
    }
}