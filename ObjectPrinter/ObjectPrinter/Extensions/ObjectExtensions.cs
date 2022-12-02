using ObjectPrinter.PrintingConfigs;

namespace ObjectPrinter.Extensions;

public static class ObjectExtensions
{
	public static string PrintToString<T>(this T obj)
	{
		return ObjectPrinter<T>.Configuration().Build().PrintToString(obj);
	}

	public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
	{
		return new ObjectPrinter<T>(config(ObjectPrinter<T>.Configuration())).PrintToString(obj);
	}
}