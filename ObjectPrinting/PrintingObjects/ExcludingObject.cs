namespace ObjectPrinting
{
    public class ExcludingObject<T> : PrintingObject<T>
    {
        public ExcludingObject(object obj, IPrintingConfig<T> config) : base(obj, config) { }

        public override string Print(int nestingLevel) => "";
    }
}