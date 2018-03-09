using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class ObjectPrinter
	{
	    public static PrintingConfig<T> For<T>()
	    {
            return new PrintingConfig<T>();
        }
	}

    public class ObjectPrinter<T>
    {
        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(string), typeof(decimal), typeof(DateTime),
            typeof(TimeSpan), typeof(Guid)
        };

        private readonly IPrintingConfig config;

        public ObjectPrinter(IPrintingConfig config) => this.config = config;

        public string PrintToString(T obj) => Serialize(obj);

        private string Serialize(object obj)
        {
            if (obj == null) return "null";

            var objectType = obj.GetType();
            if (IsFinalType(objectType))
                return obj.ToString();

            return SerializeCompositeObject(obj, objectType.Name, 0);
        }

        private string SerializeProperty(object propertyValue, PropertyInfo propertyInfo, int nestingLevel, out bool isCompositeProperty)
        {
            isCompositeProperty = false;

            if (IsPropertyExcluded(propertyInfo))
                return null;

            if (propertyValue == null) return "null";

            if (TryGetPropertyTransforamtor(propertyInfo, out var transforamtor))
                return transforamtor.Transform(propertyValue);

            if (IsFinalType(propertyInfo.PropertyType))
                return propertyValue.ToString();

            isCompositeProperty = true;
            return SerializeCompositeObject(propertyValue, propertyInfo.Name, nestingLevel);
        }

        private string SerializeCompositeObject(object obj, string objectName, int nestingLevel)
        {
            var sb = new StringBuilder();
            sb.AppendLine(objectName);
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                var serializedProperty = SerializeProperty(
                    propertyInfo.GetValue(obj), propertyInfo, nestingLevel + 1, out var isCompositeProperty);

                if (serializedProperty != null)
                {
                    var identation = new string('\t', nestingLevel + 1);
                    var prefix = isCompositeProperty ? "" : propertyInfo.Name + " = ";
                    sb.AppendLine($"{identation}{prefix}{serializedProperty}");
                }
            }
            return sb.ToString();
        }

        private bool TryGetPropertyTransforamtor(PropertyInfo propertyInfo, out ITransformator transformator) => 
            config.PropertyTransformators.TryGetValue(propertyInfo, out transformator) || config.TypeTransformators.TryGetValue(propertyInfo.PropertyType, out transformator);

        private bool IsPropertyExcluded(PropertyInfo propertyInfo) 
            => config.ExcludedTypes.Contains(propertyInfo.PropertyType) || config.ExcludedProperties.Contains(propertyInfo);

        private bool IsFinalType(Type type) => type.IsPrimitive || finalTypes.Contains(type);
    }
}