using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private static Type[] typesToPrint = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private int maxDepth;

        private List<Type> excludedTypes = new List<Type>();
        private List<string> excludedProps = new List<string>();
        
        private SerializingConfigContext serializingConfigCtx = new SerializingConfigContext()
        {
            TypeSerializers = new Dictionary<Type, Func<object, string>>(),
            PropSerializers = new Dictionary<string, Func<object, string>>()
        };

        public PrintingConfig(int maxDepth)
        {
            this.maxDepth = maxDepth;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }
        
        public PrintingConfig<TOwner> Exclude(Expression<Func<TOwner, object>> propSelector)
        {
            var propName = GetPropNameByPropSelector(propSelector);
            excludedProps.Add(propName);
            return this;
        }
        
        public SerializingConfig<TOwner, TPropType> Serialize<TPropType>()
        {
            return new SerializingConfig<TOwner, TPropType>(this, serializingConfigCtx);
        }
        
        public SerializingConfig<TOwner, Expression> Serialize(Expression<Func<TOwner, object>> propSelector)
        {   
            var propName = GetPropNameByPropSelector(propSelector);
            return new SerializingConfig<TOwner, Expression>(this, serializingConfigCtx, propName);
        }
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            
            foreach (var propertyInfo in type.GetProperties())
            {
                var propType = propertyInfo.PropertyType;
                var propName = propertyInfo.Name;
                var propValue = propertyInfo.GetValue(obj);
                var isExcluded = excludedTypes.Contains(propType) || excludedProps.Contains(propName);

                if (isExcluded)
                {
                    continue;
                }

                var displayPropValue = GetDisplayPropValue(propName, propType, propValue);
                    
                if (displayPropValue != null)
                {
                    sb.AppendLine($"{identation}{propName} = {displayPropValue}");
                }
                else if (typesToPrint.Contains(propType))
                {
                    sb.AppendLine($"{identation}{propName} = {propValue ?? "null"}");
                }
                else if (nestingLevel < maxDepth)
                {
                    sb.Append($"{identation}{propName} = {PrintToString(propValue, nestingLevel + 1)}");
                }
                else
                {
                    sb.AppendLine($"{identation}{propName} = {propType.Name}");
                }
            }
            
            return sb.ToString();
        }

        private string GetDisplayPropValue(string propName, Type propType, object propValue)
        {
            Func<object, string> propSerializer;
            serializingConfigCtx.PropSerializers.TryGetValue(propName, out propSerializer);

            Func<object, string> typeSerializer;
            serializingConfigCtx.TypeSerializers.TryGetValue(propType, out typeSerializer);

            string displayPropValue = null;
            if (propValue == null)
            {
                displayPropValue = "null";
            }
            else if (propSerializer != null)
            {
                displayPropValue = propSerializer(propValue);
            }
            else if (typeSerializer != null)
            {
                displayPropValue = typeSerializer(propValue);
            }

            return displayPropValue;
        }

        private string GetPropNameByPropSelector(Expression<Func<TOwner, object>> propSelector)
        {
            MemberExpression body = propSelector.Body as MemberExpression;

            if (body == null) {
                var uBody = (UnaryExpression)propSelector.Body;
                body = uBody.Operand as MemberExpression;
            }

            return body.Member.Name;
        }
    }
}