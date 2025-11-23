
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace ObjectPrinting;

public class ReferenceEqualityComparer : IEqualityComparer<Object>
{
    public new bool Equals(object x, object y) => ReferenceEquals(x, y);
    public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
}