using System;

namespace ObjectPrinting.Solved
{
	public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static string PrintToString<T>(this T obj)
		{
			return ObjectPrinter.For<T>().PrintToString(obj);
		}
	}
}