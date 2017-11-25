using System;

namespace ObjectPrinting.Solved
{
    public interface IPrintingConfig<TOwner>
    {
        Action<Type, Func<object, string>> AddTypeSerialisation { get; }
        Action<string, Func<object, string>> AddPropertySerialisation { get; }
    }
}