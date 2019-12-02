using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public static class PropertySerializingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmingToLength<TOwner>(this PropertySerializingConfig<TOwner, string> config,
            int length)
        {
            var propertyInfo = (config as IPropertySerializingConfig<TOwner>).UsedPropertyInfo;
            var parent = (config as IPropertySerializingConfig<TOwner>).ParentConfig as IPrintingConfig<TOwner>;

            if (propertyInfo is null )
                UpdateMethod(parent.SerializingMethods, typeof(string), s => new string(s.Take(length).ToArray()));
            else
                UpdateMethod(parent.PropertySerializingMethods, propertyInfo, s => new string(s.Take(length).ToArray()));

            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        private static void UpdateMethod<TKey>(IDictionary<TKey, Expression<Func<object, string>>> methods, TKey key, Func<string, string> method)
        {
            Func<object, string> oldMethod = (s => s?.ToString());
            if (methods.ContainsKey(key))
                oldMethod = s => methods[key].Compile().Invoke(s);

            methods[key] = s => method(oldMethod.Invoke(s));
        }
    }
}