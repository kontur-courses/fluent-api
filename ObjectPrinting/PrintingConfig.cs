using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly SerializerSettings settings = new();

        public string PrintToString(TOwner obj) => new Serializer(settings).Serialize(obj);
        
        public PrintingConfig<TOwner> AllowCyclingReference()
        {
            settings.AllowCyclingReference = true;
            return this;
        }

        public PrintingConfig<TOwner> SetDepthOfSerialize(int depth)
        {
            if (depth <= 0)
                throw new ArgumentException($"depth must be > 0, not {depth}");
            settings.Depth = depth;
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TType>()
        {
            settings.AddExcludedType(typeof(TType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TType>(Expression<Func<TOwner, TType>> memberSelector)
        {
            var memberInfo = GetMember(memberSelector);
            settings.AddExcludedMember(memberInfo);
            return this;
        }
        
        public IInnerTypeConfig<TOwner, TType> Printing<TType>() => new TypeConfig<TOwner, TType>(this, settings);

        public IInnerTypeConfig<TOwner, TType> Printing<TType>(Expression<Func<TOwner, TType>> memberSelector)
        {
            var memberInfo = GetMember(memberSelector);
            return new PropertyPrintingConfig<TOwner, TType>(this, memberInfo, settings);
        }
        
        private MemberInfo GetMember<TType>(Expression<Func<TOwner, TType>> memberSelector)
        {
            if (memberSelector is null)
                throw new ArgumentException("Expression should be not null");
            if (memberSelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException($"Cannot resolve member expression {memberSelector.Body}");
            var memberInfo = memberExpression.Member;
            if (memberInfo is null)
                throw new ArgumentException("MemberInfo should be not null");
            if (memberInfo.MemberType is not (MemberTypes.Field or MemberTypes.Property))
                throw new ArgumentException($"Expression should be a field or property selector, not {memberInfo.MemberType}");
            return memberInfo;
        }
    }
}