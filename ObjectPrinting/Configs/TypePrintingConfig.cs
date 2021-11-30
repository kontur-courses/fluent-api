using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TType>
    {
        internal PrintingConfig<TOwner> ParentConfig { get; }
        internal readonly List<MemberPrintingConfig<TOwner, TType>> MemberConfigs;

        public TypePrintingConfig
            (PrintingConfig<TOwner> parentConfig)
        {
            ParentConfig = parentConfig;
            MemberConfigs = new List<MemberPrintingConfig<TOwner, TType>>();
            TypeExtensions.GetAllMembersOfType<TOwner, TType>(parentConfig.Config.FinalTypes)
                .ForEach(m => 
                    MemberConfigs.Add(new MemberPrintingConfig<TOwner, TType>(ParentConfig, m)));
        }
        
        public PrintingConfig<TOwner> Using(Func<TType, string> serializeFunc)
        {
            MemberConfigs.ForEach(c => c.Using(serializeFunc));
            return ParentConfig;
        }
    } 
}