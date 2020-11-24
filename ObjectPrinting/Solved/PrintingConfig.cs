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
        public readonly PrintingParameters Parameters;

        public PrintingConfig()
        {
            Parameters = PrintingParameters.Default;
        }

        internal PrintingConfig(PrintingParameters parameters)
        {
            Parameters = parameters;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>
                (this, typeof(TPropType));
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
                (this, memberAccess.Member);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException();
            var memberAccess = (MemberExpression)memberSelector.Body;
            if (memberAccess.Member.MemberType != MemberTypes.Field &&
                memberAccess.Member.MemberType != MemberTypes.Property)
                throw new ArgumentException();
            return new PrintingConfig<TOwner>(Parameters.AddMemberToExclude(memberAccess.Member));
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            return new PrintingConfig<TOwner>(Parameters.AddTypeToExclude(typeof(TPropType)));
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel, int id = 0, Dictionary<object, int> handledObjects = null)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            if (handledObjects == null)
                handledObjects = new Dictionary<object, int>();
            var type = obj.GetType();
            if (Parameters.typesToSerialize.ContainsKey(type))
                return Parameters.typesToSerialize[type](obj) + Environment.NewLine;
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var members = GetFieldsAndProperties(type);
            if (handledObjects.ContainsKey(obj))
            {
                sb.AppendLine($"{type.Name} Id = {handledObjects[obj]}");
                return sb.ToString();
            }
            sb.AppendLine($"{type.Name} Id = {id}");
            handledObjects.Add(obj, id);
            foreach (var memberInfo in members
                .Where(member => (member is PropertyInfo propertyInfo && !Parameters.typesToExclude.Contains(propertyInfo.PropertyType)
                    || member is FieldInfo fieldInfo && !Parameters.typesToExclude.Contains(fieldInfo.FieldType))
                                 && !Parameters.membersToExclude.Contains(member)))
            {
                sb.Append(identation + memberInfo.Name + " = " +
                          (Parameters.membersToSerialize.ContainsKey(memberInfo) ?
                              Parameters.membersToSerialize[memberInfo](GetObject(memberInfo, obj)) + Environment.NewLine :
                          PrintToString(GetObject(memberInfo, obj),
                              nestingLevel + 1, id + 1, handledObjects)));
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
            foreach (var member in members)
            {
                if (member is PropertyInfo property && property.PropertyType == guidType)
                    return (Guid)property.GetValue(obj)!;
                if (member is FieldInfo field && field.FieldType == guidType)
                    return (Guid)field.GetValue(obj)!;
            }
            return Guid.NewGuid();
        }
    }
}