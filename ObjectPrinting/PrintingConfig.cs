using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly List<Type> excludedTypes = new List<Type>();
        private readonly List<string> excludedProperties = new List<string>();

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel, int maxRecursionDepth = 2)
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

            var collectionBuilder = new StringBuilder();
            collectionBuilder.AppendLine(obj.GetType().Name);
            if (obj is ICollection collection)
            {
                collectionBuilder.Append(indentation + "[\r\n");
                foreach (var item in collection)
                    collectionBuilder.Append(indentation + PrintToString(item, nestingLevel) + "\r\n");
                collectionBuilder.Append(indentation + "]\r\n");

                return collectionBuilder.ToString();
            }

            var propertySerializers = ((IPrintingConfig) this).PropertySerializers;
            var typeSerializers = ((IPrintingConfig) this).TypeSerializers;
            var culturallySpecificSerializers = ((IPrintingConfig) this).CulturallySpecificSerializers;
            var trimmingSerializers = ((IPrintingConfig) this).TrimmingSerializers;

            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyName = propertyInfo.Name;
                var propertyType = propertyInfo.PropertyType;

                if (excludedProperties.Contains(propertyName))
                    continue;

                if (excludedTypes.Contains(propertyType))
                    continue;

                string serialized;
                if (propertySerializers.TryGetValue(propertyName, out var serializer)
                    || typeSerializers.TryGetValue(propertyType, out serializer)
                    || culturallySpecificSerializers.TryGetValue(propertyType, out serializer)
                    || trimmingSerializers.TryGetValue(propertyType, out serializer))
                {
                    serialized = serializer(propertyInfo.GetValue(obj));
                }
                else
                {

                    if (nestingLevel == maxRecursionDepth)
                        return "null" + Environment.NewLine;

                    serialized =
                        $"{indentation}{propertyName} = {PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1, maxRecursionDepth)}";
                }

                sb.Append(serialized);
            }

            return sb.ToString();
        }

        internal PrintingConfig<TOwner> ExcludingType<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));

            return this;
        }

        public PropertySerializingConfigByType<TOwner, TPropertyTypeType> Serialize<TPropertyTypeType>()
        {
            return new PropertySerializingConfigByType<TOwner, TPropertyTypeType>(this);
        }

        Dictionary<Type, Func<object, string>> IPrintingConfig.TypeSerializers { get; set; } =
            new Dictionary<Type, Func<object, string>>();

        void IPrintingConfig.AddTypeSerializer<TPropertyType>(Func<TPropertyType, string> serializer)
        {
            ((IPrintingConfig) this).TypeSerializers[typeof(TPropertyType)] =
                property => serializer.Invoke((TPropertyType) property);
        }

        Dictionary<Type, Func<object, string>> IPrintingConfig.CulturallySpecificSerializers { get; set; } =
            new Dictionary<Type, Func<object, string>>();

        void IPrintingConfig.AddCulturallySpecificSerializer<TPropertyType>(Func<TPropertyType, string> serializer)
        {
            ((IPrintingConfig) this).CulturallySpecificSerializers[typeof(TPropertyType)] =
                property => serializer.Invoke((TPropertyType) property);
        }

        public PropertySerializingConfigByName<TOwner, TPropertyType> Serialize<TPropertyType>(
            Expression<Func<TOwner, TPropertyType>> memberSelector)
        {
            var propertyName = ((MemberExpression) memberSelector.Body).Member.Name;

            return new PropertySerializingConfigByName<TOwner, TPropertyType>(this, propertyName);
        }

        Dictionary<string, Func<object, string>> IPrintingConfig.PropertySerializers { get; set; } =
            new Dictionary<string, Func<object, string>>();

        void IPrintingConfig.AddNameSerializer<TPropertyType>(string propertyName,
            Func<TPropertyType, string> serializer)
        {
            ((IPrintingConfig) this).PropertySerializers[propertyName] =
                property => serializer.Invoke((TPropertyType) property);
        }

        Dictionary<Type, Func<object, string>> IPrintingConfig.TrimmingSerializers { get; set; } =
            new Dictionary<Type, Func<object, string>>();

        void IPrintingConfig.AddTrimmingSerializer<TPropertyType>(Func<TPropertyType, string> serializer)
        {
            ((IPrintingConfig) this).TrimmingSerializers[typeof(TPropertyType)] =
                property => serializer.Invoke((TPropertyType) property);
        }

        public PrintingConfig<TOwner> ExcludingProperty<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyName = ((MemberExpression) memberSelector.Body).Member.Name;
            excludedProperties.Add(propertyName);

            return this;
        }
    }

    public interface IPrintingConfig
    {
        Dictionary<Type, Func<object, string>> TypeSerializers { get; set; }

        void AddTypeSerializer<TPropertyType>(Func<TPropertyType, string> serializer);

        Dictionary<Type, Func<object, string>> CulturallySpecificSerializers { get; set; }

        void AddCulturallySpecificSerializer<TPropertyType>(Func<TPropertyType, string> serializer);

        Dictionary<string, Func<object, string>> PropertySerializers { get; set; }

        void AddNameSerializer<TPropertyType>(string propertyName, Func<TPropertyType, string> serializer);

        Dictionary<Type, Func<object, string>> TrimmingSerializers { get; set; }

        void AddTrimmingSerializer<TPropertyType>(Func<TPropertyType, string> serializer);
    }
}