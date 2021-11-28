using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace ObjectPrintingTests.Helpers
{
    public class ObjectDescription
    {
        private readonly string description;
        private readonly ImmutableList<ObjectDescription> childrenFields = ImmutableList<ObjectDescription>.Empty;
        private readonly int offset;
        private readonly int extraIndentation;

        public ObjectDescription(string description = "")
        {
            this.description = description;
        }

        private ObjectDescription(string description, IEnumerable<ObjectDescription> fields, int offset,
            int extraIndentation)
        {
            this.description = description;
            childrenFields = ImmutableList.CreateRange(fields);
            this.offset = offset;
            this.extraIndentation = extraIndentation;
        }

        public ObjectDescription WithFields(params string[] fields)
        {
            var newChildren = childrenFields.AddRange(fields.Select(field => new ObjectDescription(field)));
            return new ObjectDescription(description, newChildren, offset, extraIndentation);
        }

        public ObjectDescription WithFields(params ObjectDescription[] fields)
        {
            var newChildren = childrenFields.AddRange(fields);
            return new ObjectDescription(description, newChildren, offset, extraIndentation);
        }

        public ObjectDescription WithOffset(int newOffset) =>
            new(description, childrenFields, newOffset, extraIndentation);

        public ObjectDescription WithExtraIndentation(int indentation)
            => new(description, childrenFields, offset, indentation);

        public override string ToString() => ToString(this, 1);

        private static string ToString(ObjectDescription objectDescription, int nestingLevel)
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(objectDescription.description))
            {
                builder.Append(new string('\t', objectDescription.extraIndentation));
                builder.Append(objectDescription.description);
            }

            foreach (var field in objectDescription.childrenFields)
            {
                builder.Append(Environment.NewLine);
                builder.Append(new string('\t', nestingLevel + objectDescription.extraIndentation));
                builder.Append(new string(' ', objectDescription.offset));
                builder.Append(ToString(field, nestingLevel + 1));
            }

            return builder.ToString();
        }
    }
}