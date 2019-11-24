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
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;
            if (excludedTypes.Contains(obj.GetType()))
                return string.Empty;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }
        
        private  HashSet<Type> excludedTypes = new HashSet<Type>();
        
        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PropertySerilizingConfig<TOwner, T> WithSerilizing<T>()
        {
            // ...
            return new PropertySerilizingConfig<TOwner, T>(this); 
        }
        
        public PropertySerilizingConfig<TOwner, T> WithSerilizing<T>(Expression<Func<TOwner, T>> func)
        {
            // ...
            return new PropertySerilizingConfig<TOwner, T>(this);
        }
        
        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            // ...
            return this;
        }
    }

    public class PropertySerilizingConfig<TOwner, T> : IPropertySerializingConfig<TOwner>
    {
        private PrintingConfig<TOwner> parentConfig;
        
        public PropertySerilizingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }
        
        public PrintingConfig<TOwner> Using<T>(Func<T, string> func)
        {
            // ...
            return parentConfig; 
        }

        PrintingConfig<TOwner> IPropertySerializingConfig<TOwner>.ParentConfig => parentConfig;
    }
}

public static class PropertySerializingConfigExtension 
{
    public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerilizingConfig<TOwner, int> config, CultureInfo cultureInfo)
    {
        // ...
        return (config as IPropertySerializingConfig<TOwner>).ParentConfig; 
    }
    
    public static PrintingConfig<TOwner> Trim<TOwner>(this PropertySerilizingConfig<TOwner, string> config, int index)
    {
        // ...
        return (config as IPropertySerializingConfig<TOwner>).ParentConfig; 
    }
}

public interface IPropertySerializingConfig<TOwner> {
    PrintingConfig<TOwner> ParentConfig { get;  }
}