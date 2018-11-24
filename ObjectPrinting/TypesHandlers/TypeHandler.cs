using System.Collections.Immutable;

namespace ObjectPrinting.TypesHandlers
{
    public abstract class TypeHandler
    {
        protected TypeHandler Successor;

        public TypeHandler SetSuccessor(TypeHandler successor)
        {
            Successor = successor;

            return this;
        }

        public abstract string Handle(object obj, int nestingLevel, ImmutableHashSet<object> excludedValues, TypeHandler handler);
    }
}