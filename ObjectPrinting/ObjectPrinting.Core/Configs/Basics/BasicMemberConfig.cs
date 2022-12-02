using System.Reflection;

namespace ObjectPrinting.Core.Configs.Basics
{
    public class BasicMemberConfig<TProperty, TOwner>
    {
        internal readonly PrintingConfig<TOwner> Container;
        protected readonly MemberInfo member;

        internal BasicMemberConfig(PrintingConfig<TOwner> container, MemberInfo member)
        {
            Container = container;
            this.member = member;
        }

        public void Exclude()
        {
            throw new NotImplementedException();
        }

        public BasicMemberConfig<TProperty, TOwner> WithSerializer(Func<TProperty, string> serializer)
        {
            throw new NotImplementedException();
        }
    }
}
