using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public class SerializingConfigContext
    {
        public Dictionary<Type, Func<object, string>> TypeSerializers;
        public Dictionary<string, Func<object, string>> PropSerializers;
    }
}