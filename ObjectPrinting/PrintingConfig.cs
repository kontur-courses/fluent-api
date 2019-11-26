using System;
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

        private readonly Dictionary<Type, Func<object, string>> PrintingFunctionsByType =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<string, Func<object, string>> PrintingFunctionsByName =
            new Dictionary<string, Func<object, string>>();

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
            var name = expr.Member.DeclaringType.FullName + "." + expr.Member.Name;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, name);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new Exception();
            var expr = memberSelector.Body as MemberExpression;
            var name = expr.Member.DeclaringType.FullName + "." + expr.Member.Name;
            nameBlackList.Add(name);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
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
            var sb = new StringBuilder();
            if (obj == null)
                return "null" + Environment.NewLine;
            sb.Append(TryPrintingFunctionByType(obj, out var specialPrintingFunctionsResult)
                ? specialPrintingFunctionsResult
                : TryPrintingFunctionByName(obj, property, out specialPrintingFunctionsResult)
                    ? specialPrintingFunctionsResult
                    : PrintAsUsual(obj,
                        nestingLevel));
            return sb.ToString();
        }

        private bool TryPrintingFunctionByType(object obj, out string specialPrintingFunctionsResult)
        {
            specialPrintingFunctionsResult = string.Empty;
            if (!PrintingFunctionsByType.ContainsKey(obj.GetType())) return false;
            specialPrintingFunctionsResult = PrintingFunctionsByType[obj.GetType()](obj);
            return true;
        }

        private bool TryPrintingFunctionByName(object obj, PropertyInfo property,
            out string specialPrintingFunctionsResult)
        {
            specialPrintingFunctionsResult = String.Empty;
            if (property == null) return false;
            if (!PrintingFunctionsByName.ContainsKey(property.DeclaringType.FullName + "." + property.Name))
                return false;
            specialPrintingFunctionsResult =
                PrintingFunctionsByName[property.DeclaringType.FullName + "." + property.Name](obj);
            return true;
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

            var identation = new string('\t', nestingLevel + 1);
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (!typeBlackList.Contains(propertyInfo.PropertyType) &&
                    !nameBlackList.Contains(propertyInfo.DeclaringType.FullName + "." + propertyInfo.Name))
                    sb.Append(identation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj),
                                  nestingLevel + 1, propertyInfo));
            }

            return sb.ToString();
        }

        Dictionary<Type, Func<object, string>> IPrintingConfig.PrintingFunctionsByType =>
            PrintingFunctionsByType;

        Dictionary<string, Func<object, string>> IPrintingConfig.PrintingFunctionsByName =>
            PrintingFunctionsByName;
    }
}