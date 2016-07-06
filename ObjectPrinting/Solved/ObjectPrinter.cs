using System;
using System.Linq;
using System.Text;

namespace ObjectPrinting
{
	public class ObjectPrinter
	{
		public static PrintingConfig<T> For<T>()
		{
			return new PrintingConfig<T>();
		}
	}
}