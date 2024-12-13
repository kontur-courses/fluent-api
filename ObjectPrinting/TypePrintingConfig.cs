using System;
using System.Collections.Generic;
using ObjectPrinting.Interfaces;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TType> : IPrintingConfig<TOwner, TType>
{
    private readonly PrintingConfig<TOwner> parent;
    private readonly Dictionary<Type, Func<object, string>> customSerializers;

    public TypePrintingConfig(PrintingConfig<TOwner> parent, 
        Dictionary<Type, Func<object, string>> customSerializers)
    {
        this.parent = parent;
        this.customSerializers = customSerializers;
    }
    
    public PrintingConfig<TOwner> Using(Func<TType, string> print)
    {
        customSerializers[typeof(TType)] = obj => print((TType)obj);
        return parent;
    }
}