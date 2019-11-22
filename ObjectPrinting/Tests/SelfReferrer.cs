namespace ObjectPrinting.Tests
{
    public class SelfReferrer
    {
        public int X => 42;
        public SelfReferrer Self => this;
    }
}