using System;
using System.Collections;
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
        private readonly Dictionary<ElementInfo, Delegate> _alternativeSerializationByElementsInfo;
        private readonly HashSet<Type> _excludingTypes;
        private readonly HashSet<ElementInfo> _excludingElements;
        private readonly HashSet<object> _visitedInstances;

        private static HashSet<Type> _finalTypes = new HashSet<Type>
        {
            typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(decimal), typeof(Guid)
        };

        private const int MaxNestingLevel = 10;

        Dictionary<Type, Delegate> IPrintingConfig.AlternativeSerializationByTypes =>
            _alternativeSerializationByTypes;

        Dictionary<ElementInfo, Delegate> IPrintingConfig.AlternativeSerializationByElementsInfo
            => _alternativeSerializationByElementsInfo;

        public PrintingConfig()
        {
            _alternativeSerializationByTypes = new Dictionary<Type, Delegate>();
            _alternativeSerializationByElementsInfo = new Dictionary<ElementInfo, Delegate>();
            _excludingTypes = new HashSet<Type>();
            _excludingElements = new HashSet<ElementInfo>();
            _visitedInstances = new HashSet<object>();
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
            if (memberExpression.Member is PropertyInfo propertyInfo)
                return new MemberPrintingConfig<TOwner, TMemberType>(this, new ElementInfo(propertyInfo));
            return new MemberPrintingConfig<TOwner, TMemberType>(this,
                new ElementInfo(memberExpression.Member as FieldInfo));
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression memberExpression))
                throw new Exception("Expression type must be MemberExpression");
            if (memberExpression.Member is PropertyInfo propertyInfo)
                _excludingElements.Add(new ElementInfo(propertyInfo));
            else
                _excludingElements.Add(new ElementInfo(memberExpression.Member as FieldInfo));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>()
        {
            _alternativeSerializationByTypes.Clear();
            _excludingTypes.Add(typeof(TMemberType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            if (_visitedInstances.Contains(obj))
                ClearTempStoragesBeforeNewInstanceSerialization();
            return PrintToString(obj, 0);
        }

        private void ClearTempStoragesBeforeNewInstanceSerialization()
        {
            _alternativeSerializationByTypes.Clear();
            _alternativeSerializationByElementsInfo.Clear();
            _visitedInstances.Clear();
            _excludingElements.Clear();
            _excludingTypes.Clear();
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (nestingLevel > MaxNestingLevel)
                return "Nesting level exceeded\r\n";
            if (obj == null)
                return "null" + Environment.NewLine;
            var type = obj.GetType();
            if (IsFinalType(type))
                return obj + Environment.NewLine;
            if (_visitedInstances.Contains(obj))
                return $"Type = {type.Name}, Value = {obj} : found a cyclic link\r\n";
            _visitedInstances.Add(obj);
            return typeof(ICollection).IsAssignableFrom(type)
                ? GetSerializedCollection(obj, nestingLevel)
                : GetSerializedMembers(obj, nestingLevel);
        }

        private static bool IsFinalType(Type type) => _finalTypes.Contains(type) || type.IsPrimitive;

        private string GetSerializedCollection(object obj, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var result = new StringBuilder().AppendLine(obj.GetType().Name);
            var collection = (IEnumerable) obj;
            foreach (var element in collection)
                result.Append(indentation + PrintToString(element, nestingLevel + 1));
            return result.ToString();
        }

        private string GetSerializedMembers(object obj, int nestingLevel)
        {
            var serialized = new StringBuilder().AppendLine(obj.GetType().Name);
            foreach (var elementInfo in GetElementsInfo(obj).Where(IsNotExcludingMember))
                serialized.Append(GetSerializedMember(obj, elementInfo, nestingLevel));
            return serialized.ToString();
        }

        private static IEnumerable<ElementInfo> GetElementsInfo(object obj)
        {
            var objectType = obj.GetType();
            foreach (var propertyInfo in objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                yield return new ElementInfo(propertyInfo);
            foreach (var fieldInfo in objectType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                yield return new ElementInfo(fieldInfo);
        }

        private bool IsNotExcludingMember(ElementInfo elementInfo)
        {
            return !_excludingTypes.Contains(elementInfo.ElementType) &&
                   !_excludingElements.Contains(elementInfo);
        }

        private string GetSerializedMember(object obj, ElementInfo elementInfo, int nestingLevel)
        {
            var member = new StringBuilder();
            var partResult = new string('\t', nestingLevel + 1) + elementInfo.ElementName + " = ";
            var elementValue = elementInfo.GetValue(obj);
            if (elementValue != null &&
                _alternativeSerializationByElementsInfo.TryGetValue(elementInfo, out var certainMemberSerialization))
            {
                var serializationResult = (string) certainMemberSerialization
                    .DynamicInvoke(elementValue);
                member.Append(partResult + serializationResult + Environment.NewLine);
            }
            else if (elementValue != null &&
                     _alternativeSerializationByTypes.TryGetValue(elementInfo.ElementType,
                         out var certainMemberTypeSerialization))
            {
                var serializationResult = (string) certainMemberTypeSerialization
                    .DynamicInvoke(elementValue);
                member.Append(partResult + serializationResult + Environment.NewLine);
            }
            else member.Append(partResult + PrintToString(elementValue, nestingLevel + 1));

            return member.ToString();
        }
    }
}