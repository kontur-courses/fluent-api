using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedPropertiesTypes = new HashSet<Type>();
        private readonly HashSet<string> excludedPropertiesNames = new HashSet<string>();

        internal int MaxCollectionLength = 1000;

        internal readonly Dictionary<Type, Func<object, string>> PrintingFunctionsByType =
            new Dictionary<Type, Func<object, string>>();

        internal readonly Dictionary<string, Func<object, string>> PrintingFunctionsByName =
            new Dictionary<string, Func<object, string>>();

        internal readonly Dictionary<Type, CultureInfo> CulturesByType = new Dictionary<Type, CultureInfo>();

        internal readonly Dictionary<string, CultureInfo> CulturesByName = new Dictionary<string, CultureInfo>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyName = ((MemberExpression)memberSelector.Body).Member.Name;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyName);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyName = ((MemberExpression)memberSelector.Body).Member.Name;
            excludedPropertiesNames.Add(propertyName);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedPropertiesTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj) => PrintToString(obj, 0, "");

        private string PrintToString(object obj, int nestingLevel, string memberName)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (FinalTypesLibrary.FinalTypes.Contains(obj.GetType()))
                return PrintFinalType(obj, memberName);

            if (obj is IEnumerable collection)
                return PrintCollection(collection);

            return PrintOtherTypes(obj, nestingLevel);
        }

        private string PrintOtherTypes(object obj, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var builder = new StringBuilder();
            var type = obj.GetType();
            var includedMembers = GetIncludedMembers(obj);
            builder.AppendLine(includedMembers.Any() ? type.Name : obj.ToString());
            foreach (var info in includedMembers)
            {
                var value = info.Type == type
                    ? "[Infinite Recursion]" + Environment.NewLine
                    : PrintMember(nestingLevel, info);
                builder.Append(indentation + info.Name + " = " + value);
            }
            return builder.ToString();
        }

        private ClassMemberInfo[] GetIncludedMembers(object obj)
        {
            return obj.GetType()
                .GetProperties()
                .Select(property => new ClassMemberInfo(obj, property))
                .Concat(obj.GetType()
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Select(field => new ClassMemberInfo(obj, field)))
                .Where(info => !excludedPropertiesTypes.Contains(info.Type))
                .Where(info => !excludedPropertiesNames.Contains(info.Name))
                .ToArray();
        }

        private string PrintCollection(IEnumerable collection)
        {
            var builder = new StringBuilder();
            builder.AppendLine(collection.GetType().Name + Environment.NewLine + "{");
            AddCollectionItems(collection, builder);
            builder.Append("}" + Environment.NewLine);
            return builder.ToString();
        }

        private void AddCollectionItems(IEnumerable collection, StringBuilder builder)
        {
            var number = 0;
            foreach (var item in collection)
            {
                if (number > MaxCollectionLength)
                {
                    builder.Append("\t..." + Environment.NewLine);
                    break;
                }
                builder.Append("\t" + PrintToString(item, 1, ""));
                number++;
            }
        }

        private string PrintMember(int nestingLevel, ClassMemberInfo memberInfo)
        {
            Func<object, string> printingFunction = member => PrintToString(member, nestingLevel + 1, memberInfo.Name);
            if (PrintingFunctionsByType.TryGetValue(memberInfo.Type, out var function))
                printingFunction = GetPrintingFunctionWithNewLine(function);
            if (PrintingFunctionsByName.TryGetValue(memberInfo.Name, out function))
                printingFunction = GetPrintingFunctionWithNewLine(function);
            return printingFunction(memberInfo.Value);
        }

        private static Func<object, string> GetPrintingFunctionWithNewLine(Func<object, string> printingFunction)
        {
            return obj => printingFunction(obj) + Environment.NewLine;
        }

        private string PrintFinalType(object obj, string memberName)
        {
            var type = obj.GetType();
            var objectString = obj.ToString();
            if (FinalTypesLibrary.NumberTypes.Contains(type) &&
                (CulturesByName.TryGetValue(memberName, out var culture) || CulturesByType.TryGetValue(type, out culture)))
            {
                objectString = ((IFormattable)obj).ToString(null, culture);
            }
            return objectString + Environment.NewLine;
        }
    }
}