using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.PrintingMembers
{
    public class DefaultPrintingMemberFactory : IPrintingMemberFactory
    {
        public IEnumerable<MemberTypes> SupportedTypes => new[]
        {
            MemberTypes.Field,
            MemberTypes.Property
        };

        public PrintingMember Convert(MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    var fieldInfo = (FieldInfo)memberInfo;
                    return new PrintingMember(fieldInfo.FieldType, fieldInfo.Name, fieldInfo, fieldInfo.GetValue);
                case MemberTypes.Property:
                    var propertyInfo = (PropertyInfo)memberInfo;
                    return new PrintingMember(propertyInfo.PropertyType, propertyInfo.Name, propertyInfo,
                        propertyInfo.GetValue);
                default:
                    throw new ArgumentException($"Unsupported member type: {memberInfo.MemberType}",
                        nameof(memberInfo));
            }
        }
    }
}