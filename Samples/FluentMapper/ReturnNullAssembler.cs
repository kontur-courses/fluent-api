using System;

namespace FluentMapping
{
    public sealed class ReturnNullAssembler<TTgt, TSrc> : IAssembler<TTgt, TSrc>
    {
        public TTgt Assemble(TSrc source, Action<TTgt, TSrc> mappingAction)
        {
            if (source == null)
                return default;

            return DefaultAssembler<TTgt, TSrc>.Assemble(source, mappingAction);
        }
    }
}