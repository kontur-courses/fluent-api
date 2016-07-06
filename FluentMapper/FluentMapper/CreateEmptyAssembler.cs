using System;

namespace FluentMapping
{
    internal sealed class CreateEmptyAssembler<TTgt, TSrc> : IAssembler<TTgt, TSrc>
    {
        public TTgt Assemble(TSrc source, Action<TTgt, TSrc> mappingAction)
        {
            if (source == null)
                return Activator.CreateInstance<TTgt>();

            return DefaultAssembler<TTgt, TSrc>.Assemble(source, mappingAction);
        }
    }
}
