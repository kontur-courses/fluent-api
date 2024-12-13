using FluentMapping.Internal;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentMapping
{
    public interface ISetterSpecProperties<TTgt, TSrc>
    {
        TypeMappingSpec<TTgt, TSrc> Spec { get; }

        PropertyInfo TargetProperty { get; }
    }

    public sealed class SetterSpec<TTgt, TSrc, TProp> : ISetterSpecProperties<TTgt, TSrc>
    {
        private TypeMappingSpec<TTgt, TSrc> spec;
        private PropertyInfo targetProperty;

        public SetterSpec(
            TypeMappingSpec<TTgt, TSrc> spec,
            PropertyInfo targetProperty
            )
        {
            this.spec = spec;
            this.targetProperty = targetProperty;
        }

        public TypeMappingSpec<TTgt, TSrc> From<TSrcProp>(Expression<Func<TSrc, TSrcProp>> propertyExpression)
        {
            var srcParam = Expression.Parameter(typeof(TSrc));
            var tgtParam = Expression.Parameter(typeof(TTgt));
            var srcExpr = Expression.Property(srcParam, ((MemberExpression)propertyExpression.Body).Member as PropertyInfo);
            var tgtSetterInfo = targetProperty.GetSetMethod();
            var tgtSetterExpr = Expression.Call(tgtParam, tgtSetterInfo, srcExpr);

            var setterAction = Expression.Lambda<Action<TTgt, TSrc>>(tgtSetterExpr, tgtParam, srcParam)
                .Compile();

            var specProperties = spec.Properties();

            return spec
                .Transforms().WithMappingActions(
                    specProperties.MappingActions
                        .Concat(new[] { setterAction })
                        .ToArray()
                )
                .Transforms().WithSourceProperties(
                    specProperties.SourceProperties
                        .Where(x => x != srcExpr.Member)
                        .ToArray()
                )
                .Transforms().WithTargetProperties(
                    specProperties.TargetProperties
                        .Where(x => x != targetProperty)
                        .ToArray()
                )
                ;
        }

        TypeMappingSpec<TTgt, TSrc> ISetterSpecProperties<TTgt, TSrc>.Spec => spec;

        PropertyInfo ISetterSpecProperties<TTgt, TSrc>.TargetProperty => targetProperty;
    }
}