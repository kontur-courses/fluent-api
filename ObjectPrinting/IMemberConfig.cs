using System.Reflection;

namespace ObjectPrinting
{
    internal interface IMemberConfig
    {
        MemberInfo GetMember { get; }
    }
}
