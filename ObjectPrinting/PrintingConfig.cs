using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly HashSet<Type> _excludedTypes = new HashSet<Type>();

        private readonly HashSet<MemberInfo> _excludedMembers = new HashSet<MemberInfo>();

        private readonly Dictionary<Type, Delegate> _specSerializationTypes = new Dictionary<Type, Delegate>();

        private readonly Dictionary<MemberInfo, Delegate> _specSerializationMembers = new Dictionary<MemberInfo, Delegate>();

        private readonly HashSet<object> _serializedObjects = new HashSet<object>();

        HashSet<Type> IPrintingConfig<TOwner>.ExcludedTypes => _excludedTypes;

        HashSet<MemberInfo> IPrintingConfig<TOwner>.ExcludedMembers => _excludedMembers;

        Dictionary<Type, Delegate> IPrintingConfig<TOwner>.SpecialSerializationTypes => _specSerializationTypes;

        Dictionary<MemberInfo, Delegate> IPrintingConfig<TOwner>.SpecialSerializationMembers =>
            _specSerializationMembers;

        public MemberPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new MemberPrintingConfig<TOwner, TPropType>(this);
        }

        public MemberPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = ((MemberExpression)memberSelector.Body).Member;
            return new MemberPrintingConfig<TOwner, TPropType>(this, member);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = ((MemberExpression)memberSelector.Body).Member;
            _excludedMembers.Add(member);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            _excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj is null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (_serializedObjects.Contains(obj))
                return "Stop It!" + Environment.NewLine;
            _serializedObjects.Add(obj);
            if (_specSerializationTypes.ContainsKey(type))
                return _specSerializationTypes[type].DynamicInvoke(obj) + Environment.NewLine;
            
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            
            var identation = GetIdentation('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var memberInfo in type.GetFieldsAndProperties())
            {
                if (IsExclude(memberInfo))
                    continue;
                var value = memberInfo.GetValue(obj);
                var serializedObj = _specSerializationMembers.TryGetValue(memberInfo, out var result)
                    ? result.DynamicInvoke(value) + Environment.NewLine
                    : PrintToString(value, nestingLevel + 1);
                sb.Append(
                    $"{identation}{memberInfo.Name} = {serializedObj}");
            }
            _serializedObjects.Clear();
            return sb.ToString();
        }
        
        private string GetIdentation(char identationSymbol, int nestingLevel)
        {
            return new string(identationSymbol, nestingLevel);

        }

        private bool IsExclude(MemberInfo memberInfo)
        {
            var type = memberInfo.GetValueType();
            return _excludedTypes.Contains(type) || _excludedMembers.Contains(memberInfo);
        }
    }

}