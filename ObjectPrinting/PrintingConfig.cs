using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<MemberInfo> excludedMembers;
        private readonly HashSet<Type> excludedTypes;
        private readonly Dictionary<Type, Func<object, string>> typeAltPrintings;
        private readonly Dictionary<MemberInfo, Func<object, string>> memberAltPrintings;
        private readonly string newLine;
        private readonly Type[] finalTypes;
        
        public PrintingConfig()
        {
            excludedMembers = new HashSet<MemberInfo>();
            excludedTypes = new HashSet<Type>();
            typeAltPrintings = new Dictionary<Type, Func<object, string>>();
            memberAltPrintings = new Dictionary<MemberInfo, Func<object, string>>();
            finalTypes = new[] { typeof(int), typeof(double), typeof(float),
                typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(bool) };
            newLine = Environment.NewLine;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public void AddTypeAltPrinting(Type propType, Func<object, string> printingData)
        {
            typeAltPrintings[propType] = printingData;
        }
        
        public void AddPropAltPrinting(MemberInfo propType, Func<object, string> printingData)
        {
            memberAltPrintings[propType] = printingData;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression)memberSelector.Body).Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression)memberSelector.Body).Member;
            excludedMembers.Add(propInfo);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return $"null{newLine}";
            
            var objType = obj.GetType();
            if (finalTypes.Contains(objType))
                return $"{obj}{newLine}";

            var sb = new StringBuilder();
            sb.AppendLine(objType.Name);
            if (typeof(IEnumerable).IsAssignableFrom(objType))
                sb.Append(PrintCollection((IEnumerable)obj, nestingLevel + 1));
            else
            {
                var bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public 
                                                            | BindingFlags.Instance;
                var propsAndFields = objType.GetProperties(bindingAttr)
                    .Concat<MemberInfo>(objType.GetFields(bindingAttr));
                
                foreach (var memberInfo in propsAndFields)
                {
                    if (nestingLevel > 5)
                        continue;
                    
                    sb.Append(PrintMember(obj, memberInfo, nestingLevel));
                }
            }
            return sb.ToString();
        }

        private string PrintMember(object obj, MemberInfo memberInfo, int nestingLevel)
        {
            var (memberType, memberValue, memberAltPrint) = TryGetData(obj, memberInfo);
            if (memberType == null || excludedMembers.Contains(memberInfo) 
                                   || excludedTypes.Contains(memberType))
                return string.Empty;

            var memberPrinted = memberAltPrint == null
                ? PrintToString(memberValue, nestingLevel + 1)
                : $"{memberAltPrint(memberValue)}{newLine}";
            
            var identation = new string(' ', 4 * (nestingLevel + 1));
            return $"{identation}{memberInfo.Name} = {memberPrinted}";
        }
        
        private string PrintCollection(IEnumerable objIEnumerable, int nestingLevel)
        {
            var sb = new StringBuilder();
            var identation = new string(' ', 4 * (nestingLevel + 1));
            foreach (var obj in objIEnumerable)
                sb.Append($"{identation}{PrintToString(obj,  nestingLevel + 1)}");

            return sb.ToString();
        }

        private (Type, object, Func<object, string>) TryGetData(object obj, MemberInfo memberInfo)
        {
            Type type;
            object value;
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                {
                    var fieldInfo = (FieldInfo) memberInfo;
                    (type, value) = GetFieldInfo(fieldInfo, obj);
                    break;
                }
                case MemberTypes.Property:
                {
                    var propertyInfo = (PropertyInfo) memberInfo;
                    (type, value) = GetPropInfo(propertyInfo, obj);
                    break;
                }
                default:
                    return (null, null, null);
            }

            memberAltPrintings.TryGetValue(memberInfo, out var altPrint);
            if (altPrint == null)
                typeAltPrintings.TryGetValue(type, out altPrint);
            
            return (type, value, altPrint);
        }

        private static (Type, object) GetFieldInfo(FieldInfo fieldInfo, object obj)
        {
            var fieldType = fieldInfo.FieldType;
            var value = fieldInfo.GetValue(obj);
            return (fieldType, value);
        }
        
        private static (Type, object) GetPropInfo(PropertyInfo propertyInfo, object obj)
        {
            var propertyType = propertyInfo.PropertyType;
            var value = propertyInfo.GetValue(obj);
            return (propertyType, value);
        }
    }
}