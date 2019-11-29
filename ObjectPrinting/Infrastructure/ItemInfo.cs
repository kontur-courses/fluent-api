using System;

namespace ObjectPrinting.Infrastructure
{
    public class ItemInfo
    {
        public object Item { get; }
        public Type Type { get; }
        public string Name { get; }

        public ItemInfo(object item, Type type, string name = null)
        {
            Item = item;
            Type = type;
            Name = name;
        }
    }
}