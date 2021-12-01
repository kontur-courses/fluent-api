using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting.Configs
{
    public class TypePrintingConfig<TOwner, TType>
    {
        internal PrintingConfig<TOwner> ParentConfig { get; }
        internal readonly List<MemberPrintingConfig<TOwner, TType>> MemberConfigs;

        public TypePrintingConfig (PrintingConfig<TOwner> parentConfig, List<MemberInfo> members)
        {
            ParentConfig = parentConfig;
            MemberConfigs = members
                .Select(m => new MemberPrintingConfig<TOwner, TType>(ParentConfig, m))
                .ToList();
        }
        
        public PrintingConfig<TOwner> Using(Func<TType, string> serializeFunc)
        {
            MemberConfigs.ForEach(c => c.Using(serializeFunc));
            return ParentConfig;
        }
    } 
}