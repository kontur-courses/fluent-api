using System;
using System.Reflection;

namespace ObjectPrinting.PrintingMembers
{
    public class PrintingMember
    {
        public Type Type { get; }
        public string Name { get; }
        public MemberInfo MemberInfo { get; }
        public Func<object, object> GetValue { get; }

        public PrintingMember(Type type, string name, MemberInfo memberInfo, Func<object, object> getValue)
        {
            Type = type;
            Name = name;
            MemberInfo = memberInfo;
            GetValue = getValue;
        }
    }
}