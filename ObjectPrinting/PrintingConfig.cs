using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public const string NullToString = "null";

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(long), typeof(Guid)
        };
        private readonly HashSet<object> serializedMembers = new HashSet<object>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<string> excludedFieldsProperties = new HashSet<string>();

        private string pinnedPropertyName;

        private readonly Dictionary<Type, Delegate> specialSerializationsForTypes =
            new Dictionary<Type, Delegate>();


        private readonly Dictionary<string, Delegate> specialSerializationsForFieldsProperties =
            new Dictionary<string, Delegate>();


        private CultureInfo culture = CultureInfo.InvariantCulture;
        private int resultStartIndex;
        private int resultLength = int.MaxValue;



        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {

            if (serializedMembers.Contains(obj))
                return ((nestingLevel != 0) ? "this (parentObj)"
                    : GetTrimString(new StringBuilder("this (parentObj)"))) + "\r\n";

            var sb = new StringBuilder();

            if (obj == null)
                return NullToString + Environment.NewLine;

            var objType = obj.GetType();

            if (finalTypes.Contains(objType) && !excludedTypes.Contains(objType))
            {
                /*
                Delegate serializeDelegate = null;
                if (specialSerializationsForFieldsProperties.ContainsKey(objectName))
                    serializeDelegate = specialSerializationsForFieldsProperties[objectName];

                else if (specialSerializationsForTypes.ContainsKey(objType))
                    serializeDelegate = specialSerializationsForTypes[objType];

                
                if (serializeDelegate != null)
                {
                    if (obj is IFormattable formattable)
                        return serializeDelegate.DynamicInvoke(formattable.ToString(null, culture)) + Environment.NewLine;
                    return serializeDelegate.DynamicInvoke() + Environment.NewLine;
                }
                else
                {
                    if (obj is IFormattable formattable)
                        return formattable.ToString(null, culture) + Environment.NewLine;
                    return obj + Environment.NewLine;
                }
                */
                //return obj + Environment.NewLine;

                if (obj is IFormattable formattable)
                    return formattable.ToString(null, culture) + Environment.NewLine;
                return obj + Environment.NewLine;
            }

            var identation = new string('\t', nestingLevel + 1);
            sb.AppendLine(objType.Name);
            var fields =
                objType.GetFields(BindingFlags.Public | BindingFlags.Instance).Cast<MemberInfo>();
            var props =
                objType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Cast<MemberInfo>();
            var fieldsAndProperties = fields.Union(props);

            foreach (var memberInfo in fieldsAndProperties)
            {
                SerializationMemberInfo memberSerialization = default;
                if (memberInfo is PropertyInfo propertyInfo)
                {
                    object value;
                    var indexParameters = propertyInfo.GetIndexParameters();
                    if (indexParameters.Length != 0)
                    {
                        var cast = obj as ICollection;
                        if (cast == null)
                            sb.Append(propertyInfo.Name);
                        else
                        {
                            var counter = 0;
                            sb.Append(identation + propertyInfo.Name + " =\r\n");
                            foreach (var parameter in cast)
                            {
                                value = parameter.ToString();
                                sb.Append(identation + "\t" + "Index " + counter + " = " + value + "\r\n");
                                counter++;
                            }
                        }
                        continue;
                    }
                    value = propertyInfo.GetValue(obj);
                    memberSerialization =
                        new SerializationMemberInfo(propertyInfo.Name,
                            propertyInfo.PropertyType,
                            value);
                }
                else if (memberInfo is FieldInfo fieldInfo)
                {
                    memberSerialization =
                        new SerializationMemberInfo(fieldInfo.Name,
                            fieldInfo.FieldType,
                            fieldInfo.GetValue(obj));
                }



                if (excludedTypes.Contains(memberSerialization.MemberType)
                    || excludedFieldsProperties.Contains(memberSerialization.MemberName))
                    continue;



                Delegate serializeDelegate = null;

                if (specialSerializationsForFieldsProperties.ContainsKey(memberSerialization.MemberName))
                    serializeDelegate = specialSerializationsForFieldsProperties[memberSerialization.MemberName];

                else if (specialSerializationsForTypes.ContainsKey(memberSerialization.MemberType))
                    serializeDelegate = specialSerializationsForTypes[memberSerialization.MemberType];

                serializedMembers.Add(obj);

                if (serializeDelegate != null)
                {
                    sb.Append(identation + memberSerialization.MemberName + " = " +
                              PrintToString(serializeDelegate.DynamicInvoke(memberSerialization.MemberValue),
                                  nestingLevel + 1));
                }
                else
                {
                    sb.Append(identation + memberSerialization.MemberName + " = " +
                              PrintToString(memberSerialization.MemberValue,
                                  nestingLevel + 1));
                }
            }

            var a = sb.ToString();
            return (nestingLevel != 0) ? sb.ToString() : GetTrimString(sb);
        }

        private string GetTrimString(StringBuilder resultString)
        {
            return resultString.ToString()
                [resultStartIndex..Math.Min(resultLength, resultString.Length)]; ;
        } 
        
        public PrintingConfig<TOwner> ExcludedType<TExType>()
        {
            excludedTypes.Add(typeof(TExType));
            return this;
        }

        public PrintingConfig<TOwner> SpecialSerializationType<TType>(Func<TType, string> specialSerializationForType)
        {
            //var function = specialSerializationForType as Func<Type, string>;
            specialSerializationsForTypes[typeof(TType)] = specialSerializationForType;
            return this;
        }

        public PrintingConfig<TOwner> SpecialSerializationField<TFieldType>(Func<TFieldType, string> serialization)
        {
            //var function = serialization as Func<object, string>;
            //var fieldName = "Id";

            if (pinnedPropertyName != null)
            {
                specialSerializationsForFieldsProperties[pinnedPropertyName] = serialization;
                pinnedPropertyName = null;
            }
            else
            {
                foreach (var propertyName in specialSerializationsForFieldsProperties.Keys)
                {
                    specialSerializationsForFieldsProperties[propertyName] = serialization;
                }
            }

            //var result = serialization;

            //specialSerializationsForFieldsProperties[fieldName] = serialization;
            //specialSerializationsForFieldsProperties[fieldName] = result;


            return this;
        }

        public PrintingConfig<TOwner> ExcludedProperty(Expression<Func<TOwner, string>> propertyNameExpression)
        {

            excludedFieldsProperties.Add(((ConstantExpression)propertyNameExpression.Body).Value.ToString());
            return this;
        }

        public PrintingConfig<TOwner> SetCulture(Expression<Func<TOwner,CultureInfo>> inputCulture)
        {
            var cultureName = ((NewExpression)inputCulture.Body).Arguments.First().ToString();
            culture = new CultureInfo(cultureName[1..^1]);
            return this;
        }

        public PrintingConfig<TOwner> PinProperty(Expression<Func<TOwner, string>> propertyNameExpression)
        {
            pinnedPropertyName = ((ConstantExpression)propertyNameExpression.Body).Value.ToString();
            return this;
        }

        public PrintingConfig<TOwner> Trim(int start = 0, int length = int.MaxValue)
        {
            if (start < 0 || length < 0)
                throw new ArgumentException();
            resultStartIndex = start;
            resultLength = length;
            return this;
        }
    }
}