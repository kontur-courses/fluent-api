using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Interfaces;

namespace ObjectPrinting.Core
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly Dictionary<Type, Delegate> _alternativeSerializationByTypes;
        private readonly Dictionary<string, Delegate> _alternativeSerializationByNames;
        private readonly HashSet<Type> _excludingTypes;
        private readonly HashSet<string> _excludingNames;
        private readonly HashSet<object> _parents;

        private static HashSet<Type> _finalTypes = new HashSet<Type>
        {
            typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(decimal), typeof(Guid)
        };

        private const int MaxNestingLevel = 10;

        Dictionary<Type, Delegate> IPrintingConfig.AlternativeSerializationByTypes =>
            _alternativeSerializationByTypes;

        Dictionary<string, Delegate> IPrintingConfig.AlternativeSerializationByNames
            => _alternativeSerializationByNames;

        public PrintingConfig()
        {
            _alternativeSerializationByTypes = new Dictionary<Type, Delegate>();
            _alternativeSerializationByNames = new Dictionary<string, Delegate>();
            _excludingTypes = new HashSet<Type>();
            _excludingNames = new HashSet<string>();
            _parents = new HashSet<object>();
        }

        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>()
        {
            return new MemberPrintingConfig<TOwner, TMemberType>(this);
        }

        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression memberExpression))
                throw new Exception("Expression type must be MemberExpression");
            return new MemberPrintingConfig<TOwner, TMemberType>(this, memberExpression.Member.Name);
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression memberExpression))
                throw new Exception("Expression type must be MemberExpression");
            _excludingNames.Add(memberExpression.Member.Name);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>()
        {
            _excludingTypes.Add(typeof(TMemberType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (nestingLevel > MaxNestingLevel)
                return "Nesting level exceeded\r\n";
            if (obj == null)
                return "null" + Environment.NewLine;
            var type = obj.GetType();
            if (_finalTypes.Contains(type) || type.IsPrimitive)
                return obj + Environment.NewLine;
            if (_parents.Contains(obj))
                return $"Type = {type.Name}, Value = {obj} : found a cyclic link\r\n";
            _parents.Add(obj);
            return GetSerializedMembers(obj, nestingLevel);
        }

        private string GetSerializedMembers(object obj, int nestingLevel)
        {
            var serialized = new StringBuilder().AppendLine(obj.GetType().Name);
            foreach (var elementInfo in GetElementsInfo(obj).Where(IsCorrectMember))
                serialized.Append(GetSerializedMember(elementInfo, nestingLevel));
            return serialized.ToString();
        }

        private static IEnumerable<ElementInfo> GetElementsInfo(object obj)
        {
            var objectType = obj.GetType();
            foreach (var propertyInfo in objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                yield return new ElementInfo(propertyInfo, obj);
            foreach (var fieldInfo in objectType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                yield return new ElementInfo(fieldInfo, obj);
        }

        private bool IsCorrectMember(ElementInfo elementInfo)
        {
            return !_excludingTypes.Contains(elementInfo.ElementType) &&
                   !_excludingNames.Contains(elementInfo.NameElement);
        }

        private string GetSerializedMember(ElementInfo elementInfo, int nestingLevel)
        {
            var member = new StringBuilder();
            var partResult = new string('\t', nestingLevel + 1) + elementInfo.NameElement + " = ";
            if (elementInfo.Value != null && _alternativeSerializationByNames.ContainsKey(elementInfo.NameElement))
            {
                var serializationResult = (string) _alternativeSerializationByNames[elementInfo.NameElement]
                    .DynamicInvoke(elementInfo.Value);
                member.Append(partResult + serializationResult + Environment.NewLine);
            }
            else if (elementInfo.Value != null && _alternativeSerializationByTypes.ContainsKey(elementInfo.ElementType))
            {
                var serializationResult = (string) _alternativeSerializationByTypes[elementInfo.ElementType]
                    .DynamicInvoke(elementInfo.Value);
                member.Append(partResult + serializationResult + Environment.NewLine);
            }
            else member.Append(partResult + PrintToString(elementInfo.Value, nestingLevel + 1));

            return member.ToString();
        }
    }
}