using System;

namespace ObjectPrinting
{
    public static class FuncСonverter
    {
        public static Func<object, string> CastToObjectPrint<TPropType>(Func<TPropType,string> print)
        {
            return o => print((TPropType) o);
        }
    }
}