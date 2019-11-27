using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;

namespace ObjectPrinting
{
    class ConfigForObject<TOwner> : PrintingConfig<TOwner>, IConfigForObject
    {
        public object UsedObject { get; }

        public ConfigForObject(TOwner obj)
        {
            UsedObject = obj;
        }

        public string PrintToString()
        {
            return PrintToString((TOwner) UsedObject);
        }
    }

    interface IConfigForObject
    {
        object UsedObject { get; }
        string PrintToString();
    }
}
