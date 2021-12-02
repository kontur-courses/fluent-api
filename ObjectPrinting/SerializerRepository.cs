using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class SerializerRepository
    {
        public readonly Dictionary<Type, Delegate> TypeSerializers;
        public readonly Dictionary<MemberInfo, Delegate> MemberSerializers;

        public SerializerRepository()
        {
            TypeSerializers = new Dictionary<Type, Delegate>();
            MemberSerializers = new Dictionary<MemberInfo, Delegate>();
        }
    }
}