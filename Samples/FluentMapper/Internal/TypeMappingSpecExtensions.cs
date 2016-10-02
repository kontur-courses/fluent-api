namespace FluentMapping.Internal
{
    public static class TypeMappingSpecExtensions
    {
        public static ITypeMappingSpecProperties<TTgt, TSrc> 
            Properties<TTgt, TSrc>(this TypeMappingSpec<TTgt, TSrc> spec)
        {
            return spec;
        }

        public static ITypeMappingSpecTransforms<TTgt, TSrc> 
            Transforms<TTgt, TSrc>(this TypeMappingSpec<TTgt, TSrc> spec)
        {
            return spec;
        }
    }
}
