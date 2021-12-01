namespace ObjectPrintingTests
{
    public class Robot
    {
        public readonly Robot It;
        public string Name;

        public Robot(string name)
        {
            It = this;
            Name = name;
        }
    }
}