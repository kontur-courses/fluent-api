using System;
using System.Reflection;

namespace ObjectPrinting.Common
{
    public interface IPrintingPropertyBaseConfig<TOwner, T> : IHaveRoot
    {
        internal PropertyInfo CurrentProperty { get; }

        public IPrintingPropertyConfig<TOwner, T> SetSerializer(Func<T, string> serializer)
        {
            var objSerializer = new Func<object, string>(obj => serializer((T)obj));

            if (!Root.PropertySerializers.ContainsKey(CurrentProperty))
                Root.PropertySerializers.Add(CurrentProperty, objSerializer);
            else Root.PropertySerializers[CurrentProperty] = objSerializer;

            return new PrintingPropertyConfig<TOwner, T>(CurrentProperty, Root);
        }
    }
}