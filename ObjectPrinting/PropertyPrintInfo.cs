using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    class PropertyPrintInfo
    {
        public readonly string Definition;
        public readonly object Item;
        public readonly Type ItemType;
        public readonly string Name;

        public PropertyPrintInfo(object item, Type itemType, string name = "", string definition = "")
        {
            Item = item;
            ItemType = itemType;
            Definition = definition;
            Name = name;
        }
    }
}
