using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace ObjectPrintingTests
{
    public class ObjectDescription
    {
        private readonly string description;
        private readonly ImmutableList<ObjectDescription> childrenFields = ImmutableList<ObjectDescription>.Empty;

        public ObjectDescription(string description)
        {
            this.description = description;
        }

        private ObjectDescription(string description, IEnumerable<ObjectDescription> fields)
        {
            this.description = description;
            childrenFields = ImmutableList.CreateRange(fields);
        }

        public ObjectDescription WithFields(params string[] fields)
        {
            var newChildren = childrenFields.AddRange(fields.Select(field => new ObjectDescription(field)));
            return new ObjectDescription(description, newChildren);
        }

        public ObjectDescription WithFields(params ObjectDescription[] fields)
        {
            var newChildren = childrenFields.AddRange(fields);
            return new ObjectDescription(description, newChildren);
        }

        public override string ToString() => ToString(this, 1);

        private static string ToString(ObjectDescription objectDescription, int nestingLevel)
        {
            var builder = new StringBuilder(objectDescription.description + Environment.NewLine);
            foreach (var field in objectDescription.childrenFields)
            {
                builder.Append(new string('\t', nestingLevel));
                builder.Append(ToString(field, nestingLevel + 1));
            }

            return builder.ToString();
        }
    }
}