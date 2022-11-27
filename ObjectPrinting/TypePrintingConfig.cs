using System;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TType> : PrintingConfig<TOwner>
{
    public TypePrintingConfig(PrintingConfig<TOwner> parentConfig) : base(parentConfig)
    {
        if (parentConfig == null) throw new ArgumentNullException(nameof(parentConfig));
    }

    public TypePrintingConfig<TOwner, TType> Serialize(Func<TType, string> func)
    {
        GetRoot().TypeSerializers[typeof(TType)] = d => func((TType)d);
        return this;
    }

    public TypePrintingConfig<TOwner, TType> SerializeAllAssignable(Func<TType, string> func)
    {
        var rootPrintingConfig = GetRoot();
        rootPrintingConfig.AssignableTypeSerializers.AddFirst((typeof(TType), d => func((TType)d)));
        rootPrintingConfig.IgnoredTypesFromAssignableCheck.RemoveWhere(t => t.IsAssignableTo(typeof(TType)));
        return this;
    }

    public TypePrintingConfig<TOwner, TType> Exclude()
    {
        GetRoot().TypeExcluding.Add(typeof(TType));
        return this;
    }
}