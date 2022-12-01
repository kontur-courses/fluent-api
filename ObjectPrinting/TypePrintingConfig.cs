using System;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TType> : PrintingConfig<TOwner>
{
    public TypePrintingConfig(PrintingConfig<TOwner> parentConfig) : base(parentConfig)
    {
        if (parentConfig == null) throw new ArgumentNullException(nameof(parentConfig));
    }

    public TypePrintingConfig<TOwner, TType> Using(Func<TType, string> func)
    {
        ((IInternalPrintingConfig<TOwner>)this).GetRoot().TypeSerializers[typeof(TType)] = d => func((TType)d);
        return this;
    }

    public TypePrintingConfig<TOwner, TType> UsingAllAssignable(Func<TType, string> func)
    {
        var rootPrintingConfig = ((IInternalPrintingConfig<TOwner>)this).GetRoot();
        rootPrintingConfig.AssignableTypeSerializers.AddFirst((typeof(TType), d => func((TType)d)));
        rootPrintingConfig.IgnoredTypesFromAssignableCheck.RemoveWhere(t => t.IsAssignableTo(typeof(TType)));
        return this;
    }
}