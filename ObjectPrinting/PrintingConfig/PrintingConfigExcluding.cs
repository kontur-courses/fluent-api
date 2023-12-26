using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public partial class PrintingConfig<TOwner>
    {
        public PrintingConfig<TOwner> Excluding(Expression<Func<TOwner, object>> selector)
        {
            var member = ReflectionHelpers.GetMemberFromExpression(selector);

            configuration.ToExclude.Add(info => info.Name == member.Name);

            return this;
        }
        public PrintingConfig<TOwner> Excluding<TType>()
        {
            configuration.ToExclude.Add(info => info.PropertyType == typeof(TType));

            return this;
        }

        private PrintingConfig<TOwner> Excluding(Predicate predicate)
        {
            configuration.ToExclude.Add(predicate);

            return this;
        }
    }
}
