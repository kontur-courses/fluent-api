using System;

namespace ObjectPrinting
{
    public static class PrintingContextExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TMember>(
            this PrintingConfig<TOwner>.PrintingContext<TMember> context,
            IFormatProvider formatProvider)
            where TMember : IFormattable
        {
            return context.As(f => f.ToString(null, formatProvider));
        }
        
        public static PrintingConfig<TOwner> Using<TOwner, TMember>(
            this PrintingConfig<TOwner>.PrintingContext<TMember> context,
            string format,
            IFormatProvider formatProvider = null)
            where TMember : IFormattable
        {
            return context.As(f => f.ToString(format, formatProvider));
        }
        
        public static PrintingConfig<TOwner> Truncated<TOwner>(
            this PrintingConfig<TOwner>.PrintingContext<string> context,
            int length)
        {
            return context.As(s => s.Length <= length ? s : s.Substring(0, length) + "...");
        }
    }
}