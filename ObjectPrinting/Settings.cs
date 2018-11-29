using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public class Settings
    {
        private readonly HashSet<Type> _excludedProperties;

        public Settings()
        {
            _excludedProperties = new HashSet<Type>();
        }

        public void AddPropertyToExclude(Type property)
        {
            _excludedProperties.Add(property);
        }

        public HashSet<Type> GetExcludedProperties()
        {
            return _excludedProperties;
        }
    }
}
