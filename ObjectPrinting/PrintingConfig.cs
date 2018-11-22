using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace ObjectPrinting
{
    public class SerializingConfig<TOwner, TPropType> : ISerializingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> _baseClass;
        
        public SerializingConfig(PrintingConfig<TOwner> baseClass)
        {
            _baseClass = baseClass;
        }
        
        public PrintingConfig<TOwner> Using(Func<TPropType, string> param)
        {
            return _baseClass;
        }

        public PrintingConfig<TOwner> Exclude()
        {
            return _baseClass;
        }

        PrintingConfig<TOwner> ISerializingConfig<TOwner>.SerializingConfig => _baseClass;
    }

    interface ISerializingConfig<TOwner>
    {
        PrintingConfig<TOwner> SerializingConfig { get; }
    }

    public static class PrintingUsingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this SerializingConfig<TOwner, int> config, CultureInfo ci)
        {
            return ((ISerializingConfig<TOwner>) config).SerializingConfig;
        }

        public static PrintingConfig<TOwner> Trim<TOwner>(this SerializingConfig<TOwner, string> config)
        {
            return ((ISerializingConfig<TOwner>) config).SerializingConfig;
        }
    }
    
    public class PrintingConfig<TOwner>
    {
        public PrintingConfig<TOwner> Exclude<TPorpType>()
        {
            return this;
        }
        
        public SerializingConfig<TOwner, TPropType> Serialize<TPropType>()
        {
            return new SerializingConfig<TOwner, TPropType>(this);
        }
        
        public SerializingConfig<TOwner, Expression> Serialize(Expression<Func<TOwner, object>> propSelector)
        {
            return new SerializingConfig<TOwner, Expression>(this);
        }
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}