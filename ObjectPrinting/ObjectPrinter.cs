using System;
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
        private readonly IPrintingConfig<T> config;

        public ObjectPrinter(IPrintingConfig<T> config)
        {
            this.config = config;
        }

        public string PrintToString(T obj) => Serialize(obj, null, 0, out bool _);

        private string Serialize(object obj, PropertyInfo propertyInfo, int nestingLevel, out bool isCompositeObject)
        {
            isCompositeObject = false;

            if (obj == null) return "null";

            var objectType = obj.GetType();
            if (IsObjectExcluded(objectType, propertyInfo))
                return null;

            if (nestingLevel != 0)
            {
                var transforamtor = GetObjectTransforamtor(objectType, propertyInfo);
                if (transforamtor != null)
                    return transforamtor.Transform(obj);
            }

            if (config.FinalTypes.Contains(objectType))
                return obj.ToString();

            isCompositeObject = true;
            return SerializeCompositeObject(obj, propertyInfo, nestingLevel);
        }


        private string SerializeCompositeObject(object obj, PropertyInfo property, int nestingLevel)
        {
            var sb = new StringBuilder();
            var objectType = obj.GetType();
            sb.AppendLine(property?.Name ?? objectType.Name);
            foreach (var propertyInfo in objectType.GetProperties())
            {
                var serializedProperty = Serialize(
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

        private ITransformator GetObjectTransforamtor(Type objectType, PropertyInfo propertyInfo)
        {
            if (config.PropertyTransformators.ContainsKey(propertyInfo))
                return config.PropertyTransformators[propertyInfo];

            if (config.TypeTransformators.ContainsKey(objectType))
                return config.TypeTransformators[objectType];

            return null;
        }

        private bool IsObjectExcluded(Type objectType, PropertyInfo propertyInfo) 
            => config.ExcludedTypes.Contains(objectType) || config.ExcludedProperties.Contains(propertyInfo);
    }
}