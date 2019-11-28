namespace ObjectPrinting.Tests
{
    public class SelfReflect
    {
        public SelfReflect reflect { get; }

        public SelfReflect()
        {
            reflect = this;
        }
    }
}