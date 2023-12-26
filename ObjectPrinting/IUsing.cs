using System;

namespace ObjectPrinting
{
    public interface IUsing<TOwner, TSerialization>
    {
        public IWrap<TOwner> Using(Func<TSerialization, string> serialize);
    }
}