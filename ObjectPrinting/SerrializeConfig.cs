using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class SerrializeConfig
    {
        /*
         * У объекта для серриализации может быть вложенное поле, имеющшее тип, который мы сами определили,
         * и для него тоже может быть возможность определить кастомный сериализатор.
         * Также изначально не известно какие собственные типы есть у объекта, поэтому нужен отдельный хеш-сет,
         * где хранятся все исключенные типы.
         */

        public readonly HashSet<Type> ExcludedTypes;
        public readonly HashSet<PropertyInfo> ExcludedProperties;

        public readonly Dictionary<Type, Delegate> TypeSerrializers;
        public readonly Dictionary<PropertyInfo, Delegate> PropertySerrializers;

        public SerrializeConfig()
        {
            ExcludedTypes = new HashSet<Type>();

            ExcludedProperties = new HashSet<PropertyInfo>();

            TypeSerrializers = new Dictionary<Type, Delegate>
            {
                { typeof(int), DefaultSerrialize },
                { typeof(double), DefaultSerrialize },
                { typeof(float), DefaultSerrialize },
                { typeof(string), DefaultSerrialize },
                { typeof(DateTime), DefaultSerrialize },
                { typeof(TimeSpan), DefaultSerrialize },
                { typeof(Guid), DefaultSerrialize },
            };

            PropertySerrializers = new Dictionary<PropertyInfo, Delegate>();
        }

        public SerrializeConfig(SerrializeConfig old)
        {
            ExcludedTypes = new HashSet<Type>(old.ExcludedTypes);

            ExcludedProperties = new HashSet<PropertyInfo>(old.ExcludedProperties);

            TypeSerrializers = new Dictionary<Type, Delegate>(old.TypeSerrializers);

            PropertySerrializers = new Dictionary<PropertyInfo, Delegate>(old.PropertySerrializers);
        }

        private string DefaultSerrialize(object obj) => obj.ToString();
    }
}
