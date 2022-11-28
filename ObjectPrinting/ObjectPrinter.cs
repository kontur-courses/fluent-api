namespace ObjectPrinting
{
    public class ObjectPrinter
    {
        /// <summary>
        /// Printer objects of type T
        /// </summary>
        /// <typeparam name="T">Generic parameter for next print fields typeof(T)</typeparam>
        /// <returns>PrintingConfig&lt;T&gt; </returns>
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }
    }
}