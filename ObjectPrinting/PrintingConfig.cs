using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public readonly Dictionary<Type, Func<object, string>> SerializationsByType = [];
        public readonly Dictionary<string, Func<object, string>> SerializationsByPropertyFullName = [];
        private readonly HashSet<string> excludedProperties = [];
        private readonly HashSet<Type> excludedTypes = [];

        private readonly Stack<string> currentPropertyFullName = new();
        
        public string PrintToString(TOwner obj)
        {
            currentPropertyFullName.Clear();
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            
            var propertyFullName = string.Join(".", currentPropertyFullName.Reverse());
            if (SerializationsByPropertyFullName.TryGetValue(propertyFullName, out var value))
                return value(obj) + Environment.NewLine;
            
            if (SerializationsByType.ContainsKey(obj.GetType()))
                return SerializationsByType[obj.GetType()](obj) + Environment.NewLine;
            
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;
            
            if (obj is System.Collections.IEnumerable and not string)
            {
                return ProcessIEnumerable(obj, nestingLevel);
            }

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                currentPropertyFullName.Push(propertyInfo.Name);
                var newPropertyFullName = string.Join(".", currentPropertyFullName.Reverse());
                
                if (excludedProperties.Contains(newPropertyFullName))
                {
                    currentPropertyFullName.Pop();
                    continue;
                }
                
                if (excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;
                
                sb.Append(indentation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
                
                currentPropertyFullName.Pop();
            }
            return sb.ToString();
        }

        private string ProcessIEnumerable(object obj, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(obj.GetType().Name);
                
            var enumerable = (System.Collections.IEnumerable)obj;
            var index = 0;
                
            foreach (var item in enumerable)
            {
                currentPropertyFullName.Push($"[{index}]");
                sb.Append(indentation + $"[{index}] = " + 
                          PrintToString(item, nestingLevel + 1));
                currentPropertyFullName.Pop();
                index++;
            }
                
            return sb.ToString();
        }


        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }
        
        public PrintingConfig<TOwner> Exclude(Expression<Func<TOwner, object>> func)
        {
            var fullPropertyName = GetPropertyFullName(func.Body);
            excludedProperties.Add(fullPropertyName);
            return this;
        }

        public PrintingConfig<TOwner> Serialize<T>(Func<T, string> func)
        {
            SerializationsByType[typeof(T)] = obj => func((T)obj);
            return this;
        }

        public ObjectPrinterUsingProperties<TOwner, T> Using<T>()
        {
            return new ObjectPrinterUsingProperties<TOwner, T>(this);
        }
        
        public ObjectPrinterUsingProperty<TOwner, T> Using<T>(Expression<Func<TOwner, T>> func)
        {
            return new ObjectPrinterUsingProperty<TOwner, T>(this, GetPropertyFullName(func.Body));
        }
        
        private string GetPropertyFullName(Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                var parent = GetPropertyFullName(memberExpression.Expression!);
                return string.IsNullOrEmpty(parent) ? memberExpression.Member.Name : parent + "." + memberExpression.Member.Name;
            }
            
            if (expression is UnaryExpression unaryExpression)
            {
                if (unaryExpression.NodeType == ExpressionType.Convert)
                {
                    return GetPropertyFullName(unaryExpression.Operand);
                }
                throw new InvalidOperationException("Unsupported expression type");
            }
            
            if (expression is ParameterExpression)
            {
                return string.Empty;
            }
            throw new InvalidOperationException("Unsupported expression type");
        }
    }
}