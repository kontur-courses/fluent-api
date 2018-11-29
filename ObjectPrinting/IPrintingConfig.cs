using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    internal interface IPrintingConfig
    {
        void AddPropertySerializationFormat(PropertyInfo property, Func<object, string> format);
        void AddTypeSerializationFormat(Type type, Func<object, string> format);
        void AddPostProduction(PropertyInfo property, Func<object, string> format);
        void AddCultureInfo(Type type, CultureInfo cultureInfo);
    }
}
