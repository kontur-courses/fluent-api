using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public List<Func<MemberInfo, bool>> ExcludeProperties { get; set; }
        public Dictionary<Type, Delegate> AlternativeSerializationByType { get; set; }
        public Dictionary<string, Delegate> AlternativeSerializationByName { get; set; }
        public List<object> ViewedObjects { get; set; }

        public PrintingConfig()
        {
            ExcludeProperties = new List<Func<MemberInfo, bool>>();
            AlternativeSerializationByType = new Dictionary<Type, Delegate>();
            ViewedObjects = new List<object>();
            AlternativeSerializationByName = new Dictionary<string, Delegate>();
        }
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var type = obj.GetType();
            var members = type.GetMembers().Where(member => (member.MemberType & MemberTypes.Property) != 0 ||
                                                               (member.MemberType & MemberTypes.Field) != 0).ToArray();
            foreach (var e in ExcludeProperties)
                members = members.Where(e).ToArray();
            var result =PrintMembers(obj, nestingLevel, identation, members);
            return result;
        }

        private string PrintMembers(object obj, int nestingLevel, string identation, MemberInfo[] members)
        {
            var sb = new StringBuilder();
            sb.AppendLine(obj.GetType().Name);
            foreach (var memberInfo in members)
            {
                var value = GetValue(memberInfo, obj);
                var propertyType = GetType(memberInfo);
                if (ViewedObjects.Contains(value))
                    continue;
                ViewedObjects.Add(value);
                if (AlternativeSerializationByType.ContainsKey(propertyType))
                {
                    var result = AlternativeSerializationByType[propertyType].DynamicInvoke(value);
                    sb.Append(identation+result+"\r\n");
                    continue;
                }
                if (AlternativeSerializationByName.ContainsKey(memberInfo.Name))
                {
                    var result = AlternativeSerializationByName[memberInfo.Name].DynamicInvoke(value);
                    sb.Append(identation + result + "\r\n");
                    continue;
                }
                sb.Append(identation + memberInfo.Name + " = " +
                              PrintToString(value,nestingLevel + 1));
            }

            return sb.ToString();
        }

        private Type GetType(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.PropertyType;
            }

            var fieldInfo = (FieldInfo)memberInfo;
            return fieldInfo.FieldType;
        }

        private object GetValue(MemberInfo memberInfo, object obj)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetValue(obj);
            }

            var fieldInfo = (FieldInfo)memberInfo;
            return fieldInfo.GetValue(obj);
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            var excludedType = typeof(TPropType);
            var excludedFunc = new Func<MemberInfo, bool>(memberInfo => GetType(memberInfo) != excludedType);
            ExcludeProperties.Add(excludedFunc);
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> excludedExpression)
        {
            var excludedFunc = new Func<MemberInfo, bool>(memberInfo => memberInfo.Name !=
                                                                        ((MemberExpression)excludedExpression.Body).Member.Name);
            ExcludeProperties.Add(excludedFunc);
            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> Serializer<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }


        public TypePrintingConfig<TOwner, TPropType> Serializer<TPropType>(Expression<Func<TOwner, TPropType>> alternativeExpression)
        {
            ITypePrintingConfig<TOwner> tPrintingConfig = new TypePrintingConfig<TOwner, TPropType>(this);
            tPrintingConfig.NameMember = ((MemberExpression)alternativeExpression.Body).Member.Name;
            return  (TypePrintingConfig < TOwner, TPropType > )tPrintingConfig;
        }
    }
}