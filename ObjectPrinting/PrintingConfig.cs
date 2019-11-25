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
        private  HashSet<Type> excludedTypes = new HashSet<Type>();
        private Dictionary<Type, Delegate> typeFuncs = new Dictionary<Type, Delegate>();

        public void SetFuncFor(Type type, Delegate pr)
        {
            typeFuncs[type] = pr;
        }


        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;
            if (excludedTypes.Count != 0 && excludedTypes.Contains(obj.GetType()))
                return string.Empty + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                string str = "";
                if (typeFuncs.ContainsKey(propertyInfo.PropertyType))
                {
                    str = typeFuncs[propertyInfo.PropertyType].DynamicInvoke(propertyInfo.GetValue(obj)).ToString() + Environment.NewLine;
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
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
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