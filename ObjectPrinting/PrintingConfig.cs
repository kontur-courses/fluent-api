using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using ObjectPrinting;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public string lastProperty;
        private HashSet<Type> excludedTypes = new HashSet<Type>();
        private Dictionary<Type, Delegate> typeFuncs = new Dictionary<Type, Delegate>();
        private Dictionary<Type, CultureInfo> cultureInfos = new Dictionary<Type, CultureInfo>();
        private Dictionary<string, Delegate> propertyFuncs = new Dictionary<string, Delegate>();
        private HashSet<string> excludedProperties = new HashSet<string>();
        
        
        public void SetFuncFor(Type type, Delegate pr)
        {
            typeFuncs[type] = pr;
        }
        
        public void SetCultureInfoFor(Type type, CultureInfo ci)
        {
            cultureInfos[type] = ci;
        }


        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;
            if (excludedTypes.Contains(obj.GetType()))
                return string.Empty;
            
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj.ToString() + Environment.NewLine;
            
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                string str = "";
                if (typeFuncs.ContainsKey(propertyInfo.PropertyType))
                {
                    str = typeFuncs[propertyInfo.PropertyType].DynamicInvoke(propertyInfo.GetValue(obj)) + Environment.NewLine;
                }
                if (propertyFuncs.ContainsKey(propertyInfo.Name))
                {
                    var ci = propertyFuncs[propertyInfo.Name];
                    str = ci.DynamicInvoke(propertyInfo.GetValue(obj)).ToString() + Environment.NewLine;
                }
                if (cultureInfos.ContainsKey(propertyInfo.PropertyType))
                {
                    var ci = cultureInfos[propertyInfo.PropertyType];
                    str = ((IFormattable)propertyInfo.GetValue(obj)).ToString( null, ci) + Environment.NewLine;
                }

                if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(propertyInfo.Name))
                {
                    str = string.Empty;
                    sb.Append(str);
                    continue;
                }

                if (str == "")
                    str = PrintToString(propertyInfo.GetValue(obj),
                        nestingLevel + 1);
                sb.Append(identation + propertyInfo.Name + " = " +
                          str);
            }
            return sb.ToString();
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var expression = (MemberExpression)memberSelector.Body;
            string name = expression.Member.Name;
            this.lastProperty = name;
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public void SetFuncForProperty<TPropType>(string name, Func<TPropType, string> compile)
        {
            propertyFuncs[name] = compile;
            this.lastProperty = null;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var expression = (MemberExpression)memberSelector.Body;
            string name = expression.Member.Name;
            excludedProperties.Add(name);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }
    }
}