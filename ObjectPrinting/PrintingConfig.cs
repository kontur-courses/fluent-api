using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.TypesHandlers;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedProperties = new HashSet<Type>();
        internal readonly IDictionary<Type, Delegate> TypesSerializers = new Dictionary<Type, Delegate>();
        internal readonly IDictionary<Type, CultureInfo> CustomCultures = new Dictionary<Type, CultureInfo>();
        internal readonly IDictionary<PropertyInfo, Delegate> PropertiesSerializers =
            new Dictionary<PropertyInfo, Delegate>();
        internal readonly IDictionary<PropertyInfo, int> StringsTrimValues = new Dictionary<PropertyInfo, int>();
        private readonly HashSet<PropertyInfo> excludedSpecificProperties = new HashSet<PropertyInfo>();

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedProperties.Add(typeof(TPropType));

            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> selector)
        {
            var member = (MemberExpression)selector.Body;

            return new PropertyPrintingConfig<TOwner, TPropType>(this, (PropertyInfo)member.Member);
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> selector)
        {
            var member = (MemberExpression)selector.Body;
            excludedSpecificProperties.Add((PropertyInfo)member.Member);

            return this;
        }

        public string PrintToString(TOwner obj)
        {
            var handler = new FinalTypesHandler();
            var propertiesHandler = new PropertiesHandler(
                excludedProperties.ToImmutableHashSet(),
                (IReadOnlyDictionary<Type, Delegate>)TypesSerializers,
                (IReadOnlyDictionary<Type, CultureInfo>)CustomCultures,
                (IReadOnlyDictionary<PropertyInfo, Delegate>)PropertiesSerializers,
                (IReadOnlyDictionary<PropertyInfo, int>)StringsTrimValues,
                excludedSpecificProperties.ToImmutableHashSet());
            handler.SetSuccessor(new IEnumerableHandler().SetSuccessor(propertiesHandler));

            return PrintToString(handler, obj, 0, ImmutableHashSet<object>.Empty);
        }

        private string PrintToString(
            TypeHandler handler,
            object obj,
            int nestingLevel,
            ImmutableHashSet<object> excludedValues)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            return handler.Handle(obj, nestingLevel, excludedValues, handler);
        }
    }
}