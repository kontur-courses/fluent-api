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
        private readonly List<Type> excludedFields;
        private readonly Dictionary<Type, Delegate> typesSerializer;
        private readonly Dictionary<PropertyInfo, CultureInfo> numbersCulture;
        private PropertyInfo selectedProperty;

        public  PrintingConfig()
        {
            excludedFields = new List<Type>();
            typesSerializer = new Dictionary<Type, Delegate>();
            numbersCulture = new Dictionary<PropertyInfo, CultureInfo>();
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
            excludedFields.Add(typeof(TProperty));
            return this;
        }
        public PrintingConfig<TOwner> Exclude()
        {
            return this;
        }

        public PrintingConfig<TOwner> Choose<TProperty>(
            Expression<Func<TOwner, TProperty>> selector)
        {
            return this;
        }

        public PrintingConfig<TOwner> Choose(
            Expression<Func<TOwner, string>> selector)
        {
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
            return this;
        }
        
        public PrintingConfig<TOwner> Choose(
            Expression<Func<TOwner, float>> selector)
        {
            return this;
        }
        
        public PrintingConfig<TOwner> Choose(
            Expression<Func<TOwner, decimal>> selector)
        {
            return this;
        }
        
        public PrintingConfig<TOwner> UseSerializer(Func<object, string> func)
        {
            throw new NotImplementedException();
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
                if(excludedFields.Contains(propertyInfo.PropertyType))
                    continue;

                var current = propertyInfo.GetValue(obj).ToString();
                if (numbersCulture.ContainsKey(propertyInfo))
                    current = current.ToString(numbersCulture[propertyInfo]);
                
                if (typesSerializer.ContainsKey(propertyInfo.PropertyType))
                {
                    var valueToString = typesSerializer[propertyInfo.PropertyType]
                        .DynamicInvoke(current)
                        ?.ToString();
                    sb.Append(indentation + propertyInfo.Name + " = " + valueToString);
                }
                else
                    sb.Append(indentation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj),
                                  nestingLevel + 1));
                        
                
            }

            return sb.ToString();
        }
    }

    public class SelectedProperty<TOwner, TConfigurator, TProperty>
        where TConfigurator : PropertyConfigurator<TProperty>
    {
        public PrintingConfig<TOwner> Parent { get; }

        public PrintingConfig<TOwner> Using(Action<TConfigurator> configurator)
        {
            return Parent;
        }

        public PrintingConfig<TOwner> Exclude()
        {
            return Parent;
        }

        public PrintingConfig<TOwner> UseSerializer(Func<object, string> func)
        {
            throw new NotImplementedException();
        }

        public PrintingConfig<TOwner> SetCulture(CultureInfo currentCulture)
        {
            throw new NotImplementedException();
        }

        public PrintingConfig<TOwner> Trim(int i)
        {
            throw new NotImplementedException();
        }
    }

    public class PropertyConfigurator<TProperty>
    {
        public void Exclude()
        {
            
        }

        public PropertyConfigurator<TProperty> UseSerializer(Func<TProperty, string> serializer)
        {
            return this;
        }
    }

    public class StringPropertyConfigurator : PropertyConfigurator<string>
    {
        public StringPropertyConfigurator Substring()
        {
            return this;
        }
    }

    public class NumberPropertyConfigurator : PropertyConfigurator<int>
    {
        public NumberPropertyConfigurator SetCulture(CultureInfo cultureInfo)
        {
            return this;
        }
    }
}