using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ObjectPrinting;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public string LastProperty { get; private set;  }
        private HashSet<Type> excludedTypes = new HashSet<Type>();
        private Dictionary<Type, Delegate> typeFuncs = new Dictionary<Type, Delegate>();
        private Dictionary<Type, CultureInfo> cultureInfos = new Dictionary<Type, CultureInfo>();
        private Dictionary<string, Delegate> propertyFuncs = new Dictionary<string, Delegate>();
        private HashSet<string> excludedProperties = new HashSet<string>();
        private Dictionary<string, CultureInfo> propertyCultures = new Dictionary<string, CultureInfo>();
        private Stack<object> nestingStack = new Stack<object>();

        public void SetFuncFor(Type type, Delegate func)
        {
            typeFuncs[type] = func;
        }
        
        public void SetCultureInfoFor(Type type, CultureInfo ci)
        {
            cultureInfos[type] = ci;
        }


        private string PrintToString(object obj, int nestingLevel)
        {
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
                return obj + Environment.NewLine;
            
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            nestingStack.Push(obj);
            if (obj is IEnumerable)
            {
                foreach (var item in (IEnumerable)obj)
                {
                    sb.Append(PrintToString(item, nestingLevel + 1));
                }
                return sb.ToString();
            }
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(propertyInfo.Name))
                    continue;
                string str;
                if (nestingStack.Contains(propertyInfo.GetValue(obj)))
                {
                    sb.Append(identation + propertyInfo.Name + " = " +
                              "circle reference"+Environment.NewLine);
                    continue; 
                }
                

                if (!TryGetString(propertyInfo, out str, obj))
                {
                    str = PrintToString(propertyInfo.GetValue(obj),
                        nestingLevel + 1);
                }
                sb.Append(identation + propertyInfo.Name + " = " +
                          str);
            }
            
            if(nestingStack.Count != 0)
                nestingStack.Pop();
            return sb.ToString();
        }

        private bool TryGetString(PropertyInfo propertyInfo, out string str, object obj)
        {
            str = "";
            if (propertyFuncs.ContainsKey(propertyInfo.Name))
            {
                var pf = propertyFuncs[propertyInfo.Name];
                str = pf.DynamicInvoke(propertyInfo.GetValue(obj)) + Environment.NewLine;
                return true;
            }
            if (typeFuncs.ContainsKey(propertyInfo.PropertyType))
            {
                str = typeFuncs[propertyInfo.PropertyType].DynamicInvoke(propertyInfo.GetValue(obj)) + Environment.NewLine;
                return true;
            }
            if (propertyCultures.ContainsKey(propertyInfo.Name))
            {
                var pc = propertyCultures[propertyInfo.Name];
                str = ((IFormattable)propertyInfo.GetValue(obj)).ToString( null, pc) + Environment.NewLine;
                return true;
            }
            if (cultureInfos.ContainsKey(propertyInfo.PropertyType))
            {
                var ci = cultureInfos[propertyInfo.PropertyType];
                str = ((IFormattable)propertyInfo.GetValue(obj)).ToString( null, ci) + Environment.NewLine;
                return true;
            }

            return false;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var expression = (MemberExpression)memberSelector.Body;
            string name = expression.Member.Name;
            this.LastProperty = name;
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public void SetFuncForProperty<TPropType>(string name, Func<TPropType, string> func)
        {
            propertyFuncs[name] = func;
            LastProperty = null;
        }
        
        public void SetCultureInfoForProperty(string name, CultureInfo ci)
        {
            propertyCultures[name] = ci;
            LastProperty = null;
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