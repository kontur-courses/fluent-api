using System;
using System.Globalization;

namespace ObjectPrinting.Solved
{
    public interface IPrintingConfig<TOwner>
    {
        Action<Type, Func<object, string>> AddTypeSerialisation { get; }
        Action<string, Func<object, string>> AddPropertySerialisation { get; }
        Action<Type, CultureInfo> AddTypeCulture { get; }
        Action<int> SetAverageStringLength { get; }
        Action<string, int> SetStringPropertyLength { get; }
    }
}