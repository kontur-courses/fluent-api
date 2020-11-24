using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfigBase
    {
        protected Func<object, string> PrintFunc;

        protected ImmutableDictionary<MemberInfo, PrintingConfigBase> MemberConfigs =
            ImmutableDictionary<MemberInfo, PrintingConfigBase>.Empty;

        protected ImmutableDictionary<Type, PrintingConfigBase> TypeConfigs =
            ImmutableDictionary<Type, PrintingConfigBase>.Empty;

        protected virtual PrintingConfigBase Copy() => new PrintingConfigBase
        {
            PrintFunc = PrintFunc,
            MemberConfigs = MemberConfigs,
            TypeConfigs = TypeConfigs
        };

        private PrintingConfigBase WithParentTypeConfigs(
            ImmutableDictionary<Type, PrintingConfigBase> parentTypeConfigs)
        {
            var newItems = parentTypeConfigs.Where(p => !TypeConfigs.ContainsKey(p.Key));
            var result = Copy();
            result.TypeConfigs = TypeConfigs.AddRange(newItems);
            return result;
        }

        protected PrintingConfigBase WithConfigForMember(MemberInfo[] memberChain, PrintingConfigBase config)
            => Copy().WithConfigForMember(memberChain, config, 0);

        private PrintingConfigBase WithConfigForMember(MemberInfo[] memberChain, PrintingConfigBase config,
            int indexInChain)
        {
            if (indexInChain == memberChain.Length - 1)
                return WithConfigForMember(memberChain[indexInChain], config);

            if (!MemberConfigs.TryGetValue(memberChain[indexInChain], out var currentConfig))
                currentConfig = new PrintingConfigBase();

            var result = Copy();
            result.MemberConfigs = result.MemberConfigs.SetItem(memberChain[indexInChain],
                currentConfig.WithConfigForMember(memberChain, config, indexInChain + 1));
            return result;
        }

        protected PrintingConfigBase WithConfigForMemberOrDefault(
            MemberInfo[] memberChain,
            PrintingConfigBase defaultConfig,
            out PrintingConfigBase config)
        {
            config = this;
            for (var i = 0; i < memberChain.Length; i++)
            {
                var hasConfig = config.MemberConfigs.TryGetValue(memberChain[i], out config);
                if (!hasConfig || config == null)
                {
                    if (i < memberChain.Length - 1 || !hasConfig || defaultConfig != null)
                        return WithConfigForMember(memberChain, config = defaultConfig);
                    config = null;
                    return this;
                }
            }

            return this;
        }

        private PrintingConfigBase WithConfigForMember(MemberInfo memberChain, PrintingConfigBase config)
        {
            var result = Copy();
            result.MemberConfigs = result.MemberConfigs.SetItem(memberChain, config);
            return result;
        }

        protected static PrintingConfig<T> ToGeneric<T>(PrintingConfigBase config)
        {
            if (config is PrintingConfig<T> generic) return generic;
            if (config == null) return null;
            if (config.GetType() == typeof(PrintingConfigBase))
            {
                return new PrintingConfig<T>
                {
                    PrintFunc = config.PrintFunc,
                    MemberConfigs = config.MemberConfigs,
                    TypeConfigs = config.TypeConfigs
                };
            }

            throw new Exception($"{config} is not generic of {typeof(T)}!");
        }

        protected string PrintToString(object obj, int nestingLevel)
        {
            if (nestingLevel > 5) return "...";
            if (obj == null) return "null";

            if (PrintFunc != null) return PrintFunc(obj);
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType())) return obj.ToString();

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (!(MemberConfigs.TryGetValue(propertyInfo, out var config)
                      || TypeConfigs.TryGetValue(propertyInfo.PropertyType, out config)))
                    config = new PrintingConfigBase {TypeConfigs = TypeConfigs};
                else if (config == null) continue;
                else config = config.WithParentTypeConfigs(TypeConfigs);

                sb.Append(identation + propertyInfo.Name + " = " +
                          config.PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1)
                          + Environment.NewLine);
            }

            return sb.ToString();
        }
    }
}