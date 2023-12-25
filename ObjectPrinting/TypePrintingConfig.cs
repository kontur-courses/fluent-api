using System;
using System.Globalization;
using System.Linq;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, T> : PrintingConfig<TOwner>
    {
        private readonly Type[] numericTypes =
        {
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
            typeof(ulong), typeof(float), typeof(double), typeof(decimal)
        };
        
        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig) : base(printingConfig) { }

        public TypePrintingConfig<TOwner, T> SpecificSerialization(Func<T, string> serializer)
        {
            SetSerializer(typeof(T), obj => serializer((T)obj));
            
            return this;
        }

        public TypePrintingConfig<TOwner, T> NumberCulture(CultureInfo cultureInfo)
        {
            if (numericTypes.Contains(typeof(T)))
                CultureInfos.Add(typeof(T), cultureInfo);

            return this;
        }

        public TypePrintingConfig<TOwner, T> TrimString(int length)
        {
            MaxLength = length;
            return this;
        }
    }
}