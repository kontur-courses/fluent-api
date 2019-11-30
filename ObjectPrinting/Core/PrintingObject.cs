namespace ObjectPrinting
{
    public abstract class PrintingObject<T>
    {
        protected readonly object ObjectForPrint;
        protected readonly PrintingConfig<T> PrintingConfig;

        protected PrintingObject(object obj, PrintingConfig<T> config)
        {
            ObjectForPrint = obj;
            PrintingConfig = config;
        }
        public abstract string Print(int nestingLevel);
    }
}