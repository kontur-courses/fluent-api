using System;

namespace FluentMapping
{
    internal sealed class DefaultAssembler<TTgt, TSrc> : IAssembler<TTgt, TSrc>
    {
        TTgt IAssembler<TTgt, TSrc>.Assemble(TSrc source, Action<TTgt, TSrc> mappingAction)
        {
            return DefaultAssembler<TTgt, TSrc>.Assemble(source, mappingAction);
        }

        public static TTgt Assemble(TSrc source, Action<TTgt, TSrc> mappingAction)
        { 
            if (source == null)
                throw new ArgumentNullException("source", $"Cannot map instance of {typeof(TTgt).Name}" +
                    $" from null instance of {typeof(TSrc).Name}.");

            var target = (TTgt)Activator.CreateInstance(typeof(TTgt));

            mappingAction(target, source);

            return target;
        }
    }
}
