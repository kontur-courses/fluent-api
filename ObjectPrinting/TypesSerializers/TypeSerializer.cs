using System.Collections.Immutable;

namespace ObjectPrinting.TypesSerializers
{
    public abstract class TypeSerializer
    {
        protected TypeSerializer Successor;

        public TypeSerializer SetSuccessor(TypeSerializer successor)
        {
            Successor = successor;

            return this;
        }

        public abstract string Serialize(object obj, int nestingLevel, ImmutableHashSet<object> excludedValues);
    }
}