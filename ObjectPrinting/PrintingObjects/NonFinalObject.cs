using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class NonFinalObject<T> : PrintingObject<T>
    {
        public NonFinalObject(object obj, IPrintingConfig<T> config) : base(obj, config) { }

        public override string Print(int nestingLevel)
        {
            if (nestingLevel == ObjectPrinter.MaxDepthSerialize) return ObjectPrinter.MaxDepthSerializeString;
            
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(ObjectForPrint.GetType().Name);

            foreach (var propertyInfo in ObjectForPrint.GetType().GetProperties())
            {
                if (PrintingConfig.ExcludingTypes.Contains(propertyInfo.PropertyType)
                    || PrintingConfig.ExcludingProperties.Contains(propertyInfo.Name))
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
            if (PrintingConfig.PropertySerializerConfigs.ContainsKey(info.Name))
                return PrintingConfig.Print(
                    PrintingConfig.PropertySerializerConfigs[info.Name].SerializeFunc(info.GetValue(obj)), 
                    nestingLevel);
            return info.Name 
                   + " = " 
                   + PrintingConfig.Print(info.GetValue(obj), nestingLevel + 1);
        }
    }
}