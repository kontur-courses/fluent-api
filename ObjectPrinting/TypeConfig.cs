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
        public Type Type { get; }

        public PrintingConfig<TOwner> Father { get; }

        public Func<object, string> Func { get; private set; }

        public PrintingConfig<TOwner> Using(Func<object, string> func)
        {
            this.Func = func;
            return Father;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            return Father;
        }

        public TypeConfig(PrintingConfig<TOwner> father, Type type)
        {
            this.Father = father;
            this.Type = type;
        }
    }
}
