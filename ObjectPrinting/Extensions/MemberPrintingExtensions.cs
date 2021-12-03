using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting.Extensions
{
    public static class MemberPrintingExtensions
    {
        public static IPrintingConfig<TOwner> Using<TOwner, TMember>(this IMemberPrintingConfig<TOwner, TMember> config,
            CultureInfo culture, string format = null)
            where TMember : IFormattable
        {
            return config.Using(o => o.ToString(format, culture));
        }

        public static IPrintingConfig<TOwner> TrimToLength<TOwner>(
            this IMemberPrintingConfig<TOwner, string> config,
            Expression<Func<TOwner, string>> memberSelector, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "'length' cannot be negative");
            }
            
            return config.Using(memberSelector, m => length > m.Length ? m : m[..length]);
        }
        
        public static IPrintingConfig<TOwner> TrimToLength<TOwner>(
            this IMemberPrintingConfig<TOwner, string> config, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "'length' cannot be negative");
            }
            
            return config.Using(m => length > m.Length ? m : m[..length]);
        }
    }
}