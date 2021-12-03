using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        private readonly BodyPrintingConfig<TOwner> body;
        private readonly string newLine = Environment.NewLine;
        private int maxNestingLevel;
        public PrintingConfig()
        {
            body = new BodyPrintingConfig<TOwner>(this);
        }
       
        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var printingConfig = new TypePrintingConfig<TOwner, TPropType>(body);
            return printingConfig;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var printingConfig = new PropertyPrintingConfig<TOwner, TPropType>(body);
            body.PropertyKey = PathToProperty(memberSelector);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var res = PathToProperty(memberSelector);
            body.ExcludingProp.Add(res);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            body.ExcludingTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj, int maxNestingLevel = 1)
        {
            this.maxNestingLevel = maxNestingLevel;
            return PrintToString("", obj, 0);
        }

        public string PrintToString(IEnumerable<TOwner> obj, int maxNestingLevel = 2)
        {
            this.maxNestingLevel = maxNestingLevel;
            var strEle = new List<string>();
            foreach (var el in obj)
            {
                strEle.Add(PrintToString("", el, 1));
            }
            return $"[\n{string.Join(",\n", strEle)}\n]";
        }

        private string PrintToString(string parentPath, object obj, int nestingLevel)
        {
            if (obj == null)
                return "null";

            if (FinalTypes.Types.Contains(obj.GetType()))
                return obj.ToString();

            var type = obj.GetType();
            if (nestingLevel > maxNestingLevel)
                return type.Name;

            var sb = new List<string>();
            sb.Add(type.Name);

            foreach (var propertyInfo in type.GetProperties())
            {
                var pathToProp = string.IsNullOrEmpty(parentPath) ? $"{propertyInfo.Name}" : $"{parentPath}.{propertyInfo.Name}";
                var propType = propertyInfo.PropertyType;

                if (body.ExcludingTypes.Contains(propType) || body.ExcludingProp.Contains(pathToProp))
                    continue;

                sb.Add(PrintProperty(propertyInfo, obj, pathToProp, nestingLevel));
            }
            
            return string.Join(Environment.NewLine, sb);
        }

        private string PrintProperty(PropertyInfo propertyInfo, object obj, string newPath, int nestingLevel)
        {
            if (body.AlternativePrintingProp.Keys.Contains(newPath))
                return AlternativePrint(body.AlternativePrintingProp[newPath], obj, propertyInfo, nestingLevel);

            if (body.AlternativePrinting.Keys.Contains(propertyInfo.PropertyType))
                return AlternativePrint(body.AlternativePrinting[propertyInfo.PropertyType], obj, propertyInfo, nestingLevel);

            if (typeof(ICollection).IsAssignableFrom(propertyInfo.PropertyType))
                return PrintCollection(propertyInfo.Name, (IEnumerable)obj, nestingLevel);

            var propValue = PrintToString(newPath, propertyInfo.GetValue(obj), nestingLevel + 1);
            return GetPropertyString(nestingLevel, propertyInfo.Name, propValue);
        }

        private string AlternativePrint(Func<object, string> alternativePrint,
            object obj, PropertyInfo propertyInfo, int nestingLevel)
        {
            var propValue = propertyInfo.GetValue(obj);
            var propName = propertyInfo.Name;
            var stringPropValue = alternativePrint(propValue);
            return GetPropertyString(nestingLevel, propName, stringPropValue);
        }

        private string PrintCollection(string name, IEnumerable coll, int nestingLevel)
        {
            var res = new List<string>();
            
            if (coll != null)
            {
                foreach (var e in coll)
                {
                    res.Add(PrintToString("", e, nestingLevel + 2));
                }
            }

            var collectionElements = string.Join($",{newLine}", res);
            var indent = GetIndent(nestingLevel + 1);

            return $"{indent}{name} = {newLine}{indent}[{newLine}{collectionElements}{indent}]";
        }

        private string GetPropertyString(int nestingLevel, string propName, string propValue)
        {
            var identation = GetIndent(nestingLevel + 1);
            return $"{identation}{propName} = {propValue}";
        }

        private string GetIndent(int nestingLevel) => new string('\t', nestingLevel);

        private string PathToProperty<T, P>(Expression<Func<T, P>> expr)
        {
            var result = new Stack<string>();
            MemberExpression memberExpr = (MemberExpression)expr.Body;

            while (memberExpr != null)
            {
                string propertyName = memberExpr.Member.Name;

                result.Push(propertyName);

                memberExpr = memberExpr.Expression as MemberExpression;
            }

            return string.Join(".", result);
        }
    }
}