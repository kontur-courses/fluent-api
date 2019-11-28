using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting.Configs
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();

        private readonly Dictionary<Type, IPropertySerializingConfig<TOwner>> typesConfigs =
            new Dictionary<Type, IPropertySerializingConfig<TOwner>>();

        private readonly Dictionary<MemberInfo, IPropertySerializingConfig<TOwner>> membersConfig =
            new Dictionary<MemberInfo, IPropertySerializingConfig<TOwner>>();

        public string PrintToString(TOwner obj)
        {
            var printer = new Printer<TOwner>(excludedTypes, excludedMembers, typesConfigs, membersConfig);

            return printer.PrintToString(obj);
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes.Add(typeof(T));

            return this;
        }

        public PropertySerializingConfig<TOwner, T> Serializing<T>()
        {
            return new PropertySerializingConfig<TOwner, T>(this);
        }

        public void AddPropertySerializingConfig<TPropType>(
            IPropertySerializingConfig<TOwner> config)
        {
            typesConfigs.Add(typeof(TPropType), config);
        }

        public void AddPropertySerializingConfig<TPropType>(
            IPropertySerializingConfig<TOwner> config, MemberInfo targetMember)
        {
            membersConfig.Add(targetMember, config);
        }

        public PropertySerializingConfig<TOwner, T> Serializing<T>(Expression<Func<TOwner, T>> func)
        {
            var property = ((MemberExpression) func.Body).Member;
            return new PropertySerializingConfig<TOwner, T>(this, property);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            var excludedMember = ((MemberExpression) func.Body).Member;

            excludedMembers.Add(excludedMember);

            return this;
        }
    }
}