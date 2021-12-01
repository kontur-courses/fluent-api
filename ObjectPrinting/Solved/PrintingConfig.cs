using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner> :IPrintingConfig<TOwner>
    {
        private string propertyKey;
        private List<Type> excludingTypes = new List<Type>();
        private Dictionary<Type, CultureInfo> specialCulture = new Dictionary<Type, CultureInfo>();
        private Dictionary<Type, Func<object,string>> alternativePrinting = new Dictionary<Type, Func<object, string>>();
        private Dictionary<string, Func<object, string>> alternativePrintingProp = new Dictionary<string, Func<object, string>>();
        private List<string> excludingProp = new List<string>();
        private Type[] finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };

        Dictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.AlternativePrinting => alternativePrinting;
        Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.SpecialCulture => specialCulture;

        public void AddFuncForProp(Func<object, string> func)
        {
            alternativePrintingProp[propertyKey] = func;
        }
        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var printingConfig = new TypePrintingConfig<TOwner, TPropType>(this);
            return printingConfig;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var printingConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            propertyKey = PathToProperty(memberSelector);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var res = PathToProperty(memberSelector);
            excludingProp.Add(res);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj, int maxNestingLevel)
        {
            return PrintToString("",obj, 0, maxNestingLevel);
        }

        private string PrintToString(string parentPath, object obj, int nestingLevel, int maxNestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var sb = new StringBuilder();
            var type = obj.GetType();

            sb.AppendLine(type.Name);

            if (nestingLevel > maxNestingLevel)
                return sb.ToString();

            foreach (var propertyInfo in type.GetProperties())
            {
                string newPath = string.IsNullOrEmpty(parentPath) ? $"{propertyInfo.Name}" : $"{parentPath}.{propertyInfo.Name}";

                var propType = propertyInfo.PropertyType;
                if (excludingTypes.Contains(propType) || excludingProp.Contains(newPath))
                    continue;

                var baseCulture = CultureInfo.CurrentCulture.Clone();
                if (specialCulture.ContainsKey(propType))
                    CultureInfo.CurrentCulture = specialCulture[propType];
                if (alternativePrintingProp.Keys.Contains(newPath))
                {
                    var propValue = alternativePrintingProp[newPath](propertyInfo.GetValue(obj));
                    sb.AppendLine(GetPropertyString(nestingLevel, propertyInfo.Name, propValue));
                    CultureInfo.CurrentCulture = (CultureInfo)baseCulture;
                    continue;
                }
                if (alternativePrinting.Keys.Contains(propType))
                {
                    var propValue = alternativePrinting[propType](propertyInfo.GetValue(obj));
                    sb.AppendLine(GetPropertyString(nestingLevel,propertyInfo.Name, propValue));
                    CultureInfo.CurrentCulture = (CultureInfo)baseCulture;
                    continue;
                }
                sb.Append(GetPropertyString(nestingLevel,propertyInfo.Name, PrintToString(newPath, propertyInfo.GetValue(obj),
                            nestingLevel + 1, maxNestingLevel)));
                CultureInfo.CurrentCulture = (CultureInfo)baseCulture;
            }
            
            return sb.ToString();
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