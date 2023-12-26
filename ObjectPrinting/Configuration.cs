using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting
{
    public class Configuration
    {
        public List<Predicate> ToExclude;
        public List<(Predicate predicate, Func<object, string> serializer)> Serializers;
        public Options Options;

        public Configuration
            (List<Predicate> toExclude = null, 
            List<(Predicate predicate, Func<object, string> serializer)> serializers =null)
        {
            ToExclude = toExclude ?? new List<Predicate>();
            Serializers = serializers ?? new List<(Predicate predicate, Func<object, string> serializer)>();
            Options = new Options();
        }
    }
}
