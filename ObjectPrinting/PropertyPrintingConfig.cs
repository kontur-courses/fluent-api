using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPrintingConfigContainer<TOwner>, IMemberConfig
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Expression<Func<TOwner, TPropType>> selector;
        private readonly MemberInfo relatedMember;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
            Expression<Func<TOwner, TPropType>> selector)
        {
            if (selector.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new ArgumentException();
            }

            this.printingConfig = printingConfig;
            this.selector = selector;
            relatedMember = ((MemberExpression) selector.Body).Member;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
        {
            return printingConfig;
        }

        PrintingConfig<TOwner> IPrintingConfigContainer<TOwner>.PrintingConfig => printingConfig;

        MemberInfo IMemberConfig.GetMember => relatedMember;
    }
}
