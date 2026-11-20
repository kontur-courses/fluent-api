using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    internal record struct PrintDecision
    {
        public bool IsExcluded;
        public object? Value;

        public Func<object, string>? Formatter;

        public PrintDecision(bool isExcluded, object? value, Func<object, string>? formatter)
        {
            IsExcluded = isExcluded;
            Value = value;
            Formatter = formatter;
        }
        public static readonly PrintDecision Excluded = new(true, null, null);

    }
}
