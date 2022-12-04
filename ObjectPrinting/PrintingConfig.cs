using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private List<Type> removedTypes = new List<Type>();
        private Dictionary<Type, CultureInfo> cultures = new Dictionary<Type, CultureInfo>();

        private Dictionary<Type, Func<object, string>> alternativeSerialization =
            new Dictionary<Type, Func<object, string>>();

        public Dictionary<string, Func<object, string>> PropertySerialization =
            new Dictionary<string, Func<object, string>>();

        public Dictionary<string, int> PropertiesToCrop =
            new Dictionary<string, int>();

        private List<PropertyInfo> exludingProperty = new List<PropertyInfo>();

        private List<ObjectAndNestingLevel> viewedObjects = new List<ObjectAndNestingLevel>();
        
        private Type iFormattableType = typeof(IFormattable);

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> ExcludeType<T>()
        {
            var type = typeof(T);
            removedTypes.Add(type);
            return this;
        }

        public PrintingConfig<TOwner> ExcludeProperty(PropertyInfo propertyInfo)
        {
            exludingProperty.Add(propertyInfo);
            return this;
        }

        public Serialization<TOwner> SerializeProperty(String propertyInfoName)
        {
            var serialization = new Serialization<TOwner>();
            serialization.EditingPropertyInfoName = propertyInfoName;
            serialization.PrintingConfig = this;
            return serialization;
        }

        public Trimmer<TOwner> SelectString(String propertyInfoName)
        {
            var cropper = new Trimmer<TOwner>();
            cropper.EditingPropertyInfoName = propertyInfoName;
            cropper.PrintingConfig = this;
            return cropper;
        }

        public PrintingConfig<TOwner> SerializeType<T>(Func<T, string> func)
        {
            alternativeSerialization.Add(typeof(T), x => func((T)x));
            return this;
        }

        public PrintingConfig<TOwner> SetCulture<T>(CultureInfo culture)
        {
            cultures.Add(typeof(T), culture);
            return this;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            var type = obj.GetType();

            if (alternativeSerialization.ContainsKey(type))
                return alternativeSerialization[type](obj) + Environment.NewLine;

            if (cultures.ContainsKey(type))
                return ((IFormattable)obj).ToString(null, cultures[type]) + Environment.NewLine;
            
            if (type.GetInterfaces().Contains(iFormattableType))
            {
                return obj + Environment.NewLine;
            }
            
            if (obj.ToString() != obj.GetType().ToString())
            {
                return obj + Environment.NewLine;
            }

            //возвращение пустой строки, если уже обрабатывали такой объект
            //предотвращение цикла при обработке циклических объектов
            for (int i = 0; i < viewedObjects.Count; i++)
            {
                if (viewedObjects[i].Object == obj && nestingLevel != viewedObjects[i].NestingLevel)
                {
                    return "(cycle reference)" + Environment.NewLine;    
                }
            }

            viewedObjects.Add(new ObjectAndNestingLevel(obj, nestingLevel));
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();


            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (removedTypes.Contains(propertyInfo.PropertyType))
                    continue;

                if (exludingProperty.Contains(propertyInfo))
                    continue;

                if (PropertySerialization.ContainsKey(propertyInfo.Name))
                {
                    sb.Append(identation + propertyInfo.Name + " = " +
                              PropertySerialization[propertyInfo.Name](propertyInfo.GetValue(obj))
                              + Environment.NewLine);
                    continue;
                }

                if (PropertiesToCrop.ContainsKey(propertyInfo.Name))
                {
                    var propertyInfoType = propertyInfo.PropertyType;
                    if (propertyInfoType != typeof(string))
                    {
                        throw new ArgumentException();
                    }
                    var propInfoValue = propertyInfo.GetValue(obj).ToString();
                    var length = PropertiesToCrop[propertyInfo.Name];
                    
                    if (propInfoValue.Length < length)
                    {
                        throw new ArgumentException();
                    }
                    
                    sb.Append(identation + propertyInfo.Name + " = " +
                              propInfoValue.Substring(0, length)
                              + Environment.NewLine);
                    continue;
                }

                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }

            return sb.ToString();
        }
    }
}