using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly SerializerSettings settings;
        
        public PrintingConfig()
        {
            settings = new SerializerSettings();
        }

        public TypeConfig<TOwner, TPropType> SelectType<TPropType>()
        {
            return new TypeConfig<TOwner, TPropType>(this, settings);
        }

        public MemberConfig<TOwner, TPropType> SelectProperty<TPropType>(Expression<Func<TOwner, TPropType>> memberExpression)
        {
            var member = ((MemberExpression)memberExpression.Body).Member;
            return new MemberConfig<TOwner, TPropType>(this, member, settings);
        }

        public PrintingConfig<TOwner> SetCulture<T>(IFormatProvider culture) where T : IFormattable
        {
            settings.CustomCultures.Add(typeof(T), culture);
            return this;
        }

        public string PrintToString(object obj, int nestingLevel)
        {
            if (obj == string.Empty) return string.Empty;
            var serializer = new Serializer(settings, obj, nestingLevel);
            var objectString = serializer.ToString();

            return objectString;
        }
    }
}