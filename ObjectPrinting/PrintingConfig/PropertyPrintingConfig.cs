using System;
using System.Reflection;

namespace ObjectPrinting.PrintingConfig
{
    public class PropertyPrintingConfig<TOwner, TType> : INestingPrintingConfig<TOwner, TType>
    {
        private readonly PrintingConfig<TOwner> parent;
        private readonly SerializationSettings settings;
        private readonly MemberInfo memberInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parent, SerializationSettings settings,
            MemberInfo memberInfo)
        {
            this.parent = parent;
            this.settings = settings;
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Use(Func<TType, string> transformer)
        {
            if (transformer == null) throw new ArgumentNullException(nameof(transformer));
            settings.SetTransformer(memberInfo, transformer);
            return parent;
        }
    }
}