using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    internal class ObjectSerializer
    {
        private readonly Dictionary<MemberInfo, Func<object, string>> specializedSerializers =
            new Dictionary<MemberInfo, Func<object, string>>();
        
        public void Specialize(MemberInfo memberInfo, Func<object, string> serializeFunction)
        {
            specializedSerializers.Add(memberInfo, serializeFunction);
        }

        public bool TryGetSerializationFunction(MemberInfo memberInfo, out Func<object, string> serializer)
        {
            return specializedSerializers.TryGetValue(memberInfo, out serializer) ||
                   specializedSerializers.TryGetValue(memberInfo.GetReturnType(), out serializer);
        }
    }
}