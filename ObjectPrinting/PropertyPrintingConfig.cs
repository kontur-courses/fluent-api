using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProp>
    {
        private readonly SerrializeConfig serrializeConfig;
        private readonly PropertyInfo propertyInfo;

        public PropertyPrintingConfig(SerrializeConfig serrializeConfig, PropertyInfo propertyInfo = null)
        {
            this.serrializeConfig = new SerrializeConfig(serrializeConfig);
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TProp, string> serrializer)
        {
            if (propertyInfo == null)
            {
                var type = typeof(TProp);

                if (serrializeConfig.TypeSerrializers.ContainsKey(type))
                    serrializeConfig.TypeSerrializers[type] = serrializer;
                else
                    serrializeConfig.TypeSerrializers.Add(type, serrializer);
            }
            else
            {
                if (serrializeConfig.PropertySerrializers.ContainsKey(propertyInfo))
                    serrializeConfig.PropertySerrializers[propertyInfo] = serrializer;
                else
                    serrializeConfig.PropertySerrializers.Add(propertyInfo, serrializer);
            }


            return new PrintingConfig<TOwner>(serrializeConfig);
        }
    }
}
