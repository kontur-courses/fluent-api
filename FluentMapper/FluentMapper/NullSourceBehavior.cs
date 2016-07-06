using FluentMapping.Internal;
using System;

namespace FluentMapping
{
    public interface INullSourceBehaviorState<TTgt, TSrc>
    {
        TypeMappingSpec<TTgt, TSrc> Spec { get; }
    }

    public sealed class NullSourceBehavior<TTgt, TSrc>
        : INullSourceBehaviorState<TTgt, TSrc>
    {
        TypeMappingSpec<TTgt, TSrc> INullSourceBehaviorState<TTgt, TSrc>.Spec => _spec;

        private TypeMappingSpec<TTgt, TSrc> _spec;

        public NullSourceBehavior(TypeMappingSpec<TTgt, TSrc> spec)
        {
            _spec = spec;
        }

        public TypeMappingSpec<TTgt, TSrc> ReturnNull()
        {
            return _spec.Transforms()
                .WithAssembler(new ReturnNullAssembler<TTgt, TSrc>());
        }

        public TypeMappingSpec<TTgt, TSrc> CreateEmpty()
        {
            return _spec.Transforms()
                .WithAssembler(new CreateEmptyAssembler<TTgt, TSrc>());
        }
    }
}