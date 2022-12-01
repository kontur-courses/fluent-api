using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class BasePrintingConfig<TOwner>
    {
        private protected Dictionary<MemberInfo, object> SerializedProperties;
        private protected Dictionary<Type, object> SerializedTypes;
        private protected List<Type> ExcludedTypes;
        private protected List<string> ExcludedProperties;

        private protected readonly Type[] FinalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private protected int MaxNestingLevel { get; set; } = 10;

        protected BasePrintingConfig()
        {
            SerializedProperties = new Dictionary<MemberInfo, object>();
            SerializedTypes = new Dictionary<Type, object>();
            ExcludedProperties = new List<string>();
            ExcludedTypes = new List<Type>();
        }

        protected BasePrintingConfig(BasePrintingConfig<TOwner> printingConfig)
        {
            SerializedProperties = printingConfig.SerializedProperties;
            SerializedTypes = printingConfig.SerializedTypes;
            ExcludedProperties = printingConfig.ExcludedProperties;
            ExcludedTypes = printingConfig.ExcludedTypes;
        }
    }
}