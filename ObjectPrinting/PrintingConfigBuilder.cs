using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ObjectPrinting.Configuration;

namespace ObjectPrinting
{
    public class PrintingConfigBuilder<TOwner>
    {
        private readonly Type[] finalTypes;

        public PrintingConfigBuilder(Type[] finalTypes)
        {
            this.finalTypes = finalTypes;
        }

        public SelectedPropertyGroup<TOwner, TProperty> Choose<TProperty>()
        {
            var targets = SerializationTarget.EnumerateFrom(typeof(TOwner))
                .Where(t => t.MemberType == typeof(TProperty));
            return new SelectedPropertyGroup<TOwner, TProperty>(targets, this);
        }

        public SelectedProperty<TOwner, TProperty> Choose<TProperty>(Expression<Func<TOwner, TProperty>> selector)
        {
            var memberInfo = (selector.Body as MemberExpression)?.Member;

            var target = memberInfo switch
            {
                PropertyInfo propertyInfo => new SerializationTarget(propertyInfo),
                FieldInfo fieldInfo => new SerializationTarget(fieldInfo),
                _ => throw new ArgumentException($"{nameof(selector)} must point on a Property")
            };

            return new SelectedProperty<TOwner, TProperty>(target, this);
        }

        public PrintingConfig<TOwner> Build() => new PrintingConfig<TOwner>(finalTypes, null);

        public static PrintingConfigBuilder<TOwner> Default() => new PrintingConfigBuilder<TOwner>(new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        });
    }
}