using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly HashSet<Type> typeBlackList = new HashSet<Type>();
        private readonly HashSet<string> nameBlackList = new HashSet<string>();

        private readonly Dictionary<Type, Delegate> printingFunctionsByType =
            new Dictionary<Type, Delegate>();

        private readonly Dictionary<string, Delegate> printingFunctionsByName =
            new Dictionary<string, Delegate>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new Exception();
            var expr = memberSelector.Body as MemberExpression;
            var name = expr?.Member.DeclaringType?.FullName + "." + expr?.Member.Name;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, name);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new Exception();
            var expr = memberSelector.Body as MemberExpression;
            var name = expr?.Member.DeclaringType?.FullName + "." + expr?.Member.Name;
            nameBlackList.Add(name);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            foreach (var property in typeof(TOwner).GetProperties())
            {
                var type = property.PropertyType;
                if (type == typeof(TPropType))
                    typeBlackList.Add(type);
            }

            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, null);
        }

        private string PrintToString(object obj, int nestingLevel, PropertyInfo property)
        {
            var indentation = new string('\t', nestingLevel + 1);
            if (nestingLevel > 10) return indentation + "Max nesting level";
            var sb = new StringBuilder();
            if (obj == null)
                return "null" + Environment.NewLine;

            if (IsElementInBlackList(obj))
                return sb.ToString();
            if (obj is ICollection collection)
                return PrintCollection(obj, collection, nestingLevel, property);

            var stringToAppend = string.Empty;
            if (NeedToPrintWithNameFunction(property))
                stringToAppend = PrintUsingNameFunction(obj, property);
            if (NeedToPrintWithTypeFunction(obj))
                stringToAppend = PrintUsingTypeFunction(obj);
            if (!NeedToPrintWithNameFunction(property) && !NeedToPrintWithTypeFunction(obj))
                stringToAppend = PrintAsUsual(obj, nestingLevel);
            sb.Append(stringToAppend);
            return sb.ToString();
        }

        private string PrintCollection(object obj, IEnumerable collection, int nestingLevel, PropertyInfo property)
        {
            var sb = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);
            sb.Append(obj.GetType().Name + "\r\n");
            foreach (var element in collection)
                sb.Append(indentation + PrintToString(element, nestingLevel + 1, property));
            return sb.ToString();
        }

        private bool IsElementInBlackList(object obj)
        {
            var property = obj as PropertyInfo;
            return
                typeBlackList.Contains(property?.PropertyType) ||
                nameBlackList.Contains(property?.DeclaringType?.FullName + "." + property?.Name);
        }

        private bool NeedToPrintWithTypeFunction(object obj)
        {
            return printingFunctionsByType.ContainsKey(obj.GetType());
        }

        private bool NeedToPrintWithNameFunction(PropertyInfo property)
        {
            return property != null &&
                   printingFunctionsByName.ContainsKey(property.DeclaringType?.FullName + "." + property.Name);
        }

        private string PrintUsingTypeFunction(object obj)
        {
            return printingFunctionsByType[obj.GetType()]
                .DynamicInvoke(obj)
                .ToString();
        }

        private string PrintUsingNameFunction(object obj, PropertyInfo property)
        {
            return printingFunctionsByName[property.DeclaringType?.FullName + "." + property.Name]
                .DynamicInvoke(obj)
                .ToString();
        }

        private string PrintAsUsual(object obj, int nestingLevel)
        {
            var sb = new StringBuilder();
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.Name.Equals("SyncRoot")) continue;
                if (!IsElementInBlackList(propertyInfo))
                    sb.Append(indentation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj),
                                  nestingLevel + 1, propertyInfo));
            }

            return sb.ToString();
        }

        Dictionary<Type, Delegate> IPrintingConfig.PrintingFunctionsByType =>
            printingFunctionsByType;

        Dictionary<string, Delegate> IPrintingConfig.PrintingFunctionsByName =>
            printingFunctionsByName;
    }
}