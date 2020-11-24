using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Nodes;
using ObjectPrinting.Configuration;

namespace ObjectPrinting
{
    public class PrintingConfigBuilder<TOwner>
    {
        private readonly Type[] finalTypes;
        private readonly RootNode<IPropertyConfigurator> configurationRoot;
        private readonly IDictionary<Type, IPropertyConfigurator> groupAppliedConfigurators;

        public PrintingConfigBuilder(Type[] finalTypes)
        {
            this.finalTypes = finalTypes;
            configurationRoot = new RootNode<IPropertyConfigurator>(nameof(TOwner));
            groupAppliedConfigurators = new Dictionary<Type, IPropertyConfigurator>();
        }

        public SelectedPropertyGroup<TOwner, TProperty> Choose<TProperty>()
        {
            var propertyGroup = new SelectedPropertyGroup<TOwner, TProperty>(this);
            groupAppliedConfigurators.Add(typeof(TProperty), propertyGroup);
            return propertyGroup;
        }

        public SelectedProperty<TOwner, TProperty> Choose<TProperty>(Expression<Func<TOwner, TProperty>> selector)
        {
            var memberInfo = (selector.Body as MemberExpression)?.Member;

            //TODO extract full path from expression (TypedParameterExpression)
            var target = memberInfo switch
            {
                PropertyInfo propertyInfo => new SerializationTarget(propertyInfo),
                FieldInfo fieldInfo => new SerializationTarget(fieldInfo),
                _ => throw new ArgumentException($"{nameof(selector)} must point on a Property")
            };

            var selectedProperty = new SelectedProperty<TOwner, TProperty>(target, this);
            configurationRoot.AddChild(Node.Terminal<IPropertyConfigurator>(target.MemberName, selectedProperty));
            return selectedProperty;
        }

        public PrintingConfig<TOwner> Build() => 
            new PrintingConfig<TOwner>(finalTypes, configurationRoot, groupAppliedConfigurators);

        public static PrintingConfigBuilder<TOwner> Default() => new PrintingConfigBuilder<TOwner>(new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        });
    }
}