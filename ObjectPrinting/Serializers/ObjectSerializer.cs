using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectPrinting.PrintingMembers;

namespace ObjectPrinting.Serializers
{
    public class ObjectSerializer : ISerializer
    {
        private readonly PrintingConfig config;
        private readonly List<ISerializer> serializers;

        public ObjectSerializer(PrintingConfig config)
        {
            this.config = config;
            serializers = new List<ISerializer>
            {
                new PrimitiveSerializer(),
                new ListSerializer(this),
                new DictionarySerializer(this)
            };
        }

        public bool CanSerialize(object obj) => true;
        
        public StringBuilder Serialize(object obj, Nesting nesting) =>
            TrySerializeType(obj, nesting, out var typeBuilder)
                ? typeBuilder
                : SerializeObject(obj, nesting);

        private StringBuilder SerializeObject(object obj, Nesting nesting)
        {
            var builder = new StringBuilder();
            var type = obj.GetType();
            builder.AppendLine(type.Name);

            builder.Append(SerializeMembers(obj, nesting with {Level = nesting.Level + 1}));
            return builder;
        }

        private StringBuilder SerializeMembers(object obj, Nesting nesting)
        {
            var members = GetSuitableObjectMembers(obj).Select(
                objMember =>
                {
                    var builder = new StringBuilder(nesting.Indentation);
                    return builder.Append(SerializeMember(obj, objMember, nesting));
                });

            return new StringBuilder().AppendJoin(Environment.NewLine, members);
        }

        private IEnumerable<PrintingMember> GetSuitableObjectMembers(object obj)
        {
            var type = obj.GetType();
            var members = type.GetMembers()
                .Where(config.PrintingMemberFactory.CanConvert)
                .Select(config.PrintingMemberFactory.Convert);

            return members
                .Where(objMember => !config.ExcludingTypes.Contains(objMember.Type))
                .Where(objMember => !config.ExcludingMembers.Contains(objMember.MemberInfo));
        }

        private StringBuilder SerializeMember(object obj, PrintingMember objMember, Nesting nesting)
        {
            if (config.MemberPrinting.TryGetValue(objMember.MemberInfo, out var printProperty))
                return new StringBuilder(printProperty(objMember.GetValue(obj)));

            var builder = new StringBuilder(objMember.Name + " = ");
            return builder.Append(
                SerializeValue(obj, objMember, nesting with {Offset = nesting.Offset + builder.Length}));
        }

        private StringBuilder SerializeValue(object obj, PrintingMember objMember, Nesting nesting)
        {
            if (TrySerializeType(obj, nesting, out var typeBuilder))
                return typeBuilder;

            if (nesting.Level >= 100)
                return new StringBuilder("...");

            return Serialize(objMember.GetValue(obj), nesting);
        }

        private bool TrySerializeType(object obj, Nesting nesting, out StringBuilder typeBuilder)
        {
            if (obj == null)
            {
                typeBuilder = new StringBuilder("null");
                return true;
            }

            if (config.TypePrinting.TryGetValue(obj.GetType(), out var printType))
            {
                typeBuilder = new StringBuilder(printType(obj));
                return true;
            }

            typeBuilder = serializers
                .Find(serializer => serializer.CanSerialize(obj))
                ?.Serialize(obj, nesting);

            return typeBuilder != null;
        }
    }
}