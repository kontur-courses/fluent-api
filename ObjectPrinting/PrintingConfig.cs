using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly ImmutableDictionary<Type, Delegate> _changedTypes;

        private readonly ImmutableHashSet<string> _forbiddenNames;

        private readonly ImmutableHashSet<Type> _forbiddenTypes;
        private readonly string _newLine = Environment.NewLine;
        private readonly Lazy<HashSet<MemberInfo>> _printedObjects = new Lazy<HashSet<MemberInfo>>();
        private readonly int _restrictionAmount = 100;

        public PrintingConfig()
        {
            _changedTypes = ImmutableDictionary<Type, Delegate>.Empty;
            _forbiddenTypes = ImmutableHashSet<Type>.Empty;
            _forbiddenNames = ImmutableHashSet<string>.Empty;
        }

        private PrintingConfig(
            ImmutableHashSet<string> names,
            ImmutableHashSet<Type> types,
            ImmutableDictionary<Type, Delegate> changedTypes,
            int restriction = 1000)
        {
            _forbiddenNames = names;
            _changedTypes = changedTypes;
            _forbiddenTypes = types;
            _restrictionAmount = restriction;
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.With<TPropType>(Func<TPropType, string> printer)
        {
//            if (_changedTypes.TryGetValue(typeof(TPropType), out var oldPrinter))
//            {
//                var temporaryPrinter = printer;
//                printer = t => (string) oldPrinter.DynamicInvoke(temporaryPrinter(t));
//            }

            var changedTypes = _changedTypes.SetItem(typeof(TPropType), printer);

            return new PrintingConfig<TOwner>(_forbiddenNames, _forbiddenTypes, changedTypes);
        }

        public PrintingConfig<TOwner> WithSequenceAmountRestriction(int restriction) =>
            new PrintingConfig<TOwner>(_forbiddenNames, _forbiddenTypes, _changedTypes, restriction);

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression memberExpression &&
                  memberExpression.Member is PropertyInfo propertyInfo))
                throw new ArgumentException("Selector expression is invalid");

            var name = propertyInfo.Name;
            var names = _forbiddenNames.Add(name);
            return new PrintingConfig<TOwner>(names, _forbiddenTypes, _changedTypes);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>() =>
            new PropertyPrintingConfig<TOwner, TPropType>(this);

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector) =>
            new PropertyPrintingConfig<TOwner, TPropType>(this);

        /// <exception cref="ArgumentException">Obj may have some lööps</exception>
        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            var types = _forbiddenTypes.Add(typeof(TPropType));
            return new PrintingConfig<TOwner>(_forbiddenNames, types, _changedTypes);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + _newLine;
            var type = obj.GetType();

            if (_changedTypes.ContainsKey(type))
                return (string) _changedTypes[type]
                           .DynamicInvoke(obj) +
                       _newLine;

            if (_forbiddenTypes.Contains(type))
                return null;

            if (type.IsFinal())
                return obj + _newLine;

            return PrintActualPropertiesAndFields(obj, nestingLevel, type);
        }

        private string PrintActualPropertiesAndFields(object obj, int nestingLevel, Type type)
        {
            var sb = new StringBuilder();

            sb.AppendLine(type.Name);

            var indentation = new string('\t', nestingLevel + 1);

            foreach (var memberInfo in type.GetMembers()
                                           .Where(m => !_forbiddenNames.Contains(m.Name)))
            {
                if (!memberInfo.TryGetValue(obj, out var value))
                    continue;

                if (_printedObjects.Value.Contains(memberInfo))
                    throw new ArgumentException("Object has some lööps");
                _printedObjects.Value.Add(memberInfo);

                var innerPrint = PrintToString(value, nestingLevel + 1);
                if (innerPrint != null)
                    sb.Append(indentation + memberInfo.Name + " = " + innerPrint);
            }

            if (obj is IEnumerable enumerable)
                return sb + PrintEnumerableToString(enumerable, nestingLevel + 2);

            return sb.ToString();
        }

        private string PrintEnumerableToString(IEnumerable enumerable, int nestingLevel)
        {
            var sb = new StringBuilder();
            var indentation = new string('\t', nestingLevel);
            sb.AppendLine("Containing:");
            var counter = 1;
            foreach (var obj in enumerable)
            {
                sb.Append(indentation + PrintToString(obj, nestingLevel));
                if (++counter > _restrictionAmount)
                    break;
            }

            return sb.ToString();
        }
    }
}
