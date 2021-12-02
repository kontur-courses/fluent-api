using System;
using System.Collections;
using System.Text;

namespace ObjectPrinting
{
    public class DictionaryPrinterHelper : IPrinterHelper<IDictionary>
    {
        public string Print<TOwner>(PrintingConfig<TOwner> printer, IDictionary dict, int nestingLevel)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);
            var bracketIdentation = new string('\t', nestingLevel);
            sb.AppendLine(Environment.NewLine + bracketIdentation + "{");
            foreach (var key in dict.Keys)
            {
                sb.Append(identation + "key:" + printer.PrintToString(key, nestingLevel + 1));
                sb.AppendLine(identation + "value:" + printer.PrintToString(dict[key], nestingLevel));
            }

            sb.AppendLine(bracketIdentation + "}");
            return sb.ToString();
        }
    }
}