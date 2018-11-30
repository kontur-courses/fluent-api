namespace ObjectPrinting
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly ImmutableDictionary<Type, Delegate> _changedTypes;

        private readonly ImmutableHashSet<string> _forbiddenNames;

        private readonly ImmutableHashSet<Type> _forbiddenTypes;

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

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var name =
                ((memberSelector.Body as MemberExpression
               ?? throw new ArgumentException("Selector expression is invalid")).Member as PropertyInfo
              ?? throw new ArgumentException("Selector expression is invalid")).Name;
            var names = _forbiddenNames.Add(name);
            return new PrintingConfig<TOwner>(names, _forbiddenTypes, _changedTypes);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>() =>
            new PropertyPrintingConfig<TOwner, TPropType>(this);

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector) =>
            new PropertyPrintingConfig<TOwner, TPropType>(this);

        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.With<TPropType>(Func<TPropType, string> printer)
        {
            var changedTypes = _changedTypes.SetItem(typeof(TPropType), printer);

            return new PrintingConfig<TOwner>(_forbiddenNames, _forbiddenTypes, changedTypes);
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            var types = _forbiddenTypes.Add(typeof(TPropType));
            return new PrintingConfig<TOwner>(_forbiddenNames, types, _changedTypes);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            var type = obj.GetType();

            if (_changedTypes.ContainsKey(type))
                return (string)_changedTypes[type]
                           .DynamicInvoke(obj)
                     + Environment.NewLine;

            var finalTypes = new[]
                                 {
                                     typeof(string), typeof(int), typeof(double), typeof(long),
                                     typeof(float), typeof(decimal), typeof(DateTime), typeof(TimeSpan)
                                 };
            if (finalTypes.Contains(obj.GetType()))
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
