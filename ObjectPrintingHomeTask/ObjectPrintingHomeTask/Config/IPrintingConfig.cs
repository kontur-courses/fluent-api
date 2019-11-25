using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrintingHomeTask.Config
{
    public interface IPrintingConfig
    {
        void AddChangedType(Type type, Delegate del);
        void AddChangedProperty((int deep, PropertyInfo propertyInfo) key, Delegate del);
        void ChangeCulture(CultureInfo culture);
    }
}
