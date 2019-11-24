using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting
{
    class EnumerablePrinter
    {
        private readonly Config config;

        public EnumerablePrinter(Config config) => this.config = config;

        public string PrintToString(object obj, int nestingLvl, int depthCount, HashSet<object> usedObjs)
        {
            var enumerable = (IEnumerable)obj;
            var identation = new string('\t', nestingLvl);
            var sb = new StringBuilder();
            sb.AppendLine(obj.GetType().Name);
            foreach (var el in enumerable)
                sb.Append(identation + new Printer(config).PrintToString(el, nestingLvl, depthCount, usedObjs));
            sb.AppendLine();
            return sb.ToString();
        }
    }
}