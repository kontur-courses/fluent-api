using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> typesExcluding = new HashSet<Type>();
        private readonly HashSet<string> propsExcluding = new HashSet<string>();
        private readonly Dictionary<Type, IPropertySerializingConfig<TOwner>> typesSerializers = new Dictionary<Type, IPropertySerializingConfig<TOwner>>();
        private readonly Dictionary<string, IPropertySerializingConfig<TOwner>> propsSerializers = new Dictionary<string, IPropertySerializingConfig<TOwner>>();
        private readonly HashSet<object> printedObjects = new HashSet<object>();

        public string PrintToString(TOwner obj)
        {
            printedObjects.Clear();
            TryPrintToString(obj, 0, out var objString);
            return objString;
        }

        private bool TryPrintToString(object obj, int nestingLevel, out string objString)
        {
            if (obj == null)
            {
                objString = "null" + Environment.NewLine;
                return true;
            }

            if (typesExcluding.Contains(obj.GetType()))
            {
                objString = default;
                return false;
            }

            if (typesSerializers.TryGetValue(obj.GetType(), out var propertySerializingConfig))
            {
                objString = propertySerializingConfig.Serialize(obj);
                return true;
            }

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };
            if (finalTypes.Contains(obj.GetType()))
            {
                objString = obj + Environment.NewLine;
                return true;
            }

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            if (!printedObjects.Contains(obj))
            {
                printedObjects.Add(obj);

                if (obj is ICollection collection)
                {
                    foreach (var item in collection)
                    {
                        if (TryPrintToString(item, nestingLevel + 1, out var itemString))
                            sb.Append($"{identation}Item = {itemString}");
                    }
                }
                else
                {
                    foreach (var propertyInfo in type.GetProperties())
                    {
                        if (obj is TOwner owner)
                        {
                            if (propsExcluding.Contains(propertyInfo.Name))
                                continue;

                            if (propsSerializers.TryGetValue(propertyInfo.Name, out var propSerializer))
                            {
                                sb.Append($"{identation}{propertyInfo.Name} = {propSerializer.Serialize(propertyInfo.GetValue(obj))}");
                                continue;
                            }
                        }

                        if (TryPrintToString(propertyInfo.GetValue(obj), nestingLevel + 1, out var propString))
                            sb.Append($"{identation}{propertyInfo.Name} = {propString}");
                    }
                }
            }

            objString = sb.ToString();
            return true;
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            typesExcluding.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<T>(Expression<Func<TOwner, T>> func)
        {
            if (func.Body is MemberExpression memberAccessOperation)
            {
                var memberInfo = memberAccessOperation.Member;
                propsExcluding.Add(memberInfo.Name);
                return this;
            }
            else
                throw new ArgumentException("Func must be a member access lambda expression.");
        }

        public PropertySerializingConfig<TOwner, T> Serialize<T>()
        {
            var propSerializerConfig = new PropertySerializingConfig<TOwner, T>(this);
            typesSerializers.Add(typeof(T), propSerializerConfig);
            return propSerializerConfig;
        }

        public PropertySerializingConfig<TOwner, T> Serialize<T>(Expression<Func<TOwner, T>> func)
        {
            if (func.Body.NodeType == ExpressionType.MemberAccess)
            {
                var memberAccessOperation = func.Body as MemberExpression;
                var memberInfo = memberAccessOperation.Member;
                var propSerializerConfig = new PropertySerializingConfig<TOwner, T>(this);
                propsSerializers.Add(memberInfo.Name, propSerializerConfig);
                return propSerializerConfig;
            }
            else
                throw new ArgumentException("Func must be a member access lambda expression.");
        }
    }
}