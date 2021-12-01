using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly SerializerSettings settings = new();

        public string PrintToString(TOwner obj) => new Serializer(settings).PrintToString(obj);
        
        public PrintingConfig<TOwner> AllowCyclingReference()
        {
            settings.IsAllowCyclingReference = true;
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
            if (memberSelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException("Cannot resolve member expression");
            var memberInfo = memberExpression.Member;
            if (memberInfo is null)
                throw new ArgumentException("Cannot resolve member type");
            return memberInfo;
        }
    }
}