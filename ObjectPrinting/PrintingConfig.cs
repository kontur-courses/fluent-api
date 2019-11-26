using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Config config = new Config();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, config, typeof(TPropType));
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression exp))
                throw new ArgumentException($"{nameof(memberSelector)} is not {typeof(MemberExpression).Name} type");
            return new PropertyPrintingConfig<TOwner, TPropType>(this, config, exp.Member);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression exp)
                config.Excluding.Add(exp.Member);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            config.Excluding.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return new Printer(config).PrintToString(obj);
        }
    }
}