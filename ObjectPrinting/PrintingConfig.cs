using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<Type> excludedTypes;
        private readonly Dictionary<Type, Delegate> typesSerializer;
        private readonly Dictionary<PropertyInfo, CultureInfo> numbersCulture;
        private PropertyInfo selectedProperty;
        private readonly List<PropertyInfo> exludedFields;
        private readonly Dictionary<PropertyInfo, Delegate> fieldSerializers;

        public  PrintingConfig()
        {
            excludedTypes = new List<Type>();
            typesSerializer = new Dictionary<Type, Delegate>();
            numbersCulture = new Dictionary<PropertyInfo, CultureInfo>();
            exludedFields = new List<PropertyInfo>();
            fieldSerializers = new Dictionary<PropertyInfo, Delegate>();
        }
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> Choose<TProperty>()
        {
            
            return this;
        }
        
        public PrintingConfig<TOwner> TypeSerializer<TProperty>(Func<object, string> func)
        {
            typesSerializer.Add(typeof(TProperty), func);
            return this;
        }
        
        public PrintingConfig<TOwner> Exclude<TProperty>()
        {
            excludedTypes.Add(typeof(TProperty));
            return this;
        }
        public PrintingConfig<TOwner> Exclude()
        {
            exludedFields.Add(selectedProperty);
            selectedProperty = null;
            return this;
        }

        public PrintingConfig<TOwner> Choose<TProperty>(
            Expression<Func<TOwner, TProperty>> selector)
        {
            selectedProperty = (PropertyInfo)((MemberExpression) selector.Body).Member;
            return this;
        }

        public PrintingConfig<TOwner> Choose(
            Expression<Func<TOwner, string>> selector)
        {
            selectedProperty = (PropertyInfo)((MemberExpression) selector.Body).Member;
            return this;
        }
        public PrintingConfig<TOwner> Choose(
            Expression<Func<TOwner, int>> selector)
        {
            selectedProperty = (PropertyInfo)((MemberExpression) selector.Body).Member;
            return this;
        }
        
        public PrintingConfig<TOwner> Choose(
            Expression<Func<TOwner, double>> selector)
        {
            selectedProperty = (PropertyInfo) ((MemberExpression) selector.Body).Member;
            return this;
        }
        
        public PrintingConfig<TOwner> Choose(
            Expression<Func<TOwner, float>> selector)
        {
            selectedProperty = (PropertyInfo)((MemberExpression) selector.Body).Member;
            return this;
        }
        
        public PrintingConfig<TOwner> Choose(
            Expression<Func<TOwner, decimal>> selector)
        {
            selectedProperty = (PropertyInfo)((MemberExpression) selector.Body).Member;
            return this;
        }
        
        public PrintingConfig<TOwner> UseSerializer(Func<object, string> func)
        {
            fieldSerializers[selectedProperty] = func;
            return this;
        }

        public PrintingConfig<TOwner> SetCulture(CultureInfo currentCulture)
        {
            numbersCulture[selectedProperty] = currentCulture;
            return this;
        }

        public PrintingConfig<TOwner> Trim(int i)
        {
            throw new NotImplementedException();
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if(excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;
                if(exludedFields.Contains(propertyInfo))
                    continue;
                
                var current = propertyInfo.GetValue(obj).ToString();
                if (numbersCulture.ContainsKey(propertyInfo))
                    current = string.Format(numbersCulture[propertyInfo], current);
                
                if (typesSerializer.ContainsKey(propertyInfo.PropertyType))
                {
                    var valueToString = typesSerializer[propertyInfo.PropertyType]
                        .DynamicInvoke(current)
                        ?.ToString();
                    sb.Append(indentation + propertyInfo.Name + " = " + valueToString);
                }

                if (fieldSerializers.ContainsKey(propertyInfo))
                {
                    var textToAdd = fieldSerializers[propertyInfo]
                        .DynamicInvoke(current)
                        ?.ToString();
                    sb.Append(indentation + propertyInfo.Name + " = " +
                              textToAdd);
                }
                else
                    sb.Append(indentation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj),
                                  nestingLevel + 1));
                        
                
            }
            return sb.ToString();
        }
    }
}