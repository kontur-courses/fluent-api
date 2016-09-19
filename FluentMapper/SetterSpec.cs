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
        private TypeMappingSpec<TTgt, TSrc> _spec;
        private PropertyInfo _targetProperty;

        public SetterSpec(
            TypeMappingSpec<TTgt, TSrc> spec,
            PropertyInfo targetProperty
            )
        {
            _spec = spec;
            _targetProperty = targetProperty;
        }

        public TypeMappingSpec<TTgt, TSrc> From<TSrcProp>(Expression<Func<TSrc, TSrcProp>> propertyExpression)
        {
            var srcParam = Expression.Parameter(typeof(TSrc));
            var tgtParam = Expression.Parameter(typeof(TTgt));
            var srcExpr = Expression.Property(srcParam, ((MemberExpression)propertyExpression.Body).Member as PropertyInfo);
            var tgtSetterInfo = _targetProperty.GetSetMethod();
            var tgtSetterExpr = Expression.Call(tgtParam, tgtSetterInfo, srcExpr);

            var setterAction = Expression.Lambda<Action<TTgt, TSrc>>(tgtSetterExpr, tgtParam, srcParam)
                .Compile();

            var specProperties = _spec.Properties();

            return _spec
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
                        .Where(x => x != _targetProperty)
                        .ToArray()
                )
                ;
        }

        TypeMappingSpec<TTgt, TSrc> ISetterSpecProperties<TTgt, TSrc>.Spec => _spec;

        PropertyInfo ISetterSpecProperties<TTgt, TSrc>.TargetProperty => _targetProperty;
    }
}