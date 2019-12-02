using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Dictionary<Type, Func<object, string>> specialPrintingFunctionsForTypes;
        private readonly Dictionary<MemberInfo, Func<object, string>> specialPrintingFunctionsForMembers;
        private readonly MemberInfo member;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Dictionary<MemberInfo, 
            Func<object, string>> specialPrintingFunctionsForMembers, MemberInfo member)
        {
            this.printingConfig = printingConfig;
            this.specialPrintingFunctionsForMembers = specialPrintingFunctionsForMembers;
            this.member = member;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, 
            Dictionary<Type, Func<object, string>> specialPrintingFunctionsForTypes)
        {
            this.printingConfig = printingConfig;
            this.specialPrintingFunctionsForTypes = specialPrintingFunctionsForTypes;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (member != null)
            {
                specialPrintingFunctionsForMembers[member] = o => print((TPropType)o);
                return printingConfig;
            }
            specialPrintingFunctionsForTypes[typeof(TPropType)] = o => print((TPropType)o);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        Dictionary<Type, Func<object, string>> IPropertyPrintingConfig<TOwner, TPropType>.SpecialPrintingFunctionsForTypes =>
            specialPrintingFunctionsForTypes;

        Dictionary<MemberInfo, Func<object, string>> IPropertyPrintingConfig<TOwner, TPropType>.SpecialPrintingFunctionsForMembers =>
            specialPrintingFunctionsForMembers;

        MemberInfo IPropertyPrintingConfig<TOwner, TPropType>.Member => member;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        Dictionary<Type, Func<object, string>> SpecialPrintingFunctionsForTypes { get; }
        Dictionary<MemberInfo, Func<object, string>> SpecialPrintingFunctionsForMembers { get; }
        MemberInfo Member { get; }
    }
}