using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentMapping
{
    public sealed class TypeMappingSpec<TTgt, TSrc>
        : ITypeMappingSpecProperties<TTgt, TSrc>,
        ITypeMappingSpecTransforms<TTgt, TSrc>
    {
        private readonly IEnumerable<Action<TTgt, TSrc>> mappingActions;
        private readonly IEnumerable<PropertyInfo> srcProperties;
        private readonly IEnumerable<PropertyInfo> tgtProperties;
        private readonly IAssembler<TTgt, TSrc> assembler;

        public TypeMappingSpec()
            : this(
                  typeof(TTgt).GetProperties(),
                  typeof(TSrc).GetProperties(),
                  new Action<TTgt, TSrc>[0],
                  new DefaultAssembler<TTgt, TSrc>())
        {
        }

        private TypeMappingSpec(
            IEnumerable<PropertyInfo> targetProperties,
            IEnumerable<PropertyInfo> sourceProperties,
            IEnumerable<Action<TTgt, TSrc>> mappingActions,
            IAssembler<TTgt, TSrc> assembler
            )
        {
            this.mappingActions = mappingActions;
            tgtProperties = targetProperties;
            srcProperties = sourceProperties;
            this.assembler = assembler;
        }

        public NullSourceBehavior<TTgt, TSrc> WithNullSource()
        {
            return new NullSourceBehavior<TTgt, TSrc>(this);
        }

        IEnumerable<PropertyInfo> ITypeMappingSpecProperties<TTgt, TSrc>.SourceProperties => srcProperties;
        IEnumerable<PropertyInfo> ITypeMappingSpecProperties<TTgt, TSrc>.TargetProperties => tgtProperties;
        IEnumerable<Action<TTgt, TSrc>> ITypeMappingSpecProperties<TTgt, TSrc>.MappingActions => mappingActions;
        IAssembler<TTgt, TSrc> ITypeMappingSpecProperties<TTgt, TSrc>.Assembler => assembler;

        TypeMappingSpec<TTgt, TSrc> ITypeMappingSpecTransforms<TTgt, TSrc>
            .WithSourceProperties(IEnumerable<PropertyInfo> sourceProperties)
        {
            return new TypeMappingSpec<TTgt, TSrc>(
                tgtProperties,
                sourceProperties,
                mappingActions,
                assembler
                );
        }

        TypeMappingSpec<TTgt, TSrc> ITypeMappingSpecTransforms<TTgt, TSrc>
            .WithTargetProperties(IEnumerable<PropertyInfo> targetProperties)
        {
            return new TypeMappingSpec<TTgt, TSrc>(
                targetProperties,
                srcProperties,
                mappingActions,
                assembler
                );
        }

        TypeMappingSpec<TTgt, TSrc> ITypeMappingSpecTransforms<TTgt, TSrc>
            .WithMappingActions(IEnumerable<Action<TTgt, TSrc>> mappingActions)
        {
            return new TypeMappingSpec<TTgt, TSrc>(
                tgtProperties,
                srcProperties,
                mappingActions,
                assembler
                );
        }

        TypeMappingSpec<TTgt, TSrc> ITypeMappingSpecTransforms<TTgt, TSrc>
            .WithAssembler(IAssembler<TTgt, TSrc> assembler)
        {
            return new TypeMappingSpec<TTgt, TSrc>(
                tgtProperties,
                srcProperties,
                mappingActions,
                assembler
                );
        }

        public IMapper<TTgt, TSrc> Create()
        {
            var unmappedTargets = tgtProperties
                .Where(tgtProp => !srcProperties
                    .Any(srcProp => srcProp.Name == tgtProp.Name)
                    );
            var unmappedSources = srcProperties
                .Where(srcProp => !tgtProperties
                    .Any(tgtProp => tgtProp.Name == srcProp.Name)
                    );

            if (unmappedSources.Any() || unmappedTargets.Any())
            {
                var targetStrings = unmappedTargets
                    .Select(x => typeof(TTgt).Name + "." + x.Name);
                var srcStrings = unmappedSources
                    .Select(x => typeof(TSrc).Name + "." + x.Name);

                var exceptionMessage = "Unmapped properties: "
                    + string.Join(", ", targetStrings.OrderBy(x => x).Concat(srcStrings.OrderBy(x => x)));

                throw new Exception(exceptionMessage);
            }

            var srcParam = Expression.Parameter(typeof(TSrc));
            var tgtParam = Expression.Parameter(typeof(TTgt));

            var actions = new List<Expression>();

            foreach (var targetProperty in tgtProperties)
            {
                var sourceProperty = srcProperties
                    .FirstOrDefault(x => x.Name == targetProperty.Name);

                if (sourceProperty == null)
                    throw new Exception();

                var setter = targetProperty.GetSetMethod();
                var getterExpression = Expression.Property(srcParam, sourceProperty);
                var setterCallExpression = Expression.Call(tgtParam, setter, getterExpression);

                actions.Add(setterCallExpression);
            }

            actions.AddRange(mappingActions
                .Select(ToExpression)
                .Select(x => Expression.Invoke(x, tgtParam, srcParam))
                );

            var setterCallsExpression = Expression.Block(actions.ToArray());

            var compiledAction = Expression.Lambda<Action<TTgt, TSrc>>(
                setterCallsExpression,
                tgtParam,
                srcParam
                )
                .Compile()
                ;

            return new Mapper(compiledAction, assembler);
        }

        public SetterSpec<TTgt, TSrc, TProp> ThatSets<TProp>(
            Expression<Func<TTgt, TProp>> propertyExpression
            )
        {
            var memberInfo = ((MemberExpression)propertyExpression.Body).Member;

            return new SetterSpec<TTgt, TSrc, TProp>(this, (PropertyInfo)memberInfo);
        }

        public TypeMappingSpec<TTgt, TSrc> 
            IgnoringTargetProperty<T>(Expression<Func<TTgt, T>> propertyExpression)
        {
            var propInfo = GetPropertyInfo(propertyExpression, nameof(IgnoringTargetProperty), "tgt");

            var targetProperties = this.tgtProperties.Where(x => !x.Equals(propInfo));

            return new TypeMappingSpec<TTgt, TSrc>(
                targetProperties.ToArray(),
                srcProperties,
                mappingActions,
                assembler
                );
        }

        public TypeMappingSpec<TTgt, TSrc> IgnoringSourceProperty<T>(Expression<Func<TSrc, T>> propertyExpression)
        {
            var propInfo = GetPropertyInfo(propertyExpression, nameof(IgnoringSourceProperty), "src");

            var sourceProperties = this.srcProperties.Where(x => !x.Equals(propInfo));

            return new TypeMappingSpec<TTgt, TSrc>(
                tgtProperties,
                sourceProperties.ToArray(),
                mappingActions,
                assembler
                );
        }

        private PropertyInfo GetPropertyInfo<T1, T2>(
            Expression<Func<T1, T2>> expr,
            string methodName,
            string paramName)
        {
            var expression = expr.Body as MemberExpression;
            if (expression != null)
            {
                var propInfo = expression.Member as PropertyInfo;

                if (propInfo != null)
                    return propInfo;
            }
            throw new Exception($"{methodName}(...) requires an expression "
                + $"that is a simple property access of the form '{paramName} => {paramName}.Property'.");
        }

        private Expression<Action<TTgt, TSrc>> ToExpression(Action<TTgt, TSrc> delegateInstance)
        {
            return (tgt, src) => delegateInstance(tgt, src);
        }

        private sealed class Mapper : IMapper<TTgt, TSrc>
        {
            private readonly Action<TTgt, TSrc> mappingAction;
            private readonly IAssembler<TTgt, TSrc> assembler;

            public Mapper(Action<TTgt, TSrc> mappingAction, IAssembler<TTgt, TSrc> assembler)
            {
                this.mappingAction = mappingAction;
                this.assembler = assembler;
            }

            public TTgt Map(TSrc source)
            {
                return assembler.Assemble(source, mappingAction);
            }
        }
    }
}