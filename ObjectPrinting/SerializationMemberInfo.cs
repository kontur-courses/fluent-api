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
        public SerializationMemberInfo(string memberName = null, Type memberType = null, object memberValue = null)
        {
            MemberName = memberName;
            MemberType = memberType;
            MemberValue = memberValue;
        }
    }
}
