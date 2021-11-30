using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.PrintingConfig
{
    public class SerializationSettings
    {
        private readonly HashSet<Type> excludedTypes = new();
        private readonly Dictionary<Type, Func<object, string>> typeTransformers = new();
        private readonly Dictionary<MemberInfo, Func<object, string>> memberTransformers = new();
        private readonly HashSet<MemberInfo> excludedMembers = new();
        public bool IsAllowCycleReference { get; set; }

        public bool TryGetTypeTransformer(Type type, out Func<object, string> transformer) =>
            typeTransformers.TryGetValue(type, out transformer);

        public bool IsExcluded(Type type) => excludedTypes.Contains(type);

        public bool IsExcluded(MemberInfo member) => excludedMembers.Contains(member);

        public bool TryGetMemberTransformer(MemberInfo memberInfo, out Func<object, string> transformer) =>
            memberTransformers.TryGetValue(memberInfo, out transformer);

        public void Exclude(Type type) => excludedTypes.Add(type);

        public void Exclude(MemberInfo memberInfo) => excludedMembers.Add(memberInfo);

        public void SetTransformer<TType>(Func<TType, string> transformer)
        {
            if (transformer == null) throw new ArgumentNullException(nameof(transformer));
            typeTransformers[typeof(TType)] = obj => transformer((TType)obj);
        }

        public void SetTransformer<TType>(MemberInfo memberInfo, Func<TType, string> transformer)
        {
            if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));
            if (transformer == null) throw new ArgumentNullException(nameof(transformer));
            memberTransformers[memberInfo] = obj => transformer((TType)obj);
        }
    }
}