using System;

namespace FluentMapping
{
    public interface IAssembler<TTgt, TSrc>
    {
        TTgt Assemble(TSrc source, Action<TTgt, TSrc> mappingAction);
    }
}
