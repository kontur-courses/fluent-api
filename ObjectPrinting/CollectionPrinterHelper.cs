using System;
using System.Collections;
using System.Text;

namespace ObjectPrinting
{
    public class CollectionPrinterHelper : IPrinterHelper<IEnumerable>
    {
        public string Print<TOwner>(PrintingConfig<TOwner> printer, IEnumerable collection, int nestingLevel)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);
            var bracketIdentation = new string('\t', nestingLevel);
            sb.AppendLine(Environment.NewLine + bracketIdentation + "{");
            foreach (var item in collection)
            {
                sb.Append(identation + printer.PrintToString(item, nestingLevel + 1));
            }

            sb.Append(bracketIdentation + "}" + Environment.NewLine);
            return sb.ToString();
        }
    }
}