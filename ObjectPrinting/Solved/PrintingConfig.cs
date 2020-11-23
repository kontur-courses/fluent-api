using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        private static readonly Type[] finalTypes = 
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };
        private readonly PrintingParameters parameters;

        public PrintingConfig()
        {
            parameters = PrintingParameters.Default;
        }

        internal PrintingConfig(PrintingParameters parameters)
        {
            this.parameters = parameters;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>
                (this, parameters, typeof(TPropType));
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {

            if (memberSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException();
            var memberAccess = (MemberExpression)memberSelector.Body;
            if (memberAccess.Member.MemberType != MemberTypes.Field &&
                memberAccess.Member.MemberType != MemberTypes.Property)
                throw new ArgumentException();
            return new PropertyPrintingConfig<TOwner, TPropType>
                (this, parameters, memberAccess.Member);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException();
            var memberAccess = (MemberExpression)memberSelector.Body;
            if (memberAccess.Member.MemberType != MemberTypes.Field &&
                memberAccess.Member.MemberType != MemberTypes.Property)
                throw new ArgumentException();
            return new PrintingConfig<TOwner>(parameters.AddMemberToExclude(memberAccess.Member));
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            return new PrintingConfig<TOwner>(parameters.AddTypeToExclude(typeof(TPropType)));
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel, Dictionary<object, Guid> handledObjects = null)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            if (handledObjects == null)
                handledObjects = new Dictionary<object, Guid>();
            var type = obj.GetType();
            if (parameters.typesToSerialize.ContainsKey(type))
                return parameters.typesToSerialize[type](obj) + Environment.NewLine;
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine($"{type.Name}");
            var members = GetFieldsAndProperties(type);
            var guid = GetGuid(members, obj);
            if (handledObjects.ContainsKey(obj))
            {
                sb.AppendLine($"{identation}Id = {handledObjects[obj]}");
                return sb.ToString();
            }
            handledObjects.Add(obj, guid);
            foreach (var memberInfo in members
                .Where(member => (member is PropertyInfo propertyInfo && !parameters.typesToExclude.Contains(propertyInfo.PropertyType)
                    || member is FieldInfo fieldInfo && !parameters.typesToExclude.Contains(fieldInfo.FieldType))
                                 && !parameters.membersToExclude.Contains(member)))
            {
                sb.Append(identation + memberInfo.Name + " = " +
                          (parameters.membersToSerialize.ContainsKey(memberInfo) ?
                              parameters.membersToSerialize[memberInfo](GetObject(memberInfo, obj)) + Environment.NewLine :
                          PrintToString(GetObject(memberInfo, obj),
                              nestingLevel + 1, handledObjects)));
            }
            return sb.ToString();
        }

        private object GetObject(MemberInfo member, object obj)
        {
            if (member is PropertyInfo propertyInfo)
                return propertyInfo.GetValue(obj);
            if (member is FieldInfo fieldInfo)
                return fieldInfo.GetValue(obj);
            throw new ArgumentException("Wrong member type");
        }

        private IEnumerable<MemberInfo> GetFieldsAndProperties(Type type)
        {
            foreach (var property in type.GetProperties())
                yield return property;
            foreach (var fields in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                yield return fields;
        }

        private Guid GetGuid(IEnumerable<MemberInfo> members, object obj)
        {
            var guidType = typeof(Guid);
            foreach (var m in members)
            {
                if (m is PropertyInfo property && property.PropertyType == guidType)
                    return (Guid)property.GetValue(obj);
                if (m is FieldInfo field && field.FieldType == guidType)
                    return (Guid)field.GetValue(obj);
            }
            return Guid.NewGuid();
        }
    }
}