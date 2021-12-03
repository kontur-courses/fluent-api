using System;
using System.Collections;
using System.Collections.Generic;

namespace ObjectPrinting.Solved
{
    public class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }
    }
}