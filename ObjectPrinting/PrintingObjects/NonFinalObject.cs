using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class NonFinalObject<T> : PrintingObject<T>
    {
        public NonFinalObject(object obj, PrintingConfig<T> config) : base(obj, config)
        {
        }

        public override string Print(int nestingLevel)
        {
            if (nestingLevel == ObjectPrinter.MaxDepthSerialize) return ObjectPrinter.MaxDepthSerializeString;
            
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(ObjectForPrint.GetType().Name);

            foreach (var propertyInfo in ObjectForPrint.GetType().GetProperties())
            {
                if ((PrintingConfig as IPrintingConfig<T>).ExcludingTypes.Contains(propertyInfo.PropertyType)
                    || (PrintingConfig as IPrintingConfig<T>).ExcludingProperties.Contains(propertyInfo.Name))
                {
                    continue;
                } 
                    
                sb.Append(indentation
                          + PrintProperty(propertyInfo, ObjectForPrint, nestingLevel));
            }

            return sb.ToString();
        }
        
        private string PrintProperty(PropertyInfo info, object obj, int nestingLevel)
        {
            if ((PrintingConfig as IPrintingConfig<T>).PropertySerializerConfigs.ContainsKey(info.Name))
                return (PrintingConfig as IPrintingConfig<T>).Print(
                    (PrintingConfig as IPrintingConfig<T>).PropertySerializerConfigs[info.Name].SerializeFunc(info.GetValue(obj)), 
                    nestingLevel);
            return info.Name 
                   + " = " 
                   + (PrintingConfig as IPrintingConfig<T>).Print(info.GetValue(obj), nestingLevel + 1);
        }
    }
}