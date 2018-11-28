using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly List<Type> excludedTypes = new List<Type>();

        public string PrintToString(TOwner obj)
        {
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
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;

                var typeSerializers = ((IPrintingConfig) this).TypeSerializers;

                if (typeSerializers.TryGetValue(propertyInfo.PropertyType, out var serializer))
                {
                    sb.Append(serializer(propertyInfo.GetValue(obj)));
                }
                else
                {
                    sb.Append(indentation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj),
                                  nestingLevel + 1));
                }
            }
            return sb.ToString();
        }

        internal PrintingConfig<TOwner> ExcludingType<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));

            return this;
        }

        public PropertySerializingConfig<TOwner, TPropType> Serialize<TPropType>()
        {
            return new PropertySerializingConfig<TOwner, TPropType>(this);
        }

        Dictionary<Type, Func<object, string>> IPrintingConfig.TypeSerializers { get; set; } =
            new Dictionary<Type, Func<object, string>>();

        void IPrintingConfig.AddTypeSerializer<TPropertyType>(Func<TPropertyType, string> serializer)
        {
            ((IPrintingConfig) this).TypeSerializers[typeof(TPropertyType)] = property => serializer.Invoke((TPropertyType) property);
        }
    }

    public interface IPrintingConfig
    {
        Dictionary<Type, Func<object, string>> TypeSerializers { get; set; }

        void AddTypeSerializer<TPropertyType>(Func<TPropertyType, string> serializer);
    }
}