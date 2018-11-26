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
        
        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            _excludedTypes.Add(typeof(TPropType));
            return this;
        }
        
        public PrintingConfig<TOwner> Exclude(Expression<Func<TOwner, object>> propSelector)
        {
            MemberExpression body = propSelector.Body as MemberExpression;

            if (body == null) {
                UnaryExpression ubody = (UnaryExpression)propSelector.Body;
                body = ubody.Operand as MemberExpression;
            }

            _excludedProps.Add(body.Member.Name);
            return this;
        }
        
        public SerializingConfig<TOwner, TPropType> Serialize<TPropType>()
        {
            return new SerializingConfig<TOwner, TPropType>(this);
        }
        
        public SerializingConfig<TOwner, Expression> Serialize(Expression<Func<TOwner, object>> propSelector)
        {
            return new SerializingConfig<TOwner, Expression>(this);
        }
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            
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
                
                if (_typesToPrint.Contains(propType))
                {
                    var value = propValue == null ? "null" : propValue;
                    sb.AppendLine($"{identation}{propName} = {value}");
                }
                else
                {
                    sb.Append($"{identation}{propName} = {PrintToString(propValue, nestingLevel + 1)}");
                }
            }
            
            return sb.ToString();
        }
    }
}