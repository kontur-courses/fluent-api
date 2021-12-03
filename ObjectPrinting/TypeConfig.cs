﻿using System;
using System.Globalization;

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
            Func = (x) => string.Format(culture, "{0}", x);
            return Father;
        }

        public TypeConfig(PrintingConfig<TOwner> father, Type type)
        {
            this.Father = father;
            this.Type = type;
        }
    }
}
