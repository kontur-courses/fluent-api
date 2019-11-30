namespace ObjectPrinting
{
    public abstract class PrintingObject<T>
    {
        protected readonly object ObjectForPrint;
        protected readonly IPrintingConfig<T> PrintingConfig;

        protected PrintingObject(object obj, IPrintingConfig<T> config)
        {
            ObjectForPrint = obj;
            PrintingConfig = config;
        }
        
        public abstract string Print(int nestingLevel);
    }
}