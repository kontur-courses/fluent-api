using System.Reflection;

namespace ObjectPrinting.Core.Configs.Basics
{
    public class BasicMemberConfig<TProperty, TOwner>
    {        
        protected readonly MemberInfo Member;
        internal readonly PrintingConfig<TOwner> Container;

        internal BasicMemberConfig(PrintingConfig<TOwner> container, MemberInfo member)
        {            
            Member = member;
            Container = container;
        }

        public void Exclude()
        {
            Container.Exclude(Member);
        }

        public BasicMemberConfig<TProperty, TOwner> WithSerializer(Func<TProperty, string> serializer)
        {
            Container.WithSerializer(Member, serializer);
            return this;
        }
    }
}
