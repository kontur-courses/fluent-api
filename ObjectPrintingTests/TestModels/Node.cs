namespace ObjectPrintingTests.TestModels
{
    public class Node
    {
        private static int _id;
        public int Id { get; set; }
        public Node PreviousNode { get; set; }
        public Node NextNode { get; set; }

        public Node()
        {
            Id = _id++;
        }
    }
}