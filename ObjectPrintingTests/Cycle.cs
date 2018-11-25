namespace ObjectPrintingTests
{
    class Cycle
    {
        public Cycle Value { get; set; }

        public Cycle()
        {
            Value = this;
        }
    }
}
