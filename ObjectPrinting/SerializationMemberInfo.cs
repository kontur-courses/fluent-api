using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting
{
    public class SerializationMemberInfo
    {
        public string MemberName { get;  }
        public Type MemberType { get;  }
        public object MemberValue { get; }
        public SerializationMemberInfo(string memberName, Type memberType, object memberValue)
        {
            MemberName = memberName;
            MemberType = memberType;
            MemberValue = memberValue;
        }
    }
}
