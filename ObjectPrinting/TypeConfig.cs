using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class TypeConfig<TOwner>
    {
        public Type type { get; set; }

        public PrintingConfig<TOwner> father { get; set; }

        public Func<object, string> func { get; set; }

        public PrintingConfig<TOwner> Using(Func<object, string> func)
        {
            this.func = func;
            return father;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            return father;
        }

        public TypeConfig(PrintingConfig<TOwner> father, Type type)
        {
            this.father = father;
            this.type = type;
        }
    }
}
