namespace FluentMapping
{
    public static class FluentMapper
    {
        public static TargetTypeSpec<TTgt> ThatMaps<TTgt>()
        {
            return new TargetTypeSpec<TTgt>();
        }
    }
}
