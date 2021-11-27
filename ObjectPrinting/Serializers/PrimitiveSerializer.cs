using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Serializers
{
    public class PrimitiveSerializer : ISerializer
    {
        private readonly HashSet<Type> finalTypes = new()
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        public StringBuilder Serialize(object obj, Nesting nesting) =>
            new StringBuilder(obj.ToString());

        public StringBuilder Serialize(object obj) => Serialize(obj, new Nesting());

        public bool CanSerialize(object obj)
        {
            var type = obj.GetType();
            return finalTypes.Contains(type);
        }
    }
}