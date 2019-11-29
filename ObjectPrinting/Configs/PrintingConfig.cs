using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting.Configs
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<MemberInfo> excludedMembers;

        private readonly Dictionary<Type, IPropertySerializingConfig<TOwner>> typesConfigs;
        private readonly Dictionary<MemberInfo, IPropertySerializingConfig<TOwner>> membersConfigs;

        public PrintingConfig() : this(new HashSet<Type>(), new HashSet<MemberInfo>(),
            new Dictionary<Type, IPropertySerializingConfig<TOwner>>(),
            new Dictionary<MemberInfo, IPropertySerializingConfig<TOwner>>())
        {
        }

        private PrintingConfig(PrintingConfig<TOwner> config) : this(
            new HashSet<Type>(config.excludedTypes), new HashSet<MemberInfo>(config.excludedMembers),
            new Dictionary<Type, IPropertySerializingConfig<TOwner>>(config.typesConfigs),
            new Dictionary<MemberInfo, IPropertySerializingConfig<TOwner>>(config.membersConfigs))
        {
        }

        public PrintingConfig(
            HashSet<Type> excludedTypes, HashSet<MemberInfo> excludedMembers,
            Dictionary<Type, IPropertySerializingConfig<TOwner>> typesConfigs,
            Dictionary<MemberInfo, IPropertySerializingConfig<TOwner>> membersConfigs)
        {
            this.excludedTypes = excludedTypes;
            this.excludedMembers = excludedMembers;
            this.typesConfigs = typesConfigs;
            this.membersConfigs = membersConfigs;
        }

        public string PrintToString(TOwner obj)
        {
            var printer = new Printer<TOwner>(excludedTypes, excludedMembers, typesConfigs, membersConfigs);

            return printer.PrintToString(obj);
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            var newExcludedTypes = new HashSet<Type>(excludedTypes) {typeof(T)};

            return new PrintingConfig<TOwner>(newExcludedTypes, excludedMembers, typesConfigs, membersConfigs);
        }

        public PropertySerializingConfig<TOwner, T> Serializing<T>()
        {
            var newPrinterConfig = new PrintingConfig<TOwner>(this);

            return new PropertySerializingConfig<TOwner, T>(newPrinterConfig);
        }

        internal void AddPropertySerializingConfig<TPropType>(
            IPropertySerializingConfig<TOwner> config)
        {
            typesConfigs.Add(typeof(TPropType), config);
        }

        internal void AddPropertySerializingConfig(
            IPropertySerializingConfig<TOwner> config, MemberInfo targetMember)
        {
            membersConfigs.Add(targetMember, config);
        }

        public PropertySerializingConfig<TOwner, T> Serializing<T>(Expression<Func<TOwner, T>> func)
        {
            var property = ((MemberExpression) func.Body).Member;
            var newPrintingConfig = new PrintingConfig<TOwner>(this);

            return new PropertySerializingConfig<TOwner, T>(newPrintingConfig, property);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            var member = ((MemberExpression) func.Body).Member;
            var newExcludedMembers = new HashSet<MemberInfo>(excludedMembers) {member};

            return new PrintingConfig<TOwner>(excludedTypes, newExcludedMembers, typesConfigs, membersConfigs);
        }
    }
}