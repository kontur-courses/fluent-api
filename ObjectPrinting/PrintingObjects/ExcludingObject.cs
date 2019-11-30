namespace ObjectPrinting
{
    public class ExcludingObject<T> : PrintingObject<T>
    {
        public ExcludingObject(object obj, PrintingConfig<T> config) : base(obj, config)
        {
        }

        public override string Print(int nestingLevel)
        {
            return "";
        }
    }
}