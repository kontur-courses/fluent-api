using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class Serialization<TOwner>
    {
        public String EditingPropertyInfoName;
        public PrintingConfig<TOwner> PrintingConfig; 
        public PrintingConfig<TOwner> SetSerialization(Func<object, string> func)
        {
            PrintingConfig.propertySerialization.Add(EditingPropertyInfoName, func);
            return PrintingConfig;
        }
    }
}