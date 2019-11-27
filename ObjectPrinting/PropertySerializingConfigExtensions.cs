using System;
using System.Globalization;
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

            Func<string, string> oldMethod = (s => s?.ToString());
            if (propertyInfo is null)
            {
                if (parent.SerializingMethods.ContainsKey(typeof(string)))
                    oldMethod = parent.SerializingMethods[typeof(string)].Compile();

                parent.SerializingMethods[typeof(string)] = s => new string(oldMethod.Invoke((string) s).Take(length).ToArray());
                
                return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
            }

            if (parent.PropertySerializingMethods.ContainsKey(propertyInfo))
                oldMethod = parent.PropertySerializingMethods[propertyInfo].Compile();

            parent.PropertySerializingMethods[propertyInfo] = s => new string(oldMethod.Invoke((string) s).Take(length).ToArray());

            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }
    }
}