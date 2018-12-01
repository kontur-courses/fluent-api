using System;
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
        private readonly Lazy<HashSet<object>> printedObjects = new Lazy<HashSet<object>>();

        public PrintingConfig()
        {
            _changedTypes = ImmutableDictionary<Type, Delegate>.Empty;
            _forbiddenTypes = ImmutableHashSet<Type>.Empty;
            _forbiddenNames = ImmutableHashSet<string>.Empty;
        }

        private PrintingConfig(
            ImmutableHashSet<string> names,
            ImmutableHashSet<Type> types,
            ImmutableDictionary<Type, Delegate> changedTypes)
        {
            _forbiddenNames = names;
            _changedTypes = changedTypes;
            _forbiddenTypes = types;
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.With<TPropType>(Func<TPropType, string> printer)
        {
            var changedTypes = _changedTypes.SetItem(typeof(TPropType), printer);

            return new PrintingConfig<TOwner>(_forbiddenNames, _forbiddenTypes, changedTypes);

        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExpression = memberSelector.Body as MemberExpression ??
                                   throw new ArgumentException("Selector expression is invalid");
            var propertyInfo = memberExpression.Member as PropertyInfo ??
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

        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            var types = _forbiddenTypes.Add(typeof(TPropType));
            return new PrintingConfig<TOwner>(_forbiddenNames, types, _changedTypes);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (printedObjects.Value.Contains(obj))
                throw new ArgumentException("Object has some lööps");
            printedObjects.Value.Add(obj);
            if (obj == null)
                return "null" + Environment.NewLine;
            var type = obj.GetType();

            if (_changedTypes.ContainsKey(type))
                return (string) _changedTypes[type]
                           .DynamicInvoke(obj) +
                       Environment.NewLine;

            if (obj.GetType()
                   .IsFinal())
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            if (_forbiddenTypes.Contains(type))
                return null;
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties()
                                             .Where(info => !_forbiddenNames.Contains(info.Name)))
            {
                var innerPrint = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
                if (innerPrint != null)
                    sb.Append(indentation + propertyInfo.Name + " = " + innerPrint);
            }

            return sb.ToString();
        }
    }
}
