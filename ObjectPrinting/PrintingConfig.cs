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
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private const int MaxNestingLevel = 10;
        private const int MaxCollectionSize = 1000;
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<string> excludedProperties = new HashSet<string>();
        private readonly Dictionary<Type, Delegate> typePrinting = new Dictionary<Type, Delegate>();
        private readonly Dictionary<Type, CultureInfo> culturesForPrinting = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<string, Delegate> memberPrinting = new Dictionary<string, Delegate>();


        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetMemberName(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedProperties.Add(GetMemberName(memberSelector));
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (nestingLevel == MaxNestingLevel)
                return "maximum nesting depth" + Environment.NewLine;
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };
            var type = obj.GetType();
            if (finalTypes.Contains(obj.GetType()))
            {
                if (culturesForPrinting.ContainsKey(type))
                    return Convert.ToString(obj, culturesForPrinting[type]) + Environment.NewLine;
                return obj + Environment.NewLine;
                
            }
            var sb = new StringBuilder();

            sb.AppendLine(type.Name);
            foreach (var propertyInfo in GetMembers(obj))
            {
                AppendFormatedProperty(sb, propertyInfo, nestingLevel);
            }

            return sb.ToString();
        }

        private void AppendFormatedProperty(StringBuilder builder,PrintInfo info,
            int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var start = indentation + info.Definition;
            if (excludedTypes.Contains(info.Type))
                return;
            if (excludedProperties.Contains(info.Name) && nestingLevel == 0)
                return;
            if (memberPrinting.ContainsKey(info.Name) && nestingLevel == 0)
                builder.Append(start + PrintToString(memberPrinting[info.Name].DynamicInvoke(info.Item), nestingLevel + 1));
            else if (typePrinting.ContainsKey(info.Type))
                builder.Append(start + PrintToString(typePrinting[info.Type].DynamicInvoke(info.Item), nestingLevel + 1));
            else
                builder.Append(start + PrintToString(info.Item, nestingLevel + 1));

        }

        private IEnumerable<PrintInfo> GetMembers(object obj)
        {
            if (obj is IEnumerable collection)
            {
                var count = 0;
                foreach (var item in collection)
                {
                    count++;
                    yield return new PrintInfo(item, item.GetType());
                    if (count == MaxCollectionSize)
                        yield break;
                }
            }
            else
            {
                var type = obj.GetType();

                foreach (var propertyInfo in type.GetProperties())
                    yield return new PrintInfo(propertyInfo.GetValue(obj), propertyInfo.PropertyType,
                        propertyInfo.Name, propertyInfo.Name + " = ");
                foreach (var fieldInfo in type.GetFields())
                    yield return new PrintInfo(fieldInfo.GetValue(obj), fieldInfo.FieldType,
                        fieldInfo.Name, fieldInfo.Name + " = ");
            }
        }

        private static string GetMemberName<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = (PropertyInfo) ((MemberExpression) memberSelector.Body).Member;
            return propertyInfo.Name;
        }

        Dictionary<Type, Delegate> IPrintingConfig.TypePrinting => typePrinting;

        Dictionary<Type, CultureInfo> IPrintingConfig.CulturesForPrinting => culturesForPrinting;

        Dictionary<string, Delegate> IPrintingConfig.MemberPrinting => memberPrinting;
    }

    public interface IPrintingConfig
    {
        Dictionary<Type, Delegate> TypePrinting { get; }

        Dictionary<Type, CultureInfo> CulturesForPrinting { get; }

        Dictionary<string, Delegate> MemberPrinting { get; }
    }
}