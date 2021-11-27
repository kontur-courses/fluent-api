using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ObjectPrinting.PrintingMembers;

namespace ObjectPrinting.Serializers
{
    public record Nesting
    {
        public int Level { get; set; }
        public int Offset { get; set; }
        public char IndentationSymbol { get; set; } = '\t';
        public string Indentation => new string(IndentationSymbol, Level) + new string(' ', Offset);

        public Nesting(int level = 0, int offset = 0)
        {
            Level = level;
            Offset = offset;
        }
    }

    public class ObjectSerializer : ISerializer
    {
        private readonly PrintingConfig config;
        private readonly List<ISerializer> serializers;
        private readonly List<ICollectionSerializer> collectionSerializers;

        public ObjectSerializer(PrintingConfig config)
        {
            this.config = config;
            serializers = new List<ISerializer>
            {
                new PrimitiveSerializer()
            };

            collectionSerializers = new List<ICollectionSerializer>
            {
                new ArraySerializer(this)
            };
        }

        public StringBuilder Serialize(object obj) => Serialize(obj, new Nesting());

        public bool CanSerialize(object obj) => true;

        public StringBuilder Serialize(object obj, Nesting nesting) =>
            TryGetTypeString(obj, nesting, out var typeBuilder)
                ? typeBuilder
                : BuildComplexObjectString(obj, nesting);

        private StringBuilder BuildComplexObjectString(object obj, Nesting nesting)
        {
            var builder = new StringBuilder();
            var type = obj.GetType();
            builder.AppendLine(type.Name);

            builder.Append(GetPropertiesBuilder(obj, nesting with {Level = nesting.Level + 1}));

            return builder;
        }

        private StringBuilder GetPropertiesBuilder(object obj, Nesting nesting)
        {
            var builder = new StringBuilder();
            foreach (var objMember in GetSuitableObjectMembers(obj))
            {
                builder.Append(nesting.Indentation);
                builder.Append(BuildMemberString(obj, objMember, nesting));
            }

            return builder;
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

        private StringBuilder BuildMemberString(object obj, PrintingMember objMember, Nesting nesting)
        {
            if (TryGetMemberString(objMember.MemberInfo, out var propString))
                return new StringBuilder(propString);

            var builder = new StringBuilder(objMember.Name + " = ");
            builder.Append(BuildValueString(obj, objMember, nesting with {Offset = nesting.Offset + builder.Length}));
            return builder;
        }

        private bool TryGetMemberString(MemberInfo propertyInfo, out string propString)
        {
            if (config.MemberPrinting.TryGetValue(propertyInfo, out var printProperty))
            {
                propString = printProperty(propertyInfo) + Environment.NewLine;
                return true;
            }

            propString = null;
            return false;
        }

        private StringBuilder BuildValueString(object obj, PrintingMember objMember, Nesting nesting)
        {
            if (TryGetTypeString(obj, nesting, out var typeBuilder))
                return typeBuilder;

            if (nesting.Level >= 100)
                return new StringBuilder("...");

            return Serialize(objMember.GetValue(obj), nesting);
        }

        private bool TryGetTypeString(object obj, Nesting nesting, out StringBuilder typeBuilder)
        {
            typeBuilder = null;
            if (TrySerializeSingleType(obj, nesting, out typeBuilder))
            {
                typeBuilder.Append(Environment.NewLine);
                return true;
            }

            TrySerializeCollection(obj, nesting, out typeBuilder);

            return typeBuilder != null;
        }

        private bool TrySerializeSingleType(object obj, Nesting nesting, out StringBuilder typeBuilder)
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

        private bool TrySerializeCollection(object obj, Nesting nesting, out StringBuilder typeBuilder)
        {
            typeBuilder = null;

            var serializer = collectionSerializers.Find(serializer => serializer.CanSerialize(obj));
            if (serializer == null)
                return false;

            var builder = new StringBuilder();
            var items = serializer.SerializeItems(obj, nesting);
            builder.AppendJoin(nesting.Indentation, items);

            typeBuilder = builder;
            return true;
        }
    }
}