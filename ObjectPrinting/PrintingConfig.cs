using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly PrintingConfigData _data = new();

        public string PrintToString(TOwner obj)
        {
            var serializer = new ObjectSerializer(_data);
            return serializer.Serialize(obj);
        }

        public PrintingConfig<TOwner> ExcludeType<T>()
        {
            _data.ExcludedTypes.Add(typeof(T));
            return this;
        }

        public TypeConfigurator<TOwner, T> ConfigureType<T>()
        {
            return new TypeConfigurator<TOwner, T>(this, _data);
        }

        public PropertyConfigurator<TOwner, TProp> ConfigureProperty<TProp>(
            Expression<Func<TOwner, TProp>> propertySelector)
        {
            var memberExpression = propertySelector.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Expression should be a property selector");

            var propertyName = memberExpression.Member.Name;
            return new PropertyConfigurator<TOwner, TProp>(this, _data, propertyName);
        }

        public CollectionsConfigurator<TOwner, TCollection> ConfigureCollection<TCollection>()
        {
            return new CollectionsConfigurator<TOwner, TCollection>(this, _data, typeof(TCollection));
        }

        public PrintingConfig<TOwner> ExcludeProperty<TProp>(Expression<Func<TOwner, TProp>> propertySelector)
        {
            var memberExpression = propertySelector.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Expression should be a property selector");

            _data.ExcludedProperties.Add(memberExpression.Member.Name);
            return this;
        }
    }
}