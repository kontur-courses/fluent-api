using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting.Configuration
{
    public interface IPrintingConfig<TOwner>
    {
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>();
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector);
        public PrintingConfig<TOwner> AddAlternativeTypeSerializer<TMember>(Delegate alternativeSerializer);
        public PrintingConfig<TOwner> AddAlternativeTypeSerializer(MemberInfo memberInfo, Delegate alternativeSerializer);
        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector);
        public PrintingConfig<TOwner> Excluding<TPropType>();
        public PrintingConfig<TOwner> IgnoringCyclicReferences(bool ignore = true);
        public ObjectPrinter<TOwner> Build();
    }
}