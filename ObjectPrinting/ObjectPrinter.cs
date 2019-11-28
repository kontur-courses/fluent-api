using System;

namespace ObjectPrinting
{
    public static class ObjectPrinter
    {
        internal static readonly Type[] FinalTypes =
        {
            typeof(int),
            typeof(double),
            typeof(float), 
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid),
        };

        internal static readonly int MaxDepthSerialize = 15;
        internal static readonly string MaxDepthSerializeString = "..." + Environment.NewLine;
        
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }
    }
}