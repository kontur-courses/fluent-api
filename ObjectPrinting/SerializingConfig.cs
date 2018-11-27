using System;
using System.CodeDom;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class SerializingConfig<TOwner, TPropType> : ISerializingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> _parentConfig;
        private readonly SerializingConfigContext _ctx;
        private readonly string _propName;
        
        PrintingConfig<TOwner> ISerializingConfig<TOwner>.ParentConfig => _parentConfig;
        SerializingConfigContext ISerializingConfig<TOwner>.Context => _ctx;
        
        public SerializingConfig(PrintingConfig<TOwner> parentConfig, SerializingConfigContext ctx)
        {
            this._parentConfig = parentConfig;
            this._ctx = ctx;
        }
        
        public SerializingConfig(PrintingConfig<TOwner> parentConfig, SerializingConfigContext ctx, string propName)
        {
            this._parentConfig = parentConfig;
            this._ctx = ctx;
            this._propName = propName;
        }
        
        public PrintingConfig<TOwner> Using(Func<object, string> serializeFn)
        {
            if (_propName != null)
            {
                this._ctx.PropSerializers.Add(_propName, serializeFn);
            }
            else
            {
                this._ctx.TypeSerializers.Add(typeof(TPropType), serializeFn);
            }
            
            return _parentConfig;
        }
        
        public PrintingConfig<TOwner> Using(CultureInfo cultureInfo)
        {
            Func<object, string> cultureInfoSerializer = val => String.Format(cultureInfo, "{0}", val);
            
            if (_propName != null)
            {
                this._ctx.PropSerializers.Add(_propName, cultureInfoSerializer);
            }
            else
            {
                this._ctx.TypeSerializers.Add(typeof(TPropType), cultureInfoSerializer);
            }
            
            return _parentConfig;
        }
    }
}