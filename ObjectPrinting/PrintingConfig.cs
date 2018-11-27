using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private static Type[] _typesToPrint = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private List<Type> _excludedTypes = new List<Type>();
        private List<string> _excludedProps = new List<string>();
        
        private SerializingConfigContext _serializingConfigCtx = new SerializingConfigContext()
        {
            TypeSerializers = new Dictionary<Type, Func<object, string>>(),
            PropSerializers = new Dictionary<string, Func<object, string>>()
        };
        
        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            _excludedTypes.Add(typeof(TPropType));
            return this;
        }
        
        public PrintingConfig<TOwner> Exclude(Expression<Func<TOwner, object>> propSelector)
        {
            var propName = getPropNameByPropSelector(propSelector);
            _excludedProps.Add(propName);
            return this;
        }
        
        public SerializingConfig<TOwner, TPropType> Serialize<TPropType>()
        {
            return new SerializingConfig<TOwner, TPropType>(this, _serializingConfigCtx);
        }
        
        public SerializingConfig<TOwner, Expression> Serialize(Expression<Func<TOwner, object>> propSelector)
        {   
            var propName = getPropNameByPropSelector(propSelector);
            return new SerializingConfig<TOwner, Expression>(this, _serializingConfigCtx, propName);
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
                var isExcluded = _excludedTypes.Contains(propType) || _excludedProps.Contains(propName);

                if (isExcluded)
                {
                    continue;
                }
                
                Func<object, string> propSerializer;
                _serializingConfigCtx.PropSerializers.TryGetValue(propName, out propSerializer);

                Func<object, string> typeSerializer;
                _serializingConfigCtx.TypeSerializers.TryGetValue(propType, out typeSerializer);

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
                
                if (displayPropValue != null)
                {
                    sb.AppendLine($"{identation}{propName} = {displayPropValue}");
                }
                else if (_typesToPrint.Contains(propType))
                {
                    sb.AppendLine($"{identation}{propName} = {propValue ?? "null"}");
                }
                else
                {
                    sb.Append($"{identation}{propName} = {PrintToString(propValue, nestingLevel + 1)}");
                }
            }
            
            return sb.ToString();
        }

        private string getPropNameByPropSelector(Expression<Func<TOwner, object>> propSelector)
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