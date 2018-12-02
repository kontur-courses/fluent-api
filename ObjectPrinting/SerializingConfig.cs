using System;
using System.CodeDom;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class SerializingConfig<TOwner, TPropType> : ISerializingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly SerializingConfigContext ctx;
        private readonly string propName;
        
        PrintingConfig<TOwner> ISerializingConfig<TOwner>.ParentConfig => parentConfig;
        SerializingConfigContext ISerializingConfig<TOwner>.Context => ctx;
        
        public SerializingConfig(PrintingConfig<TOwner> parentConfig, SerializingConfigContext ctx)
        {
            this.parentConfig = parentConfig;
            this.ctx = ctx;
        }
        
        public SerializingConfig(PrintingConfig<TOwner> parentConfig, SerializingConfigContext ctx, string propName)
        {
            this.parentConfig = parentConfig;
            this.ctx = ctx;
            this.propName = propName;
        }
        
        public PrintingConfig<TOwner> Using(Func<object, string> serializeFn)
        {
            if (propName != null)
            {
                ctx.PropSerializers.Add(propName, serializeFn);
            }
            else
            {
                ctx.TypeSerializers.Add(typeof(TPropType), serializeFn);
            }
            
            return parentConfig;
        }
        
        public PrintingConfig<TOwner> Using(CultureInfo cultureInfo)
        {
            Func<object, string> cultureInfoSerializer = val => String.Format(cultureInfo, "{0}", val);
            Using(cultureInfoSerializer);
            return parentConfig;
        }
    }
}