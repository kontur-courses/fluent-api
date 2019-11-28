using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private static readonly int MaxNestingLevel = 5;
        private readonly SerializationInfo serializationInfo;

        public PrintingConfig()
        {
            serializationInfo = new SerializationInfo();
        }

        SerializationInfo IPrintingConfig.SerializationInfo => serializationInfo;

        public PrintingConfig<TOwner> Excluding<T>()
        {
            serializationInfo.ExcludeType(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> func)
        {
            var propInfo = ((MemberExpression) func.Body).Member as PropertyInfo;
            serializationInfo.ExcludeProperty(propInfo);
            return this;
        }

        public PropertySerializationConfig<TOwner, TPropType> For<TPropType>(
            Expression<Func<TOwner, TPropType>> func)
        {
            var propInfo = ((MemberExpression) func.Body).Member as PropertyInfo;
            return new PropertySerializationConfig<TOwner, TPropType>(this, propInfo);
        }

        public PropertySerializationConfig<TOwner, TPropType> For<TPropType>()
        {
            return new PropertySerializationConfig<TOwner, TPropType>(this);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }


        private string PrintToString(object obj, int nestingLevel)
        {
            if (nestingLevel >= MaxNestingLevel)
                return "Max nestingLevel reached";
            if (obj == null)
                return "null" + Environment.NewLine;

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
            foreach (var propertyInfo in type.GetProperties())
            {
                sb.Append(identation);
                if (!serializationInfo.Excluded(propertyInfo))
                {
                    if (serializationInfo.TryGetSerialization(propertyInfo, out var serializedProperty, obj))
                        sb.Append(serializedProperty);
                    else
                        sb.Append(propertyInfo.Name + " = " +
                                  PrintToString(propertyInfo.GetValue(obj),
                                      nestingLevel + 1));
                }
            }

            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
    }
}