using System;

namespace ObjectPrinting
{
    public class PrintInfo
    {
        public readonly string Definition;
        public readonly object Item;
        public readonly Type Type;
        public readonly string Name;

        public PrintInfo(object item, Type type, string name = "", string definition = "")
        {
            Item = item;
            Type = type;
            Definition = definition;
            Name = name;
        }
    }
}