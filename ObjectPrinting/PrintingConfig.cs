using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Dictionary<MemberInfo, Delegate> customMemberSerializers = new();
        private readonly Dictionary<Type, Delegate> customTypeSerializers = new();
        private readonly HashSet<MemberInfo> excludedMembers = new();
        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<Type> finalTypes = new()
        {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan)
        };

        private Dictionary<object, int> visited;

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public MemberPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new MemberPrintingConfig<TOwner, TPropType>(this, GetMemberInfo(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfo = GetMemberInfo(memberSelector);
            excludedMembers.Add(memberInfo);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            visited = new Dictionary<object, int>();
            return PrintToString(obj, 0);
        }

        public void AddCustomTypeSerializer<TPropType>(Type type, Func<TPropType, string> serializer)
        {
            customTypeSerializers[type] = serializer;
        }

        public void AddCustomMemberSerializer<TPropType>(MemberInfo member, Func<TPropType, string> serializer)
        {
            customMemberSerializers[member] = serializer;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return $"null{Environment.NewLine}";

            if (visited.ContainsKey(obj) && visited[obj] < nestingLevel)
                return $"Cyclic Reference{Environment.NewLine}";

            visited[obj] = nestingLevel;

            if (finalTypes.Contains(obj.GetType()))
                return $"{obj}{Environment.NewLine}";

            if (obj is IEnumerable collection)
                return $"{PrintCollection(collection, nestingLevel)}{Environment.NewLine}";

            return PrintMembers(obj, nestingLevel);
        }

        private string PrintMembers(object obj, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            foreach (var memberInfo in type.GetPropertiesAndFields().Where(x => !IsExcluded(x)))
            {
                var serializedMember = TryUseCustomSerializer(memberInfo, memberInfo.GetMemberValue(obj), out var output)
                    ? output
                    : PrintToString(memberInfo.GetMemberValue(obj), nestingLevel + 1);

                sb.Append($"{indentation}{memberInfo.Name} =  {serializedMember}");
            }

            return sb.ToString();
        }

        private static MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = memberSelector.Body as MemberExpression;
            return member?.Member;
        }

        private bool IsExcluded(MemberInfo memberInfo)
        {
            return excludedMembers.Contains(memberInfo) || excludedTypes.Contains(memberInfo.GetTypeOfMember());
        }

        private bool TryUseCustomSerializer(MemberInfo memberInfo, object obj, out string output)
        {
            var type = memberInfo.GetTypeOfMember();

            if (customTypeSerializers.ContainsKey(type) && !customMemberSerializers.ContainsKey(memberInfo))
            {
                output = $"{(string)customTypeSerializers[type].DynamicInvoke(obj)}{Environment.NewLine}";
                return true;
            }

            if (customMemberSerializers.ContainsKey(memberInfo))
            {
                output = $"{(string)customMemberSerializers[memberInfo].DynamicInvoke(obj)}{Environment.NewLine}";
                return true;
            }

            output = null;
            return false;
        }

        private string PrintCollection(IEnumerable collection, int nestingLevel)
        {
            var bracesIndentation = new string('\t', nestingLevel);
            var itemIndentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder($"{bracesIndentation}{{{Environment.NewLine}");

            foreach (var item in collection)
                sb.Append($"{itemIndentation}{PrintToString(item, nestingLevel + 1)}");

            sb.Append($"{bracesIndentation}}}");

            return sb.ToString();
        }
    }
}