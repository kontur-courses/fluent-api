using System;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting
{
    public class Member
    {
        public string Name;
        public Type Type;
        public object Value;

        private bool CheckProperties(Member currentMember, bool isSameTypes)
        {
            if (Name != currentMember.Name || Type != currentMember.Type) 
                return false;
            
            foreach (var memberInfo in Type.GetProperties().Cast<MemberInfo>().Concat(Type.GetFields()))
            {
                var member = new Member()
                {
                    Name = memberInfo.Name, Type = (Type)memberInfo.GetMemberType(), Value = memberInfo.GetMemberValue(Value)
                };

                if ((Type)memberInfo.GetMemberType() == Type)
                {
                    if (!isSameTypes)
                        return member.CheckProperties(this, true);

                    continue;
                }

                var firstValue = memberInfo.GetMemberValue(Value);
                var secondValue = memberInfo.GetMemberValue(currentMember.Value);
                    
                if (firstValue == secondValue)
                    continue;

                if (!firstValue.Equals(secondValue))
                    return false;
            }
                
            return true;

        }

        public override bool Equals(object obj)
        {
            if (!(obj is Member currentMember)) 
                return false;

            return CheckProperties(currentMember, false);
        }
    }
}