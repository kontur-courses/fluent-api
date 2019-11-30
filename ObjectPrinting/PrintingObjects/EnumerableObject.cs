using System.Collections;
using System.Text;

namespace ObjectPrinting
{
    public class EnumerableObject<T> : PrintingObject<T>
    {
        public EnumerableObject(object obj, IPrintingConfig<T> config) : base(obj, config) { }

        public override string Print(int nestingLevel)
        {
            if (nestingLevel == ObjectPrinter.MaxDepthSerialize) return ObjectPrinter.MaxDepthSerializeString;
            
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(ObjectForPrint.GetType().Name);
            
            foreach (var element in (IEnumerable) ObjectForPrint)
                sb.Append(indentation + PrintingConfig.Print(element, nestingLevel + 1));

            return sb.ToString();
        }
    }
}