using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, Expression<Func<object, string>>> SerializingMethods { get; }
        Dictionary<PropertyInfo, Expression<Func<object, string>>> PropertySerializingMethods { get; }
    }

}
