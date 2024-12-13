using FluentMapping.Internal;

namespace FluentMapping
{
    public interface INullSourceBehaviorState<TTgt, TSrc>
    {
        TypeMappingSpec<TTgt, TSrc> Spec { get; }
    }

    public sealed class NullSourceBehavior<TTgt, TSrc>
        : INullSourceBehaviorState<TTgt, TSrc>
    {
        TypeMappingSpec<TTgt, TSrc> INullSourceBehaviorState<TTgt, TSrc>.Spec => spec;

        private TypeMappingSpec<TTgt, TSrc> spec;

        public NullSourceBehavior(TypeMappingSpec<TTgt, TSrc> spec)
        {
            this.spec = spec;
        }

        public TypeMappingSpec<TTgt, TSrc> ReturnNull()
        {
            return spec.Transforms()
                .WithAssembler(new ReturnNullAssembler<TTgt, TSrc>());
        }

        public TypeMappingSpec<TTgt, TSrc> CreateEmpty()
        {
            return spec.Transforms()
                .WithAssembler(new CreateEmptyAssembler<TTgt, TSrc>());
        }
    }
}