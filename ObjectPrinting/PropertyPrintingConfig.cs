using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        public PrintingConfig<TOwner> ParentConfig { get; }
        public readonly PropertyInfo PropertyInfo;
        
        /// <summary>
        /// Конструктор, который создаёт экземпляр уточняющих настроек печати для типа данных
        /// </summary>
        /// <param name="parentConfig">Основные настройки, в которые необходимо сохранить изменения</param>
        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            ParentConfig = parentConfig;
            PropertyInfo = null;
        }

        /// <summary>
        /// Конструктор, который создаёт экземпляр уточняющих настроек печати для конктретного свойства объекта
        /// </summary>
        /// <param name="parentConfig">Основные настройки, в которые необходимо сохранить изменения</param>
        /// <param name="propertyInfo">Свойство объекта, для которого создаются уточняющие настройки</param>
        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, PropertyInfo propertyInfo)
        {
            ParentConfig = parentConfig;
            PropertyInfo = propertyInfo;
        }
        
        /// <summary>
        /// Метод, который позволяет указать правило сериализации
        /// </summary>
        /// <param name="serializer">Функция, которая преобразует данные в строковое представление</param>
        /// <returns>Исходный объект настроек с новым правилосм</returns>
        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
        {
            return PropertyInfo == null
                ? SetSerializerForType(serializer)
                : SetSerializerForProperty(serializer);
        }

        public PrintingConfig<TOwner> Using(CultureInfo cultureInfo)
        {
            ParentConfig.AddCultureInfo(typeof(TPropType), cultureInfo);
            return ParentConfig;
        }

        private PrintingConfig<TOwner> SetSerializerForType(Func<TPropType, string> serializer)
        {
            ParentConfig.AddSerializer(typeof(TPropType), serializer);
            return ParentConfig;
        }

        private PrintingConfig<TOwner> SetSerializerForProperty(Func<TPropType, string> serializer)
        {
            ParentConfig.AddSerializer(PropertyInfo, serializer);
            return ParentConfig;
        }
    }
}