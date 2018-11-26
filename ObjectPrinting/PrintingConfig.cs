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
        public List<object> ViewedObjects { get; set; }

        public PrintingConfig()
        {
            ExcludeProperties = new List<Func<MemberInfo, bool>>();
            AlternativeSerializationByType = new Dictionary<Type, Delegate>();
            ViewedObjects = new List<object>();
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
                if (AlternativeSerializationByType.ContainsKey(propertyType))
                {
                    var result = AlternativeSerializationByType[propertyType].DynamicInvoke(value);
                    sb.Append(result);
                    continue;
                }
                if (ViewedObjects.Contains(value))
                    continue;
                ViewedObjects.Add(value);
                sb.Append(identation + memberInfo.Name + " = " +
                              PrintToString(value,
                                  nestingLevel + 1));
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
            var excludedFunc = new Func<MemberInfo, bool>(property => property.Name !=
                                                                        ((MemberExpression)excludedExpression.Body).Member.Name);
            ExcludeProperties.Add(excludedFunc);
            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> Serializer<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }


        public TypePrintingConfig<TOwner, TPropType> Serializer<TPropType>(Expression<Func<TOwner, TPropType>> propertyFunc)
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }
    }
}