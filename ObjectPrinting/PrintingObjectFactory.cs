using System.Collections;
using System.Linq;

namespace ObjectPrinting
{
    public static class PrintingObjectFactory<TOwner>
    {
        public static PrintingObject<TOwner> MakePrintingObject(object obj, IPrintingConfig<TOwner> config)
        {
            if (config.ExcludingTypes.Contains(obj.GetType()))
                return new ExcludingObject<TOwner>(obj, config);
            
            if (config.TypeSerializerConfigs.ContainsKey(obj.GetType()))
                return new CustomSerializeObject<TOwner>(obj, config);
            
            if (ObjectPrinter.FinalTypes.Contains(obj.GetType()))
                return new FinalObject<TOwner>(obj, config);
            
            if(obj is IEnumerable)
                return new EnumerableObject<TOwner>(obj, config);
            
            return new NonFinalObject<TOwner>(obj, config);
        } 
    }
}