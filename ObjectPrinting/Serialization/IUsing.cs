using System;

namespace ObjectPrinting.Serialization
{
    public interface IUsing<TOwner, TSerialization>
    {
        public IWrap<TOwner> Using(Func<TSerialization, string> serialize);
    }
}