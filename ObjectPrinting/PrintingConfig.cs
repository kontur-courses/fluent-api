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
        private readonly HashSet<string> forbiddenNames = new HashSet<string>();
        private readonly HashSet<Type> forbiddenTypes = new HashSet<Type>();
        private readonly Dictionary<Type, Delegate> changedTypes = new Dictionary<Type, Delegate>();

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.With<TPropType>(Func<TPropType, string> printer)
        {
            changedTypes[typeof(TPropType)] = printer;
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>() =>
            new PropertyPrintingConfig<TOwner, TPropType>(this);

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector) =>
            new PropertyPrintingConfig<TOwner, TPropType>(this);

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var name =
                ((memberSelector.Body as MemberExpression ??
                  throw new ArgumentException("Selector expression is invalid")).Member as PropertyInfo ??
                 throw new ArgumentException("Selector expression is invalid")).Name;
            forbiddenNames.Add(name);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            forbiddenTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;
            var type = obj.GetType();

            if (changedTypes.ContainsKey(type))
                return (string) changedTypes[type]
                           .DynamicInvoke(obj) +
                       Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(string), typeof(int), typeof(double), typeof(long),
                typeof(float), typeof(decimal), typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            if (forbiddenTypes.Contains(type))
                return null;
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties()
                                             .Where(info => !forbiddenNames.Contains(info.Name)))
            {
                var innerPrint = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
                if (innerPrint != null)
                    sb.Append(indentation + propertyInfo.Name + " = " + innerPrint);
            }

            return sb.ToString();
        }
    }
}
