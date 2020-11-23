using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public readonly Dictionary<Type, CultureInfo> CultureTypes =
            new Dictionary<Type, CultureInfo>();

        public readonly HashSet<MemberInfo> ExcludingMembers = new HashSet<MemberInfo>();
        public readonly HashSet<Type> ExcludingTypes = new HashSet<Type>();

        public readonly HashSet<Type> FinalTypes = new HashSet<Type>
        {
            typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(DateTimeOffset)
        };

        public readonly Dictionary<MemberInfo, Delegate> PrintingFunctionsForMembers =
            new Dictionary<MemberInfo, Delegate>();

        public readonly Dictionary<Type, Delegate> PrintingFunctionsForTypes =
            new Dictionary<Type, Delegate>();

        public readonly HashSet<object> VisitedObjects = new HashSet<object>();

        public string Indentation = "\t";
        public string SeparatorBetweenNameAndValue = "=";

        public PrintingConfig<TOwner> AddCultureForType<TPropType>(CultureInfo culture)
        {
            if (typeof(IFormattable).IsAssignableFrom(typeof(TPropType)))
                CultureTypes[typeof(TPropType)] = culture;
            return this;
        }

        public IPropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this,
                func => PrintingFunctionsForTypes[typeof(TPropType)] = func);
        }

        public IPropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression))
                throw new InvalidCastException("unable to cast to MemberExpression");
            var member = ((MemberExpression) memberSelector.Body).Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(this,
                func => PrintingFunctionsForMembers[member] = func);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression))
                throw new InvalidCastException("unable to cast to MemberExpression");
            ExcludingMembers.Add(((MemberExpression) memberSelector.Body).Member);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            ExcludingTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> ChangeIndentation(string newIndentation)
        {
            Indentation = newIndentation;
            return this;
        }

        public PrintingConfig<TOwner> ChangeSeparatorBetweenNameAndValue(string separator)
        {
            SeparatorBetweenNameAndValue = separator;
            return this;
        }
    }
}