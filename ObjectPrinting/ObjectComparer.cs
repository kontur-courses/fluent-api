using System.Collections;
using System.Collections.Generic;

namespace ObjectPrinting
{
	public class ObjectReferenceComparer: IEqualityComparer<object>
	{
		public new bool Equals(object x, object y) => ReferenceEquals(x, y);

		public int GetHashCode(object obj) => obj.GetHashCode();
	}
}