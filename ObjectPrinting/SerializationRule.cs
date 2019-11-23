using System;
using System.Reflection;

namespace ObjectPrinting
{
    public struct SerializationRule
    {
        public readonly Func<object, PropertyInfo, bool> filterHandler;
        public readonly Func<object, PropertyInfo, string, int, string> resultHandler;

        public SerializationRule( Func<object, PropertyInfo, bool> filterHandler,
            Func<object, PropertyInfo, string, int, string> resultHandler)
        {
            this.filterHandler = filterHandler;
            this.resultHandler = resultHandler;
        }
    }
}