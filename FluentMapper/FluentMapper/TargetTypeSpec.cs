namespace FluentMapping
{
    public sealed class TargetTypeSpec<TTgt>
    {
        public TypeMappingSpec<TTgt, TSrc> From<TSrc>()
        {
            return new TypeMappingSpec<TTgt, TSrc>();
        }
    }
}